using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


//Tried to directly use the GetBoundingBoxCamera, but needed to modify the Vertex getting script. Currently the script is quite heavy because of the looping so we need to figure out if it can be used
public class GetFishBounds : MonoBehaviour
{

    Camera fishCamera;
    // Start is called before the first frame update
    void Start()
    {
        fishCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector4 bounds = GetBoundingBoxInCamera(gameObject, fishCamera);

        Debug.Log(bounds);
    }

    Vector4 GetBoundingBoxInCamera(GameObject go, Camera cam)
    {
        Vector3[] verts = GetMeshVertices(go);

        Debug.Log(verts.Length);
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = cam.WorldToScreenPoint(verts[i]);
        }
        Vector2 min = verts[0];
        Vector2 max = verts[0];
        foreach (Vector2 v in verts)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }

        min.y = Screen.height - min.y;
        max.y = Screen.height - max.y;
        if (min.y > max.y)
        {
            float temp = max.y;
            max.y = min.y;
            min.y = temp;
        }

        int min_x, min_y, max_x, max_y;
        min_x = (int)min.x;
        min_y = (int)min.y;
        max_x = (int)max.x;
        max_y = (int)max.y;

        return new Vector4(min_x, min_y, max_x, max_y);
    }

    Vector3[] GetMeshVertices(GameObject go)
    {
        //Vector3[] verts = new Vector3[0];


        SkinnedMeshRenderer skinMesh = GetComponent<SkinnedMeshRenderer>();

        Mesh mesh = skinMesh.sharedMesh;
        Vector3[] verts_local = mesh.vertices;
        for (int j = 0; j < verts_local.Length; j++)
        {
            verts_local[j] = go.transform.TransformPoint(verts_local[j]);
        }
        //verts = verts_local;


        // grap mesh filter of parent if available otherwise of children
        //if (go.TryGetComponent<MeshFilter>(out MeshFilter mMeshF))
        //{
        //    Vector3[] verts_local = mMeshF.mesh.vertices;
        //    for (int j = 0; j < verts_local.Length; j++)
        //    {
        //        verts_local[j] = go.transform.TransformPoint(verts_local[j]);
        //    }
        //    verts = verts_local;
        //}

        //else
        //{
        //    for (int i = 0; i < go.transform.childCount; i++)
        //    {
        //        GameObject childObj = go.transform.GetChild(i).gameObject;
        //        try
        //        {
        //            Vector3[] verts_local = childObj.GetComponent<MeshFilter>().mesh.vertices;
        //            for (int j = 0; j < verts_local.Length; j++)
        //            {
        //                verts_local[j] = childObj.transform.TransformPoint(verts_local[j]);
        //            }
                    
        //            verts = verts.Concat(verts_local).ToArray();
        //        }
        //        catch (NullReferenceException e)
        //        {
        //            print(go.name);
        //            print(go.transform.name);
        //            verts = verts.Concat(childObj.GetComponent<MeshFilter>().mesh.vertices).ToArray();
        //        }
        //    }

        //}
        return verts_local;
    }
}
