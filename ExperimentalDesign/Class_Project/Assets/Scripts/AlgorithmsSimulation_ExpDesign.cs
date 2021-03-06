﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using Algorithms;
using System.Collections.Generic;
//using SensorsComponent;


public class AlgorithmsSimulation_ExpDesign : MonoBehaviour
{
    GameObject robot, exploringRobot;
    float xStart = 0, yStart = 1;
    float xSpace = 3.5f, ySpace = 3.5f;
    private float placementThreshold;
    public Text sensorData;//, sensorTeamData;
    public GameObject wallPrefab, robotPrefab, floorPrefab, visitedFloorPrefab;
    public Button createMaze, backButton, manualButton, automaticButton;
    private int mazeHeight, mazeWidth;
    GameObject[]  exploredMazeObjects;
    List<GameObject[]> arrayOfGameObjects = new List<GameObject[]>();
    int arrayCounter = 0;
    int counter = 0;
    int expCounter = 0;
    int currentX=1,currentY=1;
    // string path = @"/home/lisa/new.csv";
    int[,] maze, experimentalMaze;
    ExploredMap exploredMaze;
    System.Random rand = new System.Random();
    bool mazeCreated = false;
    MazeGenerator mazeGenerator = new MazeGenerator();
    Exploration exploration;
    // Sensors1.Sensors sensor;
    private static SensorsComponent.Sensors sensor;
    private String startTime, endTime, runTime;
    private int mazeCoverage, batteryLife = 3600, pointsScored;
    private String algorithmSelected, mazeSize, sensorSelected, pointsScoredStr, mazeCoverageStr;
    private float mazeOffset = 140;
    private static database expDB;
    // private DataBaseManager dbm;
    private bool isSimulationComplete = false;
    public Slider healthBar;
    public Text statusText;
    public Text experimentText;
    int ExperimentalID = 0;
    private GameObject robotMain;    
    int currentSensor;
    int currentAlgo;
    String robotDirection = "East";

    void Start()
    {
        
        expDB = new database();
    }

    public void resetValues()
    {
        expCounter = 0;
    }
    public void Begin()
    {
        //Debug.Log("Curr Sensor: " + currentSensor);        
        currentSensor = PlayerPrefs.GetInt("SensorType");
        int expc = expCounter + 1;
        experimentText.text = "Experiment " + ExperimentalID + " is running";
        statusText.text = "Trail " + expc  + " is running.";
        sensor = SensorsComponent.SensorFactory.GetInstance(currentSensor, robotPrefab);        
        createMazeButtonListener();
        Debug.Log(sensor.GetCurrentSensor());
        // manualButton.onClick.AddListener(manualButtonListener);
        automaticButtonListener();
        backButton.interactable = false;
        
        expCounter++;
    }
    //Create the initial maze
    public void createMazeButtonListener()
    {

        UpdateParameters();
        arrayOfGameObjects.Add(new GameObject[mazeHeight * mazeWidth]);

        exploration = new Exploration(mazeHeight, mazeWidth);
        if (mazeCreated == false)
        {
            if(arrayCounter == 0)
                maze = mazeGenerator.GenerateMaze(mazeHeight, mazeWidth, placementThreshold);
            maze[currentX, currentY] = 2;
            // updateMaze();
            mazeCreated = true;
        }
    }

    void manualButtonListener(){
        getNextCommand();
    }

    void automaticButtonListener(){
        startTime = DateTime.Now.ToString(@"hh\:mm\:ss");
        InvokeRepeating("getNextCommand", 0.1f, 0.1f);
    }

    private bool checkRunTimeStatus(){
        return !(mazeCoverage >= PlayerPrefs.GetInt("MazeCoverage") || batteryLife <= 0);
    }

    private String calculateRunTime(){
        TimeSpan duration = DateTime.Parse(endTime).Subtract(DateTime.Parse(startTime));
        return duration.ToString(@"hh\:mm\:ss");
    }
    public void Expid()
    {
        expDB.get();
        if (PlayerPrefs.GetString("Experiment") == "Training Data")
        {
            ExperimentalID = expDB.get() + 1;
        }
        else
        {
            ExperimentalID = expDB.get();
        }
    }

    private void UpdateParameters()
    {
        currentSensor = PlayerPrefs.GetInt("SensorType");
        currentAlgo = PlayerPrefs.GetInt("AlgoSelected");
        placementThreshold = float.Parse(GameObject.Find("MazeButton").GetComponentInChildren<Text>().text);
        String[] size = GameObject.Find("SizeButton").GetComponentInChildren<Text>().text.ToString().Split('X');
        mazeWidth = Int32.Parse(size[0].Trim());
        mazeHeight = Int32.Parse(size[1].Trim());
        expDB.Insert(PlayerPrefs.GetString("Algo"), PlayerPrefs.GetString("Size"), Math.Round(float.Parse(PlayerPrefs.GetString("Maze")), 2), PlayerPrefs.GetString("Sensor"), PlayerPrefs.GetString("Experiment"), ExperimentalID);
    }

