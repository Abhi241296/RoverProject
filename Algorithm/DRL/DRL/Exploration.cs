﻿using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using DRL;
using RandomSystem = System.Random;

namespace Algorithms
{
    /*Main Class for Exploration.*/
    public class Exploration
    {
        private List<String> commands = new List<string>() {"West", "North", "East", "South"};

        private List<Vector2Int> vectorCommands = new List<Vector2Int>()
            {Vector2Int.down, Vector2Int.left, Vector2Int.up, Vector2Int.right};

        private String _direction = "East";
        private int _points, _rows, _cols;
        public ExploredMap exploredMap;
        FeedForwardManager forwardManager = new FeedForwardManager();

        private enum AlgorithmType
        {
            BackPropagation = 0,
            FeedForward = 1,
            DeepLearning = 2,
            RandomDirection = 3
        }

        private String[] _sensorTypeString =
        {
            "Proximity",
            "Range",
            "Lidar",
            "Radar",
            "Bumper"
        };

        private enum ExperimentType
        {
            Training = 0,
            Testing = 1,
        }

        public Exploration(int rows, int cols)
        {
            this._rows = rows;
            this._cols = cols;
            exploredMap = new ExploredMap(new Vector2Int(rows, cols), new Vector2Int(1, 1));
        }

        /*Returns the next command for the robot
        @param SensorData - Used to compute the next command*/
        public string GetNextCommand(int[,] sensorData, int sensorType, int algorithmType, int experimentType)
        {
            if (experimentType == (int) ExperimentType.Training)
            {
                Debug.Log("Starting Training");
                TrainNeuralNetworks(algorithmType, sensorType);
                return "";
            }

            exploredMap.ProcessSensor(sensorData);
            String robotCommand;
            switch (algorithmType)
            {
                case (int) AlgorithmType.BackPropagation:
                    robotCommand = GetCommandFromBackPropagation(sensorData, sensorType);
                    break;
                case (int) AlgorithmType.RandomDirection:
                    if (sensorType == 4)
                    {
                        robotCommand = GetCommandFromRandomDirectionAlgorithmBumper(sensorData);
                        if(robotCommand[0]!='R')
                            ManagePoints(vectorCommands[commands.IndexOf(robotCommand)]);
                        return robotCommand;
                    }
                    robotCommand = GetCommandFromRandomDirectionAlgorithm(sensorData);
                    Debug.Log("Not Break");
                    break;
                case (int) AlgorithmType.FeedForward:
                    robotCommand = GetCommandFromFeedForward(sensorData, sensorType);
                    break;
                default:
                    robotCommand = GetCommandFromRandomDirectionAlgorithm(sensorData);
                    Debug.Log("Unknown Algorithm Type: Switched to Random Direction Algorithm");
                    break;
            }

            ManagePoints(vectorCommands[commands.IndexOf(robotCommand)]);
            _direction = robotCommand;
            return robotCommand;
        }

        private void TrainNeuralNetworks(int algorithmType, int sensorType)
        {
            switch (algorithmType)
            {
                case (int) AlgorithmType.BackPropagation:
                    BackPropagation.Driver("Train", _sensorTypeString[sensorType], null);
                    break;
                case (int) AlgorithmType.FeedForward:
                    forwardManager.GetDirectionFromFeedForward(_sensorTypeString[sensorType], new float[] { }, 0);
                    break;
            }
        }

        String GetCommandFromBackPropagation(int[,] sensorData, int sensorType)
        {
            int[,] processedSensorData = GetProcessedSensorData(sensorData);
            var robotCommand =
                BackPropagation.Driver("Command", _sensorTypeString[sensorType],
                    convertToOneDimensionalDouble(processedSensorData));
            return robotCommand;
        }

        String GetCommandFromFeedForward(int[,] sensorData, int sensorType)
        {
            int[,] processedSensorData = GetProcessedSensorData(sensorData);
            var robotCommand =
                forwardManager.GetDirectionFromFeedForward(_sensorTypeString[sensorType],
                    ConvertToOneDimensionalFloat(processedSensorData), 1);
            return robotCommand;
        }

        String GetCommandFromRandomDirectionAlgorithm(int[,] sensorData)
        {
            RandomSystem r = new RandomSystem();
            List<String> possibleDirections = GetAvailableDirections(sensorData);
            int x = r.Next(0, possibleDirections.Count);
            var robotCommand = possibleDirections[x];
            return robotCommand;
        }

