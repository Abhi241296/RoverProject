﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentalDesignDatabase;
using Mono.Data.Sqlite;


public class database : MonoBehaviour
{
    public SqliteConnection dbConnection;


    // Start is called before the first frame update
    void Start()
    {
        try
        {
            string filePath = Application.streamingAssetsPath + "/" + "ExperimentalDatabase.db";
            dbConnection = new SqliteConnection("Data Source = " + filePath);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void Insert(string algorithm, string mazetype, double thresholdvalue, string sensor, string experimentType)
    {

        ExperimentalDesignDb expdb = new ExperimentalDesignDb();

        Start();

        expdb.Insert(dbConnection,"INSERT INTO experimental_results(AlgorithmType, MazeSize, ThresholdFrequency,SensorType,ExperimentType) VALUES ('" + algorithm + "','" + mazetype + "'," + thresholdvalue + ",'" + sensor + "','"+ experimentType+"');");


    }

    public List<float> selectValuesfromDB(string yAxisValue, string InputAlgorithmValue, string MazeSizeValue, float Threshold, string SensorTypeValue)
    {
        ExperimentalDesignDb expdb = new ExperimentalDesignDb();
        Start();
        TestSuiteDatabase tsd = new TestSuiteDatabase();
        tsd.TestSelectValuesfromDB(yAxisValue);
        List<float> range= new List<float>();
            range = expdb.Select(dbConnection, yAxisValue, InputAlgorithmValue, MazeSizeValue, Threshold, SensorTypeValue);
            return range;

    }

    public float minvalue(string yAxisValue, string InputAlgorithmValue, string MazeSizeValue, float Threshold, string SensorTypeValue)
    {
        ExperimentalDesignDb expdb = new ExperimentalDesignDb();
        Start();
        TestSuiteDatabase tsd = new TestSuiteDatabase();
        tsd.TestSelectValuesfromDB(yAxisValue);
        float range = expdb.minimumvalue(dbConnection,yAxisValue, InputAlgorithmValue, MazeSizeValue, Threshold, SensorTypeValue);
        print(range);
        return range;

    }
    public float maxvalue(string yAxisValue, string InputAlgorithmValue, string MazeSizeValue, float Threshold, string SensorTypeValue)
    {
        ExperimentalDesignDb expdb = new ExperimentalDesignDb();
        Start();
        TestSuiteDatabase tsd = new TestSuiteDatabase();
        tsd.TestSelectValuesfromDB(yAxisValue);
        float range = expdb.maximumvalue(dbConnection,yAxisValue, InputAlgorithmValue, MazeSizeValue, Threshold, SensorTypeValue);
        return range;

    }
    public float averagevalue(string yAxisValue, string InputAlgorithmValue, string MazeSizeValue, float Threshold, string SensorTypeValue)
    {
        ExperimentalDesignDb expdb = new ExperimentalDesignDb();
        Start();
        TestSuiteDatabase tsd = new TestSuiteDatabase();
        tsd.TestSelectValuesfromDB(yAxisValue);
        float range = expdb.averagevalue(dbConnection,yAxisValue, InputAlgorithmValue, MazeSizeValue, Threshold, SensorTypeValue);
        return range;

    }
    // Update is called once per frame

    public int UpdateTimeTaken(float TimeTaken)
    {
        int statusCode = 0;
        try
        {
            ExperimentalDesignDb expdb = new ExperimentalDesignDb();
            Start();
            TestSuiteDatabase tsd = new TestSuiteDatabase();
            if (tsd.testUpdateTimeTaken(dbConnection, "SELECT COUNT(ID) FROM experimental_results where TimeTaken = 0 ;"))
            {
                expdb.Update(dbConnection, "UPDATE experimental_results SET TimeTaken='" + TimeTaken + "' WHERE ID IN (SELECT Max(ID) FROM experimental_results);");

            statusCode = 200;

        }
        else
        {
            statusCode = 400;
        }

    }
        catch (SqliteException sqlEx)
        {
            statusCode = 400;
            Debug.LogError(sqlEx);
        }

        return statusCode;

    }

    public int UpdatePointsScored(float PointsScored)
    {
        int statusCode = 0;
        try
        {
            ExperimentalDesignDb expdb = new ExperimentalDesignDb();
            Start();
            TestSuiteDatabase tsd = new TestSuiteDatabase();
            if (tsd.testUpdatePointsScored(dbConnection, "SELECT COUNT(ID) FROM experimental_results where TimeTaken = 0 ;"))
            {
                expdb.Update(dbConnection, "UPDATE experimental_results SET PointsScored='" + PointsScored + "' WHERE ID IN (SELECT Max(ID) FROM experimental_results);");
                statusCode = 200;

            }
            else
            {
                statusCode = 400;
            }

        }
        catch (SqliteException sqlEx)
        {
            statusCode = 400;
            Debug.LogError(sqlEx);
        }

        return statusCode;

    }

    public int UpdateMazeCoverage(float MazeCoverage)
    {
        int statusCode = 0;
        try
        {
            ExperimentalDesignDb expdb = new ExperimentalDesignDb();
            Start();
            TestSuiteDatabase tsd = new TestSuiteDatabase();
            if (tsd.testUpdateMazeCoverage(dbConnection, "SELECT COUNT(ID) FROM experimental_results where TimeTaken = 0 ;"))
            {
                expdb.Update(dbConnection, "UPDATE experimental_results SET MazeCoverage='" + MazeCoverage + "' WHERE ID IN (SELECT Max(ID) FROM experimental_results);");
                statusCode = 200;
            }
            else
            {
                statusCode = 400;
            }
        }
        catch (SqliteException sqlEx)
        {
            statusCode = 400;
            Debug.LogError(sqlEx);
        }

        return statusCode;

    }

    public int UpdateDroneLife(float DroneLife)
    {
        int statusCode = 0;
        try
        {
         ExperimentalDesignDb expdb = new ExperimentalDesignDb();
        Start();
            TestSuiteDatabase tsd = new TestSuiteDatabase();
            if (tsd.testUpdateMazeCoverage(dbConnection, "SELECT COUNT(ID) FROM experimental_results where TimeTaken = 0 ;"))
            {
                expdb.Update(dbConnection,"UPDATE experimental_results SET DroneLife=" + DroneLife + " WHERE ID IN (SELECT Max(ID) FROM experimental_results);");
                statusCode = 200;
            }
            else
            {
                statusCode = 400;
            }
        }
        catch (SqliteException sqlEx)
        {
            statusCode = 400;
            Debug.LogError(sqlEx);
        }

        return statusCode;

    }
}