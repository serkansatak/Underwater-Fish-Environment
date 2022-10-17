//CAPTURE GAMEVIEW 
//TODO - Distractors 
//TODO - Colliders
//TODO - Swimming poses 
//https://forum.unity.com/threads/capture-overlay-ugui-camera-to-texture.1007156/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Video;

public class Spawner : MonoBehaviour
{
    [SerializeField] Vector2 numFishMinMax;
    [SerializeField] GameObject fishPrefab;
    //[SerializeField] Vector2 radiusMinMax;
    [SerializeField] Vector2 swimAnimationMinMax;

    //int[] control_vector = new int[]{0, 1, 1};
    struct conditionsControl{
        public int background;
        public int fog;
        public int distractors;

        public conditionsControl(int c1, int c2, int c3){
            this.background = c1;
            this.fog = c2;
            this.distractors = c3;
        }
    }
    conditionsControl control = new conditionsControl(0, 0, 0);

    Camera main_cam;
    Camera background_cam;
    Color fogColor;
    VideoPlayer vp;

    string videoDir = "Assets/videos";
    string[] videoFiles;
    string datasetDir = "BrackishMOT_Synth";
    string imageFolder;
    string gtFolder;
    string gtFile;
    int sequence_number = 0;
    int sequence_image;
    int sequence_goal = 2;
    int sequence_length = 100;

    int img_height = 544;
    int img_width = 960;
    RenderTexture screenRenderTexture;
    Texture2D screenshotTex;

    Mesh bakedMesh;

    public class DynamicGameObject
    {
        public GameObject go;
        public int id;
        public int previous_activity; //used to prevent fish from turning twice in a row 
        public int activity; //0 for going straight, 1 for turning
        public float speed;
        //public bool distractor;
    }
    List<DynamicGameObject> dgo_list = new List<DynamicGameObject>();