    void getNextCommand()
    {
        healthBar.value = batteryLife;
        mazeCoverageStr = exploration.GetCoverage().ToString();
        pointsScoredStr = exploration.GetPoints().ToString();
        mazeCoverage = Int32.Parse(mazeCoverageStr);
        pointsScored = Int32.Parse(pointsScoredStr);
        if(checkRunTimeStatus()){
            int matrixSize = (currentSensor==1 || currentSensor == 5)? 3:5;
            Debug.Log(robotMain);
            sensor.Update_Obstacles(sensor.GetRoverObject(), getMazeData(matrixSize), robotDirection);
            //updateUISensorData(getMazeData(matrixSize));
            int[,] sensorReading = sensor.Get_Obstacle_Matrix();
            updateUISensorData(sensorReading);
            String robotCommand = exploration.GetNextCommand(sensorReading, currentSensor);
            moveInDirection(robotCommand);
        }
        else{
            CancelInvoke("getNextCommand");
            isSimulationComplete = true;
            endTime = DateTime.Now.ToString(@"hh\:mm\:ss");
            runTime = calculateRunTime();
            //enable back button
            backButton.interactable = true;
            // calculate end time
            Debug.Log("END TIME : " + runTime);
            // store the info in DB
            Debug.Log("Battery Life : " + batteryLife);
            Debug.Log("Points : " + pointsScored);
            sendDateToExpDesign();
            Debug.Log("Maze Coverage : " + mazeCoverage);
            mazeCreated = false;
            currentX = 1;
            currentY = 1;
            mazeCoverage = 0;
            batteryLife = 3600;
            pointsScored = 0;
            
            GameObject[] mazeObjects = arrayOfGameObjects[arrayCounter];
            //Destroy UI
            for (int i = 0; i < mazeObjects.Length; i++)
                Destroy(mazeObjects[i]);
            arrayCounter++;
            if (expCounter < PlayerPrefs.GetInt("Iteration"))
            {
                Begin();
            }
            else
            {
                experimentText.text = "";
                statusText.text = "Trails are completed.";
            }
        }
    }

    public SensorsComponent.Sensors GetSensorsFromAlgorithmsSimulation(){
        return sensor;
    }

    public GameObject getRoverInstanceFromAlgorithmSimulation(){
        return robotMain;
    }

    private float getTimeInSeconds(String time){
        String[] timeSplit = time.Split(':');
        int timeTaken = Int32.Parse(timeSplit[0].Trim())*3600
            + Int32.Parse(timeSplit[1].Trim())*60
            + Int32.Parse(timeSplit[2].Trim());
        return (float) timeTaken;        
    }

    private void sendDateToExpDesign(){
        expDB.UpdatePointsScored((float) pointsScored);
        expDB.UpdateMazeCoverage((float) mazeCoverage);
        expDB.UpdateDroneLife((float) batteryLife);
        expDB.UpdateTimeTaken(getTimeInSeconds(runTime));
    }

    // proximity, bumper - 3x3
    // range, radar - 5x5
    // lidar 3x5
    void updateSensorMaze(int[,] sensorMatrix, int[,] matrix){
        for (int i = 0; i < 3; i++){
            for(int j = 0; j < 3; j++){
                if(sensorMatrix[i, j] != matrix[i, j]){
                    sensorMatrix[i, j] = matrix[i, j];
                }
            }
        }
    }

    //update the maze in the UI
    void updateMaze()
    {
        GameObject[] mazeObjects = arrayOfGameObjects[arrayCounter];
        //Destroy UI
        for (int i = 0; i < mazeObjects.Length; i++)
            Destroy(mazeObjects[i]);
        counter = 0;
        //Recreate UI
        for (int i = 0; i < mazeHeight; i++)
            for (int j = 0; j < mazeWidth; j++)
            {
                Vector3 tempVector = new Vector3(xStart + (xSpace * j), 0, yStart - (ySpace * i));
                if (maze[i, j] == 0)
                    mazeObjects[counter++] = Instantiate(floorPrefab, tempVector, Quaternion.identity);
                if (maze[i, j] == 1)
                    mazeObjects[counter++] = Instantiate(wallPrefab, tempVector, Quaternion.identity);
                else if (maze[i, j] == 2){
                    mazeObjects[counter] = Instantiate(robotPrefab, tempVector, Quaternion.identity);
                    robot = mazeObjects[counter++];
                    robotMain= robot;
                }
                else if (maze[i, j] == 4)
                    mazeObjects[counter++] = Instantiate(visitedFloorPrefab, tempVector, Quaternion.identity);
            }
    }

