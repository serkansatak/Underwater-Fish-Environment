using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowVertices : MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    [SerializeField] Material fishMat;
    GameObject Fish;
    Camera main_cam;
    Vector3[] verts;

    Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(5f, 24f)));
        return world_pos;
    }

    GameObject spawn_fish()
    {
        //float radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
        Vector3 rnd_pos = GetRandomPositionInCamera(main_cam);
        GameObject currFish = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
        currFish.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));
        //currFish.transform.rotation = Random.rotation;
        currFish.transform.parent = transform; // Parent the fish to the moverObj
        currFish.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
        //currFish.GetComponent<Animator>().SetFloat("SpeedFish", Random.Range(0.3f, 1.0f));

        //Visual randomisation
        SkinnedMeshRenderer renderer = currFish.GetComponentInChildren<SkinnedMeshRenderer>();
        float rnd_color_seed = Random.Range(75.0f, 225.0f);
        Color rnd_albedo_v2 = new Color(
            rnd_color_seed/255, 
            rnd_color_seed/255, 
            rnd_color_seed/255,
            Random.Range(0.0f, 1.0f));
        renderer.material.color = rnd_albedo_v2;
        renderer.material.SetFloat("_Metalic", Random.Range(0.1f, 0.5f));
        renderer.material.SetFloat("_Metalic/_Glossiness", Random.Range(0.1f, 0.5f));
        
        return currFish;
    }

    /*
    Vector4 GetBoundingBoxInCamera(GameObject go, Camera cam)
    {
        Vector3[] verts = GetMeshVertices(go);

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
        //Vector4 (x, y, z, w)
        return new Vector4(min_x, min_y, max_x, max_y);
    }

    
    Vector3[] GetMeshVertices(GameObject go)
    {
        //SkinnedMeshRenderer skinMesh = go.GetComponentInChildren<SkinnedMeshRenderer>();
        MeshRenderer skinMesh = go.GetComponent<MeshRenderer>();
        Mesh bakedMesh = new Mesh();
        bakedMesh = skinMesh.mesh;
        //skinMesh.BakeMesh(bakedMesh);
        //https://docs.unity3d.com/ScriptReference/SkinnedMeshRenderer.BakeMesh.html
        //skinMesh.BakeMesh(bakedMesh, true);
        Vector3[] verts_local = bakedMesh.vertices;

        for (int j = 0; j < verts_local.Length; j++)
        {
            verts_local[j] = go.transform.TransformPoint(verts_local[j]);
        }
        return verts_local;
    }*/

    Vector3[] GetMeshVertices_v2(GameObject go)
    {
        SkinnedMeshRenderer skinMeshRend = go.GetComponentInChildren<SkinnedMeshRenderer>();
        Mesh bakedMesh = new Mesh();
        skinMeshRend.BakeMesh(bakedMesh, true);
        //Vector3[] verts_local;
        //bakedMesh.GetVertices(verts_local);
        Vector3[] verts_local = bakedMesh.vertices;
        Transform rendererOwner = skinMeshRend.transform;
        //Debug.Log("Renderer Scale " + skinMeshRend.transform.localScale);
        //Transform rendererOwner  = Fish.transform;
        
        
        //https://docs.unity3d.com/ScriptReference/SkinnedMeshRenderer.BakeMesh.html
        //skinMesh.BakeMesh(bakedMesh, true);
        

        for (int j = 0; j < verts_local.Length; j++)
        {
            verts_local[j] = rendererOwner.localToWorldMatrix.MultiplyPoint3x4(verts_local[j]);
        }

        return verts_local;
    }

    // Start is called before the first frame update
    void Start()
    {
        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        Fish = spawn_fish();
        verts = GetMeshVertices_v2(Fish);
        for (int i = 0; i < verts.Length; i+=50)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = verts[i];
            sphere.transform.localScale = Vector3.one * 0.1f;
            sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            //sphere.transform.parent = Fish.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
