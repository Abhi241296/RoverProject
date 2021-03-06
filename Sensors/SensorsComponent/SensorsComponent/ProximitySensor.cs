﻿using UnityEngine;

namespace SensorsComponent
{
    class ProximitySensor : Sensors
    {        
        public ProximitySensor()
        {
            obstacle_matrix = new int[,] {  { -1,  0, -1 }, 
                                            {  0,  2,  0 }, 
                                            { -1,  0, -1 } };
            sensorLength = 2f;            
        }

        public override void Update_Obstacles(GameObject gObj, int[,] mazeData, string Direction)
        {
            base.Update_Obstacles(gObj, mazeData, Direction);
            obstacle_matrix = new int[,] { { -1, 0, -1 }, { 0, 2, 0 }, { -1, 0, -1 } };

            Update_proximity_matrix(mazeData);

            //Checks obstacles in 4 directions.
            CheckObstacle(gObj.transform.position,
                          Vector3.forward,
                          gObj, 0, "Front",
                          new int[] { 0, 1 },
                          new int[] { 0, 1 });

            CheckObstacle(gObj.transform.position,
                          Vector3.right,
                          gObj, 0, "Right",
                          new int[] { 1, 2 },
                          new int[] { 1, 2 });

            CheckObstacle(gObj.transform.position,
                          -Vector3.right,
                          gObj, 0, "Left",
                          new int[] { 1, 0 },
                          new int[] { 1, 0 });

            CheckObstacle(gObj.transform.position,
                          -Vector3.forward,
                          gObj, 0, "Back",
                          new int[] { 2, 1 },
                          new int[] { 2, 1 });
        }
    }
}
