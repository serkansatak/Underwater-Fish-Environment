//CAPTURE GAMEVIEW 
//TODO - Distractors 
//TODO - Colliders
//TODO - Swimming poses 
//https://forum.unity.com/threads/capture-overlay-ugui-camera-to-texture.1007156/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class Spawner : MonoBehaviour
{
    [SerializeField] Vector2 numFishMinMax;
    [SerializeField] GameObject fishPrefab;
    [SerializeField] Vector2 radiusMinMax;
    [SerializeField] Vector2 swimAnimationMinMax;

    Camera main_cam;
    Camera background_cam;
    Color fogColor;

    /*struct CameraBoundsInWorld
    {
        public Vector3 topLeft;
        public Vector3 topRight;
        public Vector3 bottomLeft;
        public Vector3 bottomRight;
    }
    CameraBoundsInWorld bounds;*/
    //Texture2D screenshotTex;
    //List<GameObject> fish_inst;

    string datasetDir = "BrackishMOT_Synth";
    string imageFolder;
    string gtFolder;
    string gtFile;
    int sequence_number = 0;
    int sequence_image;

    public class DynamicGameObject
    {
        public GameObject go;
        public int id;
        public int previous_activity; //used to prevent fish from turning twice in a row 
        public int activity; //0 for going straight, 1 for turning
        public float speed;
        //public bool distractor;
    }
    List<DynamicGameObject> dgo_list;


    void generateFogColor()
    {
        //Base values 181, 202, 147, 161
        fogColor = new Color(
            Random.Range(171f, 191f)/255,  
            Random.Range(192f, 212f)/255, 
            Random.Range(137f, 157f)/255,
            Random.Range(151f, 171f)/255);
    }
    
    public Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.8f), UnityEngine.Random.Range(0.1f, 0.8f), UnityEngine.Random.Range(5f, 24f)));
        return world_pos;
    }

    Texture2D GetFishTexture()
    {
        //string filename = datasetDir + "/" + Time.frameCount.ToString() + ".png";
        RenderTexture rt = RenderTexture.GetTemporary(main_cam.pixelWidth, main_cam.pixelHeight, 24);
        main_cam.targetTexture = rt;
        RenderTexture.active = rt;
        main_cam.Render();

        Texture2D screenshotTex = new Texture2D(main_cam.pixelWidth, main_cam.pixelHeight, TextureFormat.RGB24, false);
        //screenshotTex.Reinitialize(main_cam.pixelWidth, main_cam.pixelHeight);
        screenshotTex.ReadPixels(new Rect(0, 0, main_cam.pixelWidth, main_cam.pixelHeight), 0, 0);

        main_cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        RenderTexture.ReleaseTemporary(rt);
        rt = null;
        Destroy(rt);

        //byte[] bytes = screenshotTex.EncodeToPNG();
        //System.IO.File.WriteAllBytes(filename, bytes);
        return screenshotTex;
    }

    void SaveTexture(Texture2D tex)
    {   
        string filename;
        sequence_image += 1;
        if (Time.frameCount > 99999){
            filename = imageFolder + "/" + sequence_image.ToString() + ".png";
        } else if (Time.frameCount > 9999) {
            filename = imageFolder + "/0" + sequence_image.ToString() + ".png";
        } else if (Time.frameCount > 999) {
            filename = imageFolder + "/00" + sequence_image.ToString() + ".png";
        } else if (Time.frameCount > 99) {
            filename = imageFolder + "/000" + sequence_image.ToString() + ".png";
        } else if (Time.frameCount > 9) {
            filename = imageFolder + "/0000" + sequence_image.ToString() + ".png";
        } else {
            filename = imageFolder + "/00000" + sequence_image.ToString() + ".png";
        }
        //string filename = imageFolder + "/" + Time.frameCount.ToString() + ".png";
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, bytes);
        
    }

    void randomizeFog()
    {
        RenderSettings.fog = false;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        //Color rnd_col = new Color(Random.value, Random.value, Random.value, Random.value);
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = Random.Range(0.01f, 0.05f);
        RenderSettings.fog = true;
    }

    void randomizeBackgroundColor(){
        GameObject bg = GameObject.Find("backgroundTransparent");
        MeshRenderer bg_renderer = bg.GetComponent<MeshRenderer>();
        bg_renderer.material.color = fogColor;
    }

    //TODO - Add pose within Unity Sphere for some images in order to simulate flocks
    void InstantiateFish()
    {
        //fish_inst = new List<GameObject>();
        dgo_list = new List<DynamicGameObject>();
        int numberOfFish = (int)Random.Range(numFishMinMax.x, numFishMinMax.y);
        for (int i = 0; i < numberOfFish; i++)
        {   
            //float radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
            DynamicGameObject dgo = new DynamicGameObject();
            Vector3 rnd_pos = GetRandomPositionInCamera(main_cam);
            GameObject currFish = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
            currFish.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));
            //currFish.transform.rotation = Random.rotation;
            currFish.transform.parent = transform; // Parent the fish to the moverObj
            currFish.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
            float speed = Random.Range(swimAnimationMinMax.x, swimAnimationMinMax.y);
            currFish.GetComponent<Animator>().SetFloat("SpeedFish", speed);
            currFish.name = "fish_" + i.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
            currFish.GetComponentInChildren<fishName>().fishN = "fish_" + i.ToString();
            //fish_inst.Add(currFish);

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
            
            dgo.go = currFish;
            dgo.id = i;
            dgo.activity = 0;
            dgo.speed = speed;
            dgo_list.Add(dgo);
        }
    }

    void CleanUp()
    {
        foreach (DynamicGameObject dgo in dgo_list)
        {
            Destroy(dgo.go);
        }
    }

    bool isWithinTheView(GameObject go)
    {
        
        Vector3 viewPos = main_cam.WorldToViewportPoint(go.transform.position);
        if (viewPos.x <= 0 ||  viewPos.x >= 0.9 ){
            return false;
        }
        if (viewPos.y <= 0 ||  viewPos.y >= 0.9 ){
            return false;
        }
        if (viewPos.z >  26f){
            return false;
        }
        return true;
        /*if (go.transform.position.z < bounds.topRight.z && go.transform.position.z < bounds.bottomLeft.z &&
        go.transform.position.x > bounds.bottomLeft.x && go.transform.position.y > bounds.bottomLeft.y &&
        go.transform.position.x < bounds.topRight.x && go.transform.position.y < bounds.topRight.y )
        {
            return true;
        } else {
            return false;
        }*/
    }

    Vector4 GetBoundingBoxInCamera(GameObject go, Camera cam)
    {
        int min_x, min_y, max_x, max_y;
        bool withinTheView = isWithinTheView(go);
        Debug.Log("Within the view " + withinTheView.ToString());

        if (withinTheView)
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

            min_x = (int)min.x;
            min_y = (int)min.y;
            max_x = (int)max.x;
            max_y = (int)max.y;
            
        } else {
            min_x = -1;
            min_y = -1;
            max_x = -1;
            max_y = -1;
        }

        //Vector4 (x, y, z, w)
        return new Vector4(min_x, min_y, max_x, max_y);
    }

    Vector3[] GetMeshVertices(GameObject go)
    {
        SkinnedMeshRenderer skinMeshRend = go.GetComponentInChildren<SkinnedMeshRenderer>();
        Mesh bakedMesh = new Mesh();
        skinMeshRend.BakeMesh(bakedMesh, true);
        Vector3[] verts_local = bakedMesh.vertices;
        Transform rendererOwner = skinMeshRend.transform;

        for (int j = 0; j < verts_local.Length; j++)
        {
            verts_local[j] = rendererOwner.localToWorldMatrix.MultiplyPoint3x4(verts_local[j]);
        }

        return verts_local;
    }

    //TODO - add ID
    void SaveAnnotation(Vector4 bbox, int go_id)
    {   
        if (bbox.x != -1 && bbox.y != -1 && bbox.z != -1 && bbox.w != -1 )
        {
            string frame = Time.frameCount.ToString();
            string id = go_id.ToString();
            string left = bbox.x.ToString();
            string top = bbox.y.ToString();

            //float widthf = bbox.z-bbox.x;
            //float heightf = bbox.w-bbox.y;
            //Debug.Log("FLOAT widthf " + widthf + " heightf " + heightf);

            int width = (int)Mathf.Round(bbox.z-bbox.x);
            int height = (int)Mathf.Round(bbox.w-bbox.y);

            string confidence = "1";
            string class_id = "1";
            string visibility = "1";

            string annotation = frame + ","
                + id + ","
                + left + ","
                + top + ","
                + width.ToString() + ","
                + height.ToString() + ","
                + confidence + ","
                + class_id + ","
                + visibility + ","
                + "\n";
            
            //string line = maskObjects[i].name.Split('_')[0] + " " + bboxs[i].x.ToString() + " " + bboxs[i].y.ToString() + " " + bboxs[i].z.ToString() + " " + bboxs[i].w.ToString() + "\n";
            using (StreamWriter writer = new StreamWriter(gtFile, true))
            {
                writer.Write(annotation);
            }
        }
        //Debug.Log(annotation);
    }

    void updateActivity(DynamicGameObject dgo)
    {
        /*if(Random.value > 0.75 || Random.value < 0.25)
        {
            dgo.activity = 1;
            dgo.speed = Random.Range(10f, 100f);
        } else {
            dgo.activity = 0;
            dgo.speed = Random.Range(0.5f, 2f);
        }*/

        dgo.previous_activity = dgo.activity;

        if(Random.value > 0.5 && dgo.previous_activity != 1)
        {
            dgo.activity = 1;
            dgo.speed = Random.Range(10f, 100f);
        } else {
            dgo.activity = 0;
            dgo.speed = Random.Range(1f, 3f);
        }

    }

    void Turn(DynamicGameObject dgo)
    {
        //Debug.Log("Turning, Time Delta " + Time.deltaTime.ToString());
        dgo.go.transform.Rotate(0, Time.deltaTime*dgo.speed, 0 );
    }

    void goStraight(DynamicGameObject dgo)
    {
        dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", dgo.speed);
        Quaternion rot = dgo.go.transform.rotation;
        Vector3 test = new Vector3(1f, 0f, 0f);
        //speed = Random.Range(0.5f, 1.5f);
        dgo.go.transform.position += rot*test*Time.deltaTime*dgo.speed;
    }

    void addNewSequence()
    {
        sequence_number += 1;
        sequence_image = 0;
        string new_sequence = datasetDir + "/" + datasetDir + "-" + sequence_number.ToString();
        
        //Debug.Log(gt_txt);

        if (System.IO.Directory.Exists(new_sequence))
        {
            System.IO.Directory.Delete(new_sequence, true);
            System.IO.Directory.CreateDirectory(new_sequence);
        } else {
            System.IO.Directory.CreateDirectory(new_sequence);
        }

        imageFolder = new_sequence + "/img1";
        gtFolder = new_sequence + "/gt";
        System.IO.Directory.CreateDirectory(imageFolder);
        System.IO.Directory.CreateDirectory(gtFolder);

        gtFile = gtFolder + "/gt.txt";
    }

    void Awake()
    {
        //Setup folder structure

        //Create a parent folder, remove the old one if it exists
        if (System.IO.Directory.Exists(datasetDir))
        {
            System.IO.Directory.Delete(datasetDir, true);
            System.IO.Directory.CreateDirectory(datasetDir);
        } else {
             System.IO.Directory.CreateDirectory(datasetDir);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //SET UP CONSTANT VARIABLES
        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        background_cam = GameObject.Find("Background Camera").GetComponent<Camera>();
        //getWorldBounds();
        //screenshotTex = new Texture2D(main_cam.pixelWidth, main_cam.pixelHeight, TextureFormat.RGB24, false);
        //Fog
        
        generateFogColor();
        randomizeBackgroundColor();
        randomizeFog(); 
        InstantiateFish();

        addNewSequence();
    }

    void Update()
    {
        /*
        Debug.Log("Iteration " + Time.frameCount.ToString());
        generateFogColor();
        randomizeBackgroundColor();
        randomizeFog(); 
        InstantiateFish();
        */
        
        
        if (Time.frameCount%15 == 0)
        {
           foreach (DynamicGameObject dgo in dgo_list)
           {
            updateActivity(dgo);
           }
           addNewSequence();
        }

        foreach (DynamicGameObject dgo in dgo_list)
        {
            
            if (dgo.activity == 0){
                goStraight(dgo);
            } else {
                Turn(dgo);
            }

            Vector4 bounds = GetBoundingBoxInCamera(dgo.go, main_cam);
            SaveAnnotation(bounds, dgo.id);
            Debug.Log("Bounds" + bounds);
        }

        Texture2D fish = GetFishTexture();
        SaveTexture(fish);
        /*
        //if (Time.frameCount%600 == 0) {reset}
        if (Time.frameCount == 300)
        {
            CleanUp();
        }
        */
    }
}