        String GetCommandFromRandomDirectionAlgorithmBumper(int[,] sensorData)
        {
            List<string> possibleDirections = new List<string>();
            if (sensorData[0, 1] == 0)
            {
                possibleDirections.Add("North");
            }
            else if (sensorData[1, 2] == 0)
            {
                possibleDirections.Add("East");
            }
            else if (sensorData[1, 0] == 0)
            {
                possibleDirections.Add("West");
            }
            else if (sensorData[2, 1] == 0)
            {
                possibleDirections.Add("South");
            }
            
            else if (sensorData[0, 1] == 1)
            {
                possibleDirections.Add("REast");
                possibleDirections.Add("RWest");
            }
            else if (sensorData[1, 2] == 1)
            {
                possibleDirections.Add("RNorth");
                possibleDirections.Add("RSouth");
            }
            else if (sensorData[1, 0] == 1)
            {
                possibleDirections.Add("RNorth");
                possibleDirections.Add("RSouth");
            }
            else if (sensorData[2, 1] == 1)
            {
                possibleDirections.Add("REast");
                possibleDirections.Add("RWest");
            }
            RandomSystem r = new RandomSystem();
            int x = r.Next(0, possibleDirections.Count);
            var robotCommand = possibleDirections[x];
            return robotCommand;
        }

        //Computes all the available directions
        public List<String> GetAvailableDirections(int[,] sensorData)
        {
            List<string> possibleDirections = new List<string>();
            Vector2Int robotPosition = exploredMap.GetCurrentPosition();
            //Checking North Cell
            Vector2Int mazeCellPosition = new Vector2Int(robotPosition.x - 1, robotPosition.y);
            MazeCell mazeCell = exploredMap.GetCell(mazeCellPosition);
            if (mazeCell != null)
                if (!mazeCell.IsWallCell() && !mazeCell.IsVisited())
                    possibleDirections.Add("North");
            //Checking East Cell
            mazeCellPosition = new Vector2Int(robotPosition.x, robotPosition.y + 1);
            mazeCell = exploredMap.GetCell(mazeCellPosition);
            if (mazeCell != null)
                if (!mazeCell.IsWallCell() && !mazeCell.IsVisited())
                    possibleDirections.Add("East");
            //Checking South Cell
            mazeCellPosition = new Vector2Int(robotPosition.x + 1, robotPosition.y);
            mazeCell = exploredMap.GetCell(mazeCellPosition);
            if (mazeCell != null)
                if (!mazeCell.IsWallCell() && !mazeCell.IsVisited())
                    possibleDirections.Add("South");
            //Checking West Cell
            mazeCellPosition = new Vector2Int(robotPosition.x, robotPosition.y - 1);
            mazeCell = exploredMap.GetCell(mazeCellPosition);
            if (mazeCell != null)
                if (!mazeCell.IsWallCell() && !mazeCell.IsVisited())
                    possibleDirections.Add("West");
            if (possibleDirections.Count != 0) return possibleDirections;
            mazeCellPosition = new Vector2Int(robotPosition.x - 1, robotPosition.y);
            mazeCell = exploredMap.GetCell(mazeCellPosition);
            if (mazeCell != null)
                if (!mazeCell.IsWallCell())
                    possibleDirections.Add("North");
            mazeCellPosition = new Vector2Int(robotPosition.x, robotPosition.y + 1);
            mazeCell = exploredMap.GetCell(mazeCellPosition);
            if (mazeCell != null)
                if (!mazeCell.IsWallCell())
                    possibleDirections.Add("East");
            mazeCellPosition = new Vector2Int(robotPosition.x + 1, robotPosition.y);
            mazeCell = exploredMap.GetCell(mazeCellPosition);
            if (mazeCell != null)
                if (!mazeCell.IsWallCell())
                    possibleDirections.Add("South");
            mazeCellPosition = new Vector2Int(robotPosition.x, robotPosition.y - 1);
            mazeCell = exploredMap.GetCell(mazeCellPosition);
            if (mazeCell == null) return possibleDirections;
            if (!mazeCell.IsWallCell())
                possibleDirections.Add("West");
            return possibleDirections;
        }

        //Save movement and sensor data to csv file
        public void GenerateDataset(int[,] sensorData, string resultDirection, int sensorType)
        {
            int[,] dataToBeSaved = sensorData;
            if (sensorType == 3)
                sensorData = RotateSensorData(sensorData, _direction);
            exploredMap.ProcessSensor(sensorData);
            var robotCommand = resultDirection;
            WriteSensorDataToCsv(dataToBeSaved, robotCommand, sensorType);
            _direction = robotCommand;
        }

        //Calculates the points of the robot
        public void ManagePoints(Vector2Int direction)
        {
            var futurePosition = GetExploredMap().GetCurrentPosition() + direction;
            if (exploredMap.GetCell(futurePosition).IsVisited() == false)
            {
                _points += 10;
            }
            else
            {
                _points -= 2;
            }
        }

