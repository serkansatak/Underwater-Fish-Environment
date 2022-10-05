using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Script for getting the bounding rectangle from the Renderer bounds of an object. 

// This script needs to be tested by outputting some of these bounds together with the images and rendering them in OpenCV just to see if what we get is correct.
// As some of the coordinate systems might need to be switched or scaled
public class getScreenSpaceBounds : MonoBehaviour
{
    //Initialize the mesh renderer and a rect variable
    Renderer objRenderer;
    Rect rect;

    void Start()
    {
        //Get the renderer 
        objRenderer = transform.GetComponent<Renderer>();
    }


    void Update()
    {
        //Each update cycle get the screen space rect. Here we also use the fishName script to get the name of the child object. This should be used as fish IDs in the parsed .csv/.txt file
        rect = RendererBoundsInScreenSpace(objRenderer);

        Debug.Log($"{transform.GetComponent<fishName>().fishN} Pos {rect.position}, {rect.width}, {rect.height}");

    }

    //This is a function that I found in the Unity forums, No modifications. Needs to be tested
    Rect RendererBoundsInScreenSpace(Renderer r)
    {
        // This is the space occupied by the object's visuals
        // in WORLD space.
        Bounds bigBounds = r.bounds;

        //if (screenSpaceCorners == null)
        Vector3[] screenSpaceCorners = new Vector3[8];

        Camera theCamera = Camera.main;

        // For each of the 8 corners of our renderer's world space bounding box,
        // convert those corners into screen space.
        screenSpaceCorners[0] = theCamera.WorldToScreenPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
        screenSpaceCorners[1] = theCamera.WorldToScreenPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));
        screenSpaceCorners[2] = theCamera.WorldToScreenPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
        screenSpaceCorners[3] = theCamera.WorldToScreenPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));
        screenSpaceCorners[4] = theCamera.WorldToScreenPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
        screenSpaceCorners[5] = theCamera.WorldToScreenPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));
        screenSpaceCorners[6] = theCamera.WorldToScreenPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
        screenSpaceCorners[7] = theCamera.WorldToScreenPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));

        // Now find the min/max X & Y of these screen space corners.
        float min_x = screenSpaceCorners[0].x;
        float min_y = screenSpaceCorners[0].y;
        float max_x = screenSpaceCorners[0].x;
        float max_y = screenSpaceCorners[0].y;

        for (int i = 1; i < 8; i++)
        {
            if (screenSpaceCorners[i].x < min_x)
            {
                min_x = screenSpaceCorners[i].x;
            }
            if (screenSpaceCorners[i].y < min_y)
            {
                min_y = screenSpaceCorners[i].y;
            }
            if (screenSpaceCorners[i].x > max_x)
            {
                max_x = screenSpaceCorners[i].x;
            }
            if (screenSpaceCorners[i].y > max_y)
            {
                max_y = screenSpaceCorners[i].y;
            }
        }

        return Rect.MinMaxRect(min_x, min_y, max_x, max_y);

    }



}