    void updateExplored()
    {
        if(mazeWidth == 50){
            mazeOffset = 190;
        }
        exploredMaze = exploration.GetExploredMap();
        for (int i = 0; i < exploredMazeObjects.Length; i++)
            Destroy(exploredMazeObjects[i]);
        counter = 0;

        // // //Recreate UI
        for (int i = 0; i < mazeHeight; i++)
            for (int j = 0; j < mazeWidth; j++)
            {
                Vector3 tempVector = new Vector3(xStart + (xSpace * j)+mazeOffset, 0, yStart - (ySpace * i));
                MazeCell mazeCell = exploredMaze.GetCell(new Vector2Int(i,j));
                if(mazeCell == null)
                    continue;
                if (mazeCell.IsWallCell() == true)
                    exploredMazeObjects[counter++] = Instantiate(wallPrefab, tempVector, Quaternion.identity);
                else if (mazeCell.IsVisited() == false)
                    exploredMazeObjects[counter++] = Instantiate(floorPrefab, tempVector, Quaternion.identity);
                else if (exploredMaze.GetCell(new Vector2Int(i,j)).IsVisited())
                    exploredMazeObjects[counter++] = Instantiate(visitedFloorPrefab, tempVector, Quaternion.identity);
            }
        Vector2Int vector = exploredMaze.GetCurrentPosition();
        Vector3 robotPosition = new Vector3(xStart + (xSpace * vector.y)+mazeOffset,0, yStart - (ySpace * vector.x));
        exploredMazeObjects[counter] = Instantiate(robotPrefab, robotPosition, Quaternion.identity);
        exploringRobot = exploredMazeObjects[counter++];
    }

    public enum CellType : int
    {
        floor,
        wall,
        robot,
        endPoint,
        visitedFloor
    }

    //Update sensor data on UI
    void updateUISensorData(int[,] tempData)
    {
        sensorData.text= "";
        for (int i= 0; i <tempData.GetLength(0); i++){
            for (int j= 0; j < tempData.GetLength(1); j++){
                if(tempData[i, j]==-1)
                    sensorData.text += "  ";
                else
                    sensorData.text += tempData[i, j] + " ";
            }
            sensorData.text += "\n";
        }
    }

    void moveInDirection(string direction)
    {
        batteryLife--;
        if (direction== "North")
        {
            if (maze[currentX - 1, currentY +0 ]== 1) return;
            move(-1, 0);
            //robot.transform.Rotate(0.0f, 270f, 0.0f, Space.Self);
            // exploringRobot.transform.Rotate(0.0f, 270.0f, 0.0f, Space.Self);
        }
        else if (direction== "East")
        {
            if (maze[currentX, currentY +1 ]== 1) return;
            move(0, 1);
            //robot.transform.Rotate(0.0f, 0f, 0.0f, Space.Self);
            // exploringRobot.transform.Rotate(0.0f, 0.0f, 0.0f, Space.Self);
        }
        else if (direction== "West")
        {
            if (maze[currentX, currentY -1 ]== 1) return;
            move(0, -1);
            //robot.transform.Rotate(0.0f, -180.0f, 0.0f, Space.Self);
            // exploringRobot.transform.Rotate(0.0f, -180.0f, 0.0f, Space.Self);
        }
        else if (direction== "South"){
            if (maze[currentX + 1, currentY +0 ]== 1) return;
            move(1, 0);
            //robot.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
            // exploringRobot.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
        }
    }

    void move(int x, int y)
    {
        if (maze[currentX + x, currentY + y] == 1) return;
        maze[currentX, currentY] = 4;
        currentX += x;
        currentY += y;
        maze[currentX, currentY] = 2;
        // updateMaze();
        // updateExplored();
    }

    public bool getIsSimulationComplete(){
        return isSimulationComplete;
    }

    //get maze data around robot with a diameter ${size
    int[,] getMazeData(int size)
    {
        int[,] result= new int[size, size];
        for (int i= 0; i < size; i++)
            for (int j= 0; j < size; j++)
                result[i,j] = -1;

        int position = (size==3)?1:2;
        for (int i= 0; i < size; i++)
            for (int j= 0; j < size; j++){
                int x= currentX - position + i;
                int y= currentY - position + j;
                if(x<0 || x>=mazeHeight || y<0 || y>=mazeWidth)
                    continue;
                if (maze[x,y]== 1)
                    result[i, j]= 1;
                else
                    result[i, j]= 0;
            }
        result[position,position]=2;
        return result;
    }

    public void changeScene(String sceneName)
    {
        Application.LoadLevel(sceneName);
    }
}