    //controlDistractors distractor_control;
    //GameObject distractor_obj;
    //controlDistractors distractor_control;
    int number_of_distractors;
    List<DynamicGameObject> distractors_list = new List<DynamicGameObject>(); 
    Vector3 current_direction;
    [SerializeField] Material mat;

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-49, 43);
        float y = Random.Range (-20, 22);
        float z = Random. Range (0, 24);
        return new Vector3(x, y, z);
    }

    void getNewCurrentDirection()
    {
        current_direction = new Vector3 (Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        current_direction.Normalize();
    }

    public void generateDistractors()
    {
        getNewCurrentDirection();
        number_of_distractors = (int) Random.Range(500, 1000);
        //distractors_list = new List<DynamicGameObject>(); 

        for (int i = 0; i < number_of_distractors; i++)
        {
            DynamicGameObject dgo = new DynamicGameObject();
            dgo.speed = Random.Range(1, 10);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = GetRandomPosition();

            sphere.transform.parent = transform;
            sphere.transform.localScale = Vector3.one * Random.Range(0.01f, 1f);
            dgo.go = sphere;
            Renderer rend = sphere.GetComponent<Renderer>();
            rend.material = mat;
            //float rnd_color_seed = Random.Range(75.0f, 225.0f);
            Color rnd_albedo = new Color(
                Random.Range(171f, 191f)/255,  
                Random.Range(192f, 212f)/255, 
                Random.Range(137f, 157f)/255,
                Random.Range(151f, 171f)/255);  
            rend.material.color = rnd_albedo;
            rend.material.SetFloat("_TranspModify", Random.Range(0.25f, 0.5f));
            distractors_list.Add(dgo);
        }
    }

    void updateDistractors()
    {
        //int distractors_to_create = 0;

        for (int i = distractors_list.Count - 1; i >= 0; i--)
        {
            DynamicGameObject distractor = distractors_list[i];
            distractor.go.transform.position += current_direction*Time.deltaTime*distractor.speed;
            if (distractor.go.transform.position.x > 45f || distractor.go.transform.position.x < -55f || 
                distractor.go.transform.position.y > 25f || distractor.go.transform.position.y < -25f ||
                distractor.go.transform.position.z > 25f || distractor.go.transform.position.z < -10f )
            {
                /*distractors_list.RemoveAt(i);
                Destroy(distractor.go);
                distractors_to_create += 1;*/
                distractor.go.transform.position = GetRandomPosition();

            }
        }
    }

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
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.8f), UnityEngine.Random.Range(0.1f, 0.8f), UnityEngine.Random.Range(20f, 30f)));
        return world_pos;
    }

    void SaveImage()
    {   
        string filename;
        if (sequence_image > 99999){
            filename = imageFolder + "/" + sequence_image.ToString() + ".png";
        } else if (sequence_image > 9999) {
            filename = imageFolder + "/0" + sequence_image.ToString() + ".png";
        } else if (sequence_image > 999) {
            filename = imageFolder + "/00" + sequence_image.ToString() + ".png";
        } else if (sequence_image > 99) {
            filename = imageFolder + "/000" + sequence_image.ToString() + ".png";
        } else if (sequence_image > 9) {
            filename = imageFolder + "/0000" + sequence_image.ToString() + ".png";
        } else {
            filename = imageFolder + "/00000" + sequence_image.ToString() + ".png";
        }
        ScreenCapture.CaptureScreenshot(filename);
    }

    void SaveCameraView()
    {
        //Camera cam1 = background_cam;
        //Camera cam2 = main_cam;
        string filename;
        if (sequence_image > 99999){
            filename = imageFolder + "/" + sequence_image.ToString() + ".png";
        } else if (sequence_image > 9999) {
            filename = imageFolder + "/0" + sequence_image.ToString() + ".png";
        } else if (sequence_image > 999) {
            filename = imageFolder + "/00" + sequence_image.ToString() + ".png";
        } else if (sequence_image > 99) {
            filename = imageFolder + "/000" + sequence_image.ToString() + ".png";
        } else if (sequence_image > 9) {
            filename = imageFolder + "/0000" + sequence_image.ToString() + ".png";
        } else {
            filename = imageFolder + "/00000" + sequence_image.ToString() + ".png";
        }
        //string filename = dataDir + "/" + Time.frameCount.ToString() + ".png";
        
        screenRenderTexture = RenderTexture.GetTemporary(img_width, img_height, 24);
        main_cam.targetTexture = screenRenderTexture;
        main_cam.Render();
        RenderTexture.active = screenRenderTexture;

        //Texture2D screenshotTex = new Texture2D(img_width, img_height, TextureFormat.RGB24, false);
        screenshotTex.ReadPixels(new Rect(0, 0, img_width, img_height), 0, 0);

        RenderTexture.active = null; // JC: added to avoid errors
        RenderTexture.ReleaseTemporary(screenRenderTexture);
        screenRenderTexture = null;
        Destroy(screenRenderTexture);

        byte[] byteArray = screenshotTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, byteArray);
        //Destroy(screenshotTex);
    }

    void randomizeFog()
    {
        RenderSettings.fog = false;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        //Color rnd_col = new Color(Random.value, Random.value, Random.value, Random.value);
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = Random.Range(0.005f, 0.02f);
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
        //dgo_list = new List<DynamicGameObject>();
        int numberOfFish = (int)Random.Range(numFishMinMax.x, numFishMinMax.y);
        for (int i = 0; i < numberOfFish; i++)
        {   
            //float radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
            DynamicGameObject dgo = new DynamicGameObject();
            Vector3 rnd_pos = GetRandomPositionInCamera(main_cam);
            GameObject currFish = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
            currFish.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));
            currFish.transform.parent = transform; // Parent the fish to the moverObj
            currFish.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
            float speed = Random.Range(swimAnimationMinMax.x, swimAnimationMinMax.y);
            currFish.GetComponent<Animator>().SetFloat("SpeedFish", speed);
            currFish.name = "fish_" + i.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
            currFish.GetComponentInChildren<fishName>().fishN = "fish_" + i.ToString();

            //Visual randomisation
            SkinnedMeshRenderer renderer = currFish.GetComponentInChildren<SkinnedMeshRenderer>();
            float rnd_color_seed = Random.Range(75.0f, 225.0f);
            Color rnd_albedo = new Color(
                rnd_color_seed/255, 
                rnd_color_seed/255, 
                rnd_color_seed/255,
                Random.Range(0.0f, 1.0f));
            renderer.material.color = rnd_albedo;
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

        foreach (DynamicGameObject dgo in distractors_list)
        {
            Destroy(dgo.go);
        }

        dgo_list.Clear();
        distractors_list.Clear();
    }

    bool isWithinTheView(GameObject go)
    {
        //Animator ani = go.GetComponent<Animator>();
        Vector3 headPosition = go.transform.Find("Armature/Bone").transform.position;
        Vector3 tailPosition = go.transform.Find("Armature/Bone/Bone.001/Bone.002/Bone.003/Bone.004").transform.position;
        Vector3 viewPosHead = main_cam.WorldToViewportPoint(headPosition);
        Vector3 viewPosTail = main_cam.WorldToViewportPoint(tailPosition);
        /*
        GameObject sphere;
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = head_position;
        sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = tail_position;
        sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        */

        /*if (viewPosHead.x <= 0 ||  viewPosHead.x >= 1 ||  
            viewPosTail.x <= 0 ||  viewPosTail.x >= 1 ){*/
        if (viewPosHead.x <= 0 && viewPosTail.x <= 0 ||  
            viewPosHead.x >= 1 && viewPosTail.x >= 1 ){
            return false;
        }
        
        if (viewPosHead.y <= 0 && viewPosTail.y <= 0 ||  
            viewPosHead.y >= 1 && viewPosTail.y >= 1 ){
            return false;
        }

        if (go.transform.position.z > 34f || go.transform.position.z < -10f){
            return false;
        }
        
        return true;
    }

    Vector4 GetBoundingBoxInCamera(GameObject go, Camera cam)
    {
        int min_x, min_y, max_x, max_y;
        bool withinTheView = isWithinTheView(go);
        //Debug.Log("Within the view " + withinTheView.ToString());

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
        //Mesh bakedMesh = new Mesh();
        skinMeshRend.BakeMesh(bakedMesh, true);
        Vector3[] verts_local = bakedMesh.vertices;
        Transform rendererOwner = skinMeshRend.transform;

        for (int j = 0; j < verts_local.Length; j++)
        {
            verts_local[j] = rendererOwner.localToWorldMatrix.MultiplyPoint3x4(verts_local[j]);
        }

        return verts_local;
    }

    void SaveAnnotation(Vector4 bbox, int go_id)
    {   
        if (bbox.x != -1 && bbox.y != -1 && bbox.z != -1 && bbox.w != -1 )
        {   
            if (bbox.x < 0)
            {
                bbox.x = 0;
            }

            if (bbox.y < 0)
            {
                bbox.y = 0;
            }

            string frame = sequence_image.ToString();
            string id = go_id.ToString();
            string left = bbox.x.ToString();
            string top = bbox.y.ToString();

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
        dgo.previous_activity = dgo.activity;
        if(Random.value > 0.5 && dgo.previous_activity != 1)
        {
            dgo.activity = 1;
            dgo.speed = Random.Range(10f, 100f);
        } else {
            dgo.activity = 0;
            dgo.speed = Random.Range(1f, 2f);
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
        Vector3 temp = new Vector3(1f, 0f, 0f);
        dgo.go.transform.position += rot*temp*Time.deltaTime*dgo.speed;
    }

    void addNewSequence()
    {   
        sequence_number += 1;
        if (sequence_number != sequence_goal + 1){
            sequence_image = 0;
            string new_sequence = datasetDir + "/" + datasetDir + "-" + sequence_number.ToString();

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
            string iniFile = new_sequence + "/seqinfo.ini";

            string seqInfo = "[Sequence]\n" + 
                "name = " + new_sequence.ToString() + "\n" +
                "imdir = img1\n" +
                "framerate = 15\n" +
                "seqlength = " + sequence_length.ToString() + "\n" +
                "imwidth = " + img_width.ToString() + "\n" +
                "imheight = " + img_height.ToString() + "\n" +
                "imext = .png";
            
            using (StreamWriter writer = new StreamWriter(iniFile, true))
            {
                writer.Write(seqInfo);
            }
        }
    }

    void randomizeVideo()
    {
        //vp.Stop();
        string random_file = videoFiles[Random.Range(0, videoFiles.Length)];
        //Debug.Log(files[Random.Range(0,files.Length)]);
        //vp.url = random_file;
        vp.url = "Assets/videos/converted/video_1_conv.ogv";
        vp.Prepare();
    }

    void Awake()
    {
        //Setup folder structure
        //Create string describing generation conditions and append it to the base daatset folder name        
        string controlString = "";
        if (control.background == 1){
            controlString += "_Background";
        } else {
            controlString += "_NoBackground";
        }

        if (control.fog == 1){
            controlString += "_Fog";
        } else {
            controlString += "_NoFog";
        }

        if (control.distractors == 1){
            controlString += "_Distractors";
        } else {
            controlString += "_NoDistractors";
        }
        datasetDir += controlString;


        //Create a parent folder, remove the old one if it exists
        if (System.IO.Directory.Exists(datasetDir))
        {
            System.IO.Directory.Delete(datasetDir, true);
            System.IO.Directory.CreateDirectory(datasetDir);
        } else {
             System.IO.Directory.CreateDirectory(datasetDir);
        }

        //Set up constant variables
        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        background_cam = GameObject.Find("Background Camera").GetComponent<Camera>();
        if (control.background == 0) {
            background_cam.enabled = false;
        }

        videoFiles = System.IO.Directory.GetFiles(videoDir,"*.avi");
        vp = GameObject.Find("Video player").GetComponent<VideoPlayer>();

        bakedMesh = new Mesh();
        screenshotTex = new Texture2D(img_width, img_height, TextureFormat.RGB24, false);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //Set up a first scene
        if (control.background == 1) randomizeVideo();
        generateFogColor();
        randomizeBackgroundColor();
        if (control.fog == 1) randomizeFog();         
        if (control.distractors == 1) generateDistractors();
        InstantiateFish();
        addNewSequence();
    }


    void Update()
    {   
        if (sequence_image == sequence_length)
        {      
            CleanUp();

            if (control.background == 1) randomizeVideo();
            generateFogColor();
            randomizeBackgroundColor();
            if (control.fog == 1) randomizeFog();         
            if (control.distractors == 1) generateDistractors();
            InstantiateFish();
            addNewSequence();
        } 

        if(vp.isPlaying || control.background == 0)
        {
            sequence_image += 1;
            foreach (DynamicGameObject dgo in dgo_list)
            {
                if (Time.frameCount%20 == 0)
                {
                    updateActivity(dgo);
                }
                
                //updateActivity(dgo);
                if (dgo.activity == 0){
                    goStraight(dgo);
                } else {
                    Turn(dgo);
                }
            }
            if (control.distractors == 1) updateDistractors();
        }
    }


    void LateUpdate()
    {

        if (sequence_number == sequence_goal+1)
        {  
            Debug.Log("All sequences were generated");
            UnityEditor.EditorApplication.isPlaying = false;

        } else {

            if(vp.isPlaying || control.background == 0)
            {
                foreach (DynamicGameObject dgo in dgo_list)
                {
                    Vector4 bounds = GetBoundingBoxInCamera(dgo.go, main_cam);
                    SaveAnnotation(bounds, dgo.id);
                    //Debug.Log("Bounds" + bounds);
                }

                if (control.background == 1){
                    SaveImage();
                } else {
                    SaveCameraView();
                }

                Debug.Log("Sequence Number " + sequence_number.ToString() 
                + " Sequence Image " + sequence_image.ToString() 
                + "/" + sequence_length.ToString());
            }
        }

    }
}