        //Returns the total coverage of the robot.
        public int GetCoverage()
        {
            float coverage = 0, total = _cols * _rows;
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    if (exploredMap.GetCell(new Vector2Int(i, j)) != null)
                        coverage += 1;
                }
            }

            coverage = coverage / total * 100;
            return (int) coverage;
        }

        public int GetPoints()
        {
            return _points;
        }

        public ExploredMap GetExploredMap()
        {
            return exploredMap;
        }

        //Returns the sensor data with the information about the visited cells
        public int[,] GetProcessedSensorData(int[,] sensorData)
        {
            int[,] processedSensorData = sensorData;
            Vector2Int localRobotPosition = exploredMap.GetSensorRobotPosition(sensorData);
            Vector2Int robotPosition = exploredMap._robotPosition;
            for (var x = 0; x < processedSensorData.GetLength(0); x++)
            for (var y = 0; y < processedSensorData.GetLength(1); y++)
            {
                var xMaze = robotPosition.x + x - localRobotPosition.x;
                var yMaze = robotPosition.y + y - localRobotPosition.y;
                if (exploredMap.GetCell(new Vector2Int(xMaze, yMaze)) != null &&
                    exploredMap.GetCell(new Vector2Int(xMaze, yMaze)).IsVisited())
                    processedSensorData[x, y] = 4;
            }

            processedSensorData[localRobotPosition.x, localRobotPosition.y] = 2;
            return processedSensorData;
        }

        //Converts int[,] to int[]
        public double[] convertToOneDimensionalDouble(int[,] array)
        {
            double[] result = new double[array.GetLength(0) * array.GetLength(1)];
            int i = 0;
            foreach (int variable in array)
            {
                result[i++] = Convert.ToDouble(variable);
            }

            return result;
        }

        //Converts int[,] to float[]
        public float[] ConvertToOneDimensionalFloat(int[,] array)
        {
            float[] result = new float[array.GetLength(0) * array.GetLength(1)];
            int i = 0;
            foreach (int variable in array)
            {
                result[i++] = variable;
            }

            return result;
        }

        //Prints the sensorData array
        public static void PrintSensorData(int[,] sensorData)
        {
            string sensorDataString = "";
            for (var i = 0; i < sensorData.GetLength(0); i++)
            {
                for (var j = 0; j < sensorData.GetLength(1); j++)
                {
                    sensorDataString += sensorData[i, j] + " ";
                }

                sensorDataString += " ,,, ";
            }

            Debug.Log(sensorDataString);
        }

        // Rotates the array clockwise and returns the rotated array
        // @param direction - affects the number of times that array will be rotated
        public static int[,] RotateSensorData(int[,] sensorData, string direction)
        {
            int counter = 0;
            switch (direction)
            {
                case "West":
                    counter = 3;
                    break;
                case "South":
                    counter = 2;
                    break;
                case "East":
                    counter = 1;
                    break;
            }

            int[,] output = sensorData;
            for (int n = 0; n < counter; n++)
            {
                int cols = sensorData.GetLength(0);
                int rows = sensorData.GetLength(1);
                output = new int [rows, cols];
                for (int i = 0; i < cols; i++)
                for (int j = 0; j < rows; j++)
                    output[j, cols - 1 - i] = sensorData[i, j];
                sensorData = output;
            }

            return output;
        }

        //Write sensor data and the given direction to csv file
        public void WriteSensorDataToCsv(int[,] sensorData, string direction, int sensorType)
        {
            int[,] dataToBeSaved = sensorData;
            Vector2Int localRobotPosition = exploredMap.GetSensorRobotPosition(sensorData);
            Vector2Int robotPosition = exploredMap._robotPosition;
            for (var x = 0; x < dataToBeSaved.GetLength(0); x++)
            for (var y = 0; y < dataToBeSaved.GetLength(1); y++)
            {
                var xMaze = robotPosition.x + x - localRobotPosition.x;
                var yMaze = robotPosition.y + y - localRobotPosition.y;
                if (exploredMap.GetCell(new Vector2Int(xMaze, yMaze)) != null &&
                    exploredMap.GetCell(new Vector2Int(xMaze, yMaze)).IsVisited())
                    dataToBeSaved[x, y] = 4;
                dataToBeSaved[localRobotPosition.x, localRobotPosition.y] = 2;
            }

            var path = Directory.GetCurrentDirectory();
            string filePath = "";
            filePath = path + "/Datasets/" + _sensorTypeString[sensorType] + ".csv";
            foreach (var item in sensorData)
            {
                File.AppendAllText(filePath, item.ToString(), Encoding.UTF8);
                File.AppendAllText(filePath, ",", Encoding.UTF8);
            }

            File.AppendAllText(filePath, direction + ",", Encoding.UTF8);
            File.AppendAllText(filePath, Environment.NewLine, Encoding.UTF8);
        }
    }
}