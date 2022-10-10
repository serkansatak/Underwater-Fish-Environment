using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] Vector2 numFishMinMax;
    [SerializeField] GameObject fishPrefab;
    [SerializeField] Vector2 radiusMinMax;
    [SerializeField] Vector2 swimAnimationMinMax;

    Camera main_cam;
    Texture2D screenshotTex;
    List<GameObject> fish_inst;
    

    public Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(5f, 24f)));
        return world_pos;
    }

    void SaveCameraRGB(Camera cam)
    {
        string filename = "Assets/Scripts/data/img_" + Time.frameCount.ToString() + ".png";
        RenderTexture rt = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 24);
        cam.targetTexture = rt;

        cam.Render();
        RenderTexture.active = rt;
        screenshotTex.Reinitialize(cam.pixelWidth, cam.pixelHeight);
        screenshotTex.ReadPixels(new Rect(0, 0, cam.pixelWidth, cam.pixelHeight), 0, 0);

        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        RenderTexture.ReleaseTemporary(rt);

        rt = null;
        Destroy(rt);

        byte[] bytes = screenshotTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, bytes);
    }

    void add_fog()
    {
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        //Color rnd_col = new Color(Random.value, Random.value, Random.value, Random.value);
        Color rnd_fog_color = new Color(
                Random.Range(162f, 198f)/255, 
                Random.Range(180f, 220f)/255, 
                Random.Range(135f, 165f)/255,
                Random.Range(144f, 176f)/255);
        RenderSettings.fogColor = rnd_fog_color;
        RenderSettings.fogDensity = Random.Range(0.01f, 0.05f);
        RenderSettings.fog = true;
    }

    void instantiate_fish()
    {
        fish_inst = new List<GameObject>();
        int numberOfFish = (int)Random.Range(numFishMinMax.x, numFishMinMax.y);
        for (int i = 0; i < numberOfFish; i++)
        {   
            //float radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
            Vector3 rnd_pos = GetRandomPositionInCamera(main_cam);
            GameObject currFish = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
            currFish.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));
            //currFish.transform.rotation = Random.rotation;
            currFish.transform.parent = transform; // Parent the fish to the moverObj
            currFish.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
            currFish.GetComponent<Animator>().SetFloat("SpeedFish", Random.Range(swimAnimationMinMax.x, swimAnimationMinMax.y));

            currFish.name = "fish_" + i.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
            currFish.GetComponentInChildren<fishName>().fishN = "fish_" + i.ToString();
            fish_inst.Add(currFish);

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

        }
    }

    void clean_up()
    {
        foreach (GameObject go in fish_inst)
        {
            Destroy(go);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //SET UP VARIABLES
        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        screenshotTex = new Texture2D(main_cam.pixelWidth, main_cam.pixelHeight, TextureFormat.RGB24, false);
        //Fog
        add_fog();
        
    }

    void Update()
    {
        instantiate_fish();
        SaveCameraRGB(main_cam);
        clean_up();
    }

    /*

             foreach (Transform childObject in transform)
        {
            // First we get the Mesh attached to the child object
            Mesh mesh = childObject.gameObject.GetComponent<SkinnedMeshRenderer>().mesh;
 
            // If we've found a mesh we can use it to add a collider
            if (mesh != null)
            {                      
                // Add a new MeshCollider to the child object
                MeshCollider meshCollider = childObject.gameObject.AddComponent<MeshCollider>();
 
                // Finaly we set the Mesh in the MeshCollider
                meshCollider.sharedMesh = mesh;
            }
        }

    [SerializeField] GameObject fishPrefab;
    [SerializeField] Vector2 numFishMinMax;
    [SerializeField] Vector2 swimAnimationMinMax;
    // Start is called before the first frame update7

    public Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(12.5f, 17.5f)));
        return world_pos;
    }

    void Awake()
    {
        //Random fish
        Camera cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        GameObject currFish = Instantiate(fishPrefab, GetRandomPositionInCamera(cam), Random.rotation);
        currFish.transform.localScale = new Vector3(Random.value, Random.value, Random.value);
        currFish.GetComponent<Animator>().SetFloat("SpeedFish", Random.Range(swimAnimationMinMax.x, swimAnimationMinMax.y));
        currFish.transform.parent = transform;

        //Fog
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        Color rnd_col = new Color(Random.value, Random.value, Random.value, Random.value);
        RenderSettings.fogColor = rnd_col;
        RenderSettings.fogDensity = Random.Range(0.01f, 0.03f);
        RenderSettings.fog = true;



    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
