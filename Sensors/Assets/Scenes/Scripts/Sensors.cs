/*
    Authors   : Sumanth Paranjape, Aneesh Dalvi
    Function : Implements Inheritance where Base class is Sensor
               and derived classes are ProximitySensor | RangeSensor | 
               LidarSensor | RadarSensor and notifies clients using 
               Observer Design Pattern.
    Version  : V1
    Email    : sparanj2@asu.edu | Arizona State University.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Sensors : MonoBehaviour
{
    protected static Sensors sensors;
    protected static GameObject Cube;    
    protected float Distance;
    protected float oldTime;
    protected float newTime;
    protected float newVelocity;
    protected float oldVelocity;

    [Header("Sensors")]
    protected float sensorLength;
    protected float sideSensorPos;
    protected float frontSensorAngle = 45;

    protected static int sensorType;

    protected static int[,] obstacle_matrix;

    //Interface for updating matrix     
    public virtual void update_Obstacles(GameObject gameObj){

    }

    public void update(){
        sensors.update_Obstacles(Cube);
    }

    
    //Singleton Pattern. Only one instance allowed.
    protected Sensors(){
    }

    public int[,] get_Obstacle_Matrix(){
        return obstacle_matrix;
    }

    public void setSensorType(int value)
    {
        sensorType = value;
    }

    protected int getSensorType()
    {
        return sensorType;
    }

    protected void testProximityMatrix(int[,] matrix)
    {
        Debug.Log("-- Printing Matrix : -- ");
        
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Debug.Log(matrix.GetValue(i,j));
            }
            Debug.Log("-- row done -- ");
        }
        Debug.Log("-- Printing Matrix done -- ");
    }

    protected void DrawCircle(Vector3 position, float radius, Color color)
    {
        var increment = 10;
        for (int angle = 0; angle < 360; angle = angle + increment)
        {
            var heading = Vector3.forward - position;
            var direction = heading / heading.magnitude;
            var point = position + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
            var point2 = position + Quaternion.Euler(0, angle + increment, 0) * Vector3.forward * radius;
            Debug.DrawLine(point, point2, color);
        }
    }

    protected void checkObstacle(Vector3 origin, Vector3 direction, GameObject gObj, 
                               int angle, String obstacleToBeFound, int[] outRangeIndexes, 
                               int[] inRangeIndexes)
    {
        RaycastHit hit;
        Ray ray;

        if (angle != 0)
        {
            ray = new Ray(origin, direction);
        }
        else // for 0
        {
            ray = new Ray(origin, gObj.transform.TransformDirection(direction));
        }
        

        int result = 1;

        if (Physics.Raycast(ray, out hit, sensorLength))
        {
            Debug.Log("found " + obstacleToBeFound + " obstacle");

            if (getSensorType() == 1)
            {
                drawRayOnRover(ray, hit, "");
                obstacle_matrix[outRangeIndexes[0], outRangeIndexes[1]] = result;
            }
            else
            {
                // for LiDAR and Radar
                if (getSensorType() == 3 || getSensorType() == 4) result = (int)hit.distance;
                
                if (hit.distance > 2.0f)
                {
                    drawRayOnRover(ray, hit, "out");
                    obstacle_matrix[outRangeIndexes[0], outRangeIndexes[1]] = result;
                }
                else
                {
                    drawRayOnRover(ray, hit, "in");
                    obstacle_matrix[inRangeIndexes[0], inRangeIndexes[1]] = result;
                }
            }
        }

    }

    /* Function description for Drawing Ray on Rover. */
    protected void drawRayOnRover(Ray ray, RaycastHit hit, String range)
    {
        if (getSensorType() == 3) // LiDAR
        {
            if (range.Equals("out"))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }
            else
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }
        }
        else if (getSensorType() == 4) // Radar
        {
            if (range.Equals("out"))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.blue);
            }
            else
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }
        }
        else if (getSensorType() == 2) // Range
        {
            if (range.Equals("out"))
            {
                Debug.DrawLine(ray.origin, hit.point);
            }
            else
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }
        }
        else  // Proximity
        {
            Debug.DrawLine(ray.origin, hit.point);
        }

    }

}