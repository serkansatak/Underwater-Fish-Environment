using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Class that generates a grid of waypoints in front of the camera. Users can specify the space between the points as well as a vector3 that gives the size of the grid in x,y,z
public class GenerateGridWaypoints : MonoBehaviour
{
    

    public static List<Transform> allWayPoints;

    [SerializeField] float spaceBetween = 3f;

    [SerializeField] Vector3 size;
    

    //All the points are parented to the Waypoints object and their position is saved in the static list of transforms allWayPoints
    void Awake()
    {

        allWayPoints = new List<Transform>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    GameObject waypoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    waypoint.GetComponent<MeshRenderer>().enabled = false;
                    waypoint.name = (x > y) ? "x" + x : "y" + y;
                    waypoint.transform.position = new Vector3(transform.position.x + x * (size.x+ spaceBetween), transform.position.y + y * (size.y + spaceBetween), transform.position.z + z * (size.z + spaceBetween));
                   
                    
                    waypoint.transform.SetParent(transform);

                    allWayPoints.Add(waypoint.transform);
                }
            }
        }
    }


}
