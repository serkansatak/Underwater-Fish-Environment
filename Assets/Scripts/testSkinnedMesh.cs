using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testSkinnedMesh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SkinnedMeshRenderer skinMesh = GetComponent<SkinnedMeshRenderer>();

        Mesh mesh = skinMesh.sharedMesh;
        Vector3[] verts_local = mesh.vertices;

        //Mesh mesh = new Mesh();

        //skinMesh.BakeMesh(mesh, true);
        //List<Vector3> meshVertices = new List<Vector3>();
        //mesh.GetVertices(meshVertices);

        Debug.Log(mesh.vertices.Length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
