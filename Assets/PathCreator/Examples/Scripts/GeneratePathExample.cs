using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace PathCreation.Examples {

    //Modified version of the PathCreation script
    // Example of creating a path at runtime from a set of points.
    //The number of waypoints can be set by user to make a longer or shorter path. From all the generated waypoints a number is selected at random
    // and the bezier curve path generator is called on these points 

    [RequireComponent(typeof(PathCreator))]
    public class GeneratePathExample : MonoBehaviour {

        public bool closedLoop = true;
        //public Transform[] waypoints;

        [SerializeField] int maxWaypoints = 5;

        void Start () {

            List<Transform> waypointObjs = GenerateGridWaypoints.allWayPoints;

            List<int> selectedInds = new List<int>();

            List<Transform> selectedWaypints = new List<Transform>();

            int numWaypoints = 0;
            while (numWaypoints <= maxWaypoints)
            {
                int currIndex = Random.Range(0, waypointObjs.Count);

                if (!selectedInds.Contains(currIndex))
                {
                    selectedInds.Add(currIndex);

                    selectedWaypints.Add(waypointObjs[currIndex]);

                    numWaypoints+=1;

                    waypointObjs[currIndex].GetComponent<Renderer>().material.color = new Color(1,0,0);
                }


            }

            if (selectedWaypints.Count > 0)
            {
                // Create a new bezier path from the waypoints.
                BezierPath bezierPath = new BezierPath(selectedWaypints, closedLoop, PathSpace.xyz);
                GetComponent<PathCreator>().bezierPath = bezierPath;
            }
        }
    }
}