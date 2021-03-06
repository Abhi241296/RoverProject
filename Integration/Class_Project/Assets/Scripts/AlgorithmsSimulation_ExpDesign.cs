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
    public Text droneLifeText, pointsText, timeTakenText, mazeCoverageText;
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
        Debug.Log(expCounter+">>>>");
        currentSensor = PlayerPrefs.GetInt("SensorType");
        int expc = expCounter + 1;
        experimentText.text = "Experiment " + ExperimentalID + " is running";
        statusText.text = "Trial " + expc  + " is running.";
        sensor = SensorsComponent.SensorFactory.GetInstance(currentSensor, robotPrefab);        

        createMazeButtonListener();

        if (PlayerPrefs.GetString("ExperimentTypevalue") == "Training Data"){
            exploration.GetNextCommand(null, currentSensor-1, currentAlgo, 0);
            Debug.Log("Training Complete "+expCounter);
        }
        else{
            Debug.Log(sensor.GetCurrentSensor());
            // manualButton.onClick.AddListener(manualButtonListener);
            automaticButtonListener();
            backButton.interactable = false;

        }

        expCounter++;
        if(PlayerPrefs.GetString("ExperimentTypevalue") == "Training Data" && expCounter<PlayerPrefs.GetInt("Iteration")){
            Begin();
        }



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
            mazeCreated = true;
        }
        Debug.Log(PlayerPrefs.GetString("ExperimentTypevalue"));
        expDB.Insert(convertMatrixToString(), PlayerPrefs.GetString("Algo"), PlayerPrefs.GetString("Size"), Math.Round(float.Parse(PlayerPrefs.GetString("Maze")), 2), PlayerPrefs.GetString("Sensor"), PlayerPrefs.GetString("ExperimentTypevalue"), ExperimentalID);
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
        if (PlayerPrefs.GetString("ExperimentTypevalue") == "Training Data")
        {
            ExperimentalID = expDB.get() + 1;
        }
        else
        {
            ExperimentalID = expDB.get();
        }
    }

    private String convertMatrixToString(){
        string str = "";
        for (int i = 0; i <= maze.GetUpperBound(0); i++)
        {
            str += "";
            for (int j = 0; j <= maze.GetUpperBound(1); j++)
            {
                str += maze[i, j];
                if (j != maze.GetUpperBound(1))
                {
                    str += ",";
                }
            }
            str += "";
            if (i != maze.GetUpperBound(0))
            {
                str += ";";
            }
        }
        str += ""; 
            return str;
    }

    private void UpdateParameters()
    {
        // PlayerPrefs.GetInt("Experiment");
        currentAlgo = PlayerPrefs.GetInt("AlgoSelected");
        placementThreshold = float.Parse(GameObject.Find("MazeButton").GetComponentInChildren<Text>().text);
        String[] size = GameObject.Find("SizeButton").GetComponentInChildren<Text>().text.ToString().Split('X');
        mazeWidth = Int32.Parse(size[0].Trim());
        mazeHeight = Int32.Parse(size[1].Trim());
    }

    //returns the sensor data and update it on the screen
    int[,] getSensorData(){
        int matrixSize = (currentSensor==1 || currentSensor == 5)? 3:5;
        sensor.Update_Obstacles(robotMain, getMazeData(matrixSize), robotDirection);            
        int[,] sensorReading = sensor.Get_Obstacle_Matrix();
        if(currentSensor == 3)
            sensorReading = Exploration.RotateSensorData(sensorReading,robotDirection);
        updateUISensorData(sensorReading);
        return sensorReading;   
    }

    void getNextCommand()
    {
                updateMaze();
        healthBar.value = batteryLife;
        mazeCoverageStr = exploration.GetCoverage().ToString();
        pointsScoredStr = exploration.GetPoints().ToString();
        mazeCoverage = Int32.Parse(mazeCoverageStr);
        pointsScored = Int32.Parse(pointsScoredStr);
        if(checkRunTimeStatus()){
            int [,] sensorReading = getSensorData();
            String robotCommand = exploration.GetNextCommand(sensorReading, currentSensor-1, currentAlgo, PlayerPrefs.GetInt("Experiment"));
            if(robotCommand != ""){
                if(robotCommand=="North"|robotCommand=="East"|robotCommand=="West"|robotCommand=="South")
                    robotDirection = robotCommand;
                moveInDirection(robotCommand);
            }
            mazeCoverageText.text="Maze Coverage: "+mazeCoverageStr;
            pointsText.text="Points: "+pointsScoredStr;
            timeTakenText.text="Time: "+runTime;
            droneLifeText.text="Life: "+batteryLife;
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
                statusText.text = "Trials are completed.";
            }
        }
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
                Vector3 tempVector = new Vector3(xStart + (xSpace * j), 1000, yStart - (ySpace * i));
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
        if(currentSensor == 3 && robotDirection == "South")
            sensorData.text+="\n\n";
        for (int i= 0; i <tempData.GetLength(0); i++){
            if(currentSensor == 3 && robotDirection == "East")
                sensorData.text+="    ";

            for (int j= 0; j < tempData.GetLength(1); j++){
                if(tempData[i, j] == -1)
                {
                    sensorData.text += "• ";
                }                    
                else
                {                    
                    sensorData.text += tempData[i, j] + " ";
                }     
            }
            if(currentSensor == 3 && robotDirection == "West")
                sensorData.text+="    ";
            sensorData.text += "\n";
        }
        if(currentSensor == 3 && robotDirection == "North")
            sensorData.text+="\n\n";
    }

    void moveInDirection(string direction)
    {
        batteryLife--;
        exploredMaze = exploration.GetExploredMap();
        if (direction== "North")
        {
            if (maze[currentX - 1, currentY +0 ]== 1) return;
            exploredMaze.MoveRelative(Vector2Int.left);
            move(-1, 0);
            robot.transform.Rotate(0.0f, 270f, 0.0f, Space.Self);
        }
        else if (direction== "East")
        {
            if (maze[currentX, currentY +1 ]== 1) return;
            exploredMaze.MoveRelative(Vector2Int.up);
            move(0, 1);
            robot.transform.Rotate(0.0f, 0f, 0.0f, Space.Self);
        }
        else if (direction== "West")
        {
            if (maze[currentX, currentY -1 ]== 1) return;
            exploredMaze.MoveRelative(Vector2Int.down);
            move(0, -1);
            robot.transform.Rotate(0.0f, -180.0f, 0.0f, Space.Self);
        }
        else if (direction== "South"){
            if (maze[currentX + 1, currentY +0 ]== 1) return;
            exploredMaze.MoveRelative(Vector2Int.right);
            move(1, 0);
            robot.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
        }
        else if (direction== "RNorth"){
            robotDirection = "North";
            robot.transform.Rotate(0.0f, 270f, 0.0f, Space.Self);
        }
        else if (direction== "RSouth"){
            robotDirection = "South";
            robot.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
        }
        else if (direction== "REast"){
            robotDirection = "East";
            robot.transform.Rotate(0.0f, 0.0f, 0.0f, Space.Self);
        }
        else if (direction== "RWest"){
            robotDirection = "West";
            robot.transform.Rotate(0.0f, -180.0f, 0.0f, Space.Self);
        }
    }

    void move(int x, int y)
    {
        if (maze[currentX + x, currentY + y] == 1) return;
        maze[currentX, currentY] = 4;
        currentX += x;
        currentY += y;
        maze[currentX, currentY] = 2;

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
