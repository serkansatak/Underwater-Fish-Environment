//CAPTURE GAMEVIEW 
//TODO - Distractors 
//TODO - Colliders
//TODO - Swimming poses 
//https://forum.unity.com/threads/capture-overlay-ugui-camera-to-texture.1007156/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class SpawnerFlocks : MonoBehaviour
{
    //[SerializeField] Vector2 numFishMinMax;
    [SerializeField] GameObject fishPrefab;
    //[SerializeField] Vector2 radiusMinMax;
    //[SerializeField] Vector2 swimAnimationMinMax;

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

    Camera mainCam;
    Camera backgroundCam;
    
    Color fogColor;
    
    VideoPlayer vp;
    string videoDir = "Assets/videos";
    string[] videoFiles;

    int FPS = 15;
    float deltaTime;
    float timePassed;
    
    int numberOfFlocksMin = 1;
    int numberOfFlocksMax = 10;
    int numberOfFlocks;
    int numberOfFishInTheFlockMin = 5;
    int numberOfFishInTheFlockMax = 10;
    int numberOfFishInTheFlock;
    int radiusMin = 3;
    int radiusMax = 9;

    float maxLinSpeed = 5f;
    float minLinSpeed = 1f;
    float maxAngSpeed = 180f;
    float minAngSpeed = -180f;
    float animationSpeed = 0.75f;


    string rootDir;
    string datasetDir = "BrackishMOT_Synth";
    string imageFolder;
    string gtFolder;
    string gtFile;
    int sequenceNumber = 0;
    int sequenceImage;
    int sequenceGoal = 2;
    //int sequenceLength = 100;
    int sequenceLength = 50;

    int imgHeight = 544;
    int imgWidth = 960;
    RenderTexture screenRenderTexture;
    Texture2D screenshotTex;

    Mesh bakedMesh;

    public class DynamicGameObject
    {
        public GameObject go;
        public int id; //used for fish only
        public int previousActivity; //fish, used to prevent fish from turning twice in a row 
        public int activity; //fish, 0 for going straight, 1 for turning
        public Vector3 linSpeed; //used for fish
        public Vector3 angSpeed; //used for fish
        public float speed; //used for distractors
        //public bool distractor;
    }

    List<DynamicGameObject> fish_list = new List<DynamicGameObject>(); //RENAME TO FISH LIST
    List<DynamicGameObject> flock_list = new List<DynamicGameObject>();
    List<DynamicGameObject> distractors_list = new List<DynamicGameObject>(); 

    int numberOfDistractors;
    Vector3 currentDirection;
    [SerializeField] Material mat;


    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-49, 43);
        float y = Random.Range (-20, 22);
        float z = Random. Range (0, 24);
        return new Vector3(x, y, z);
    }

    Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 worldPos = cam.ViewportToWorldPoint( new Vector3(
            UnityEngine.Random.Range(0.1f, 0.8f), 
            UnityEngine.Random.Range(0.1f, 0.8f), 
            UnityEngine.Random.Range(20f, 30f)));
        return worldPos;
    }

    Vector3 GetRandomPositionInUnitSphere(Camera cam, Vector3 offset)
    {
        float radius = Random.Range(radiusMin, radiusMax);
        Vector3 spherePos = Random.insideUnitSphere * radius + offset;
        return spherePos;
    }

    void getNewCurrentDirection()
    {
        currentDirection = new Vector3 (Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        currentDirection.Normalize();
    }

    public void generateDistractors()
    {
        getNewCurrentDirection();
        numberOfDistractors = (int) Random.Range(500, 1000);
        //distractors_list = new List<DynamicGameObject>(); 

        for (int i = 0; i < numberOfDistractors; i++)
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
            distractor.go.transform.position += currentDirection*deltaTime*distractor.speed;
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

    void SaveGameView()
    {   
        string filename;
        if (sequenceImage > 99999){
            filename = imageFolder + "/" + sequenceImage.ToString() + ".png";
        } else if (sequenceImage > 9999) {
            filename = imageFolder + "/0" + sequenceImage.ToString() + ".png";
        } else if (sequenceImage > 999) {
            filename = imageFolder + "/00" + sequenceImage.ToString() + ".png";
        } else if (sequenceImage > 99) {
            filename = imageFolder + "/000" + sequenceImage.ToString() + ".png";
        } else if (sequenceImage > 9) {
            filename = imageFolder + "/0000" + sequenceImage.ToString() + ".png";
        } else {
            filename = imageFolder + "/00000" + sequenceImage.ToString() + ".png";
        }
        ScreenCapture.CaptureScreenshot(filename);
    }

    void SaveCameraView()
    {
        //Camera cam1 = backgroundCam;
        //Camera cam2 = mainCam;
        string filename;
        if (sequenceImage > 99999){
            filename = imageFolder + "/" + sequenceImage.ToString() + ".png";
        } else if (sequenceImage > 9999) {
            filename = imageFolder + "/0" + sequenceImage.ToString() + ".png";
        } else if (sequenceImage > 999) {
            filename = imageFolder + "/00" + sequenceImage.ToString() + ".png";
        } else if (sequenceImage > 99) {
            filename = imageFolder + "/000" + sequenceImage.ToString() + ".png";
        } else if (sequenceImage > 9) {
            filename = imageFolder + "/0000" + sequenceImage.ToString() + ".png";
        } else {
            filename = imageFolder + "/00000" + sequenceImage.ToString() + ".png";
        }
        //string filename = dataDir + "/" + Time.frameCount.ToString() + ".png";
        
        screenRenderTexture = RenderTexture.GetTemporary(imgWidth, imgHeight, 24);
        mainCam.targetTexture = screenRenderTexture;
        mainCam.Render();
        RenderTexture.active = screenRenderTexture;

        //Texture2D screenshotTex = new Texture2D(imgWidth, imgHeight, TextureFormat.RGB24, false);
        screenshotTex.ReadPixels(new Rect(0, 0, imgWidth, imgHeight), 0, 0);

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
    void InstantiateFish(int fishIteration)
    { 
        //float radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
        DynamicGameObject dgo = new DynamicGameObject();
        Vector3 rnd_pos = GetRandomPositionInUnitSphere(mainCam, flock_list.Last().go.transform.position);
        dgo.go = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
        //dgo.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));
        dgo.go.transform.parent = flock_list.Last().go.transform; // Parent the fish to the moverObj
        dgo.go.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
        
        /*float speed = Random.Range(swimAnimationMinMax.x, swimAnimationMinMax.y);
        currFish.GetComponent<Animator>().SetFloat("SpeedFish", speed);*/
        float linSpeed = Random.Range(minLinSpeed, maxLinSpeed);
        //dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", );
        dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", animationSpeed);
        dgo.linSpeed = new Vector3(linSpeed, 0f, 0f);

        dgo.go.name = "fish_" + fishIteration.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
        dgo.go.GetComponentInChildren<fishName>().fishN = "fish_" + fishIteration.ToString();

        

        //Visual randomisation
        SkinnedMeshRenderer renderer = dgo.go.GetComponentInChildren<SkinnedMeshRenderer>();
        float rnd_color_seed = Random.Range(75.0f, 225.0f);
        Color rnd_albedo = new Color(
            rnd_color_seed/255, 
            rnd_color_seed/255, 
            rnd_color_seed/255,
            Random.Range(0.0f, 1.0f));
        renderer.material.color = rnd_albedo;
        renderer.material.SetFloat("_Metalic", Random.Range(0.1f, 0.5f));
        renderer.material.SetFloat("_Metalic/_Glossiness", Random.Range(0.1f, 0.5f));
        
        //dgo.go = currFish;
        dgo.id = flock_list.Last().id;
        dgo.activity = 0;
        //dgo.speed = speed;
        fish_list.Add(dgo);
    }

    void InstantiateFlocks()
    {
        numberOfFlocks = (int) Random.Range(numberOfFlocksMin, numberOfFlocksMax);
        for (int i = 0; i < numberOfFlocks; i++)
        {
            DynamicGameObject dgo = new DynamicGameObject();
            dgo.go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = verts[i];
            //sphere.transform.localScale = Vector3.one * 0.1f;
            dgo.go.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            dgo.go.transform.parent = transform;
            dgo.go.transform.position = GetRandomPositionInCamera(mainCam);
            dgo.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));

            dgo.id = i;
            dgo.activity = 0;
            float linSpeed = Random.Range(minLinSpeed, maxLinSpeed);
            dgo.linSpeed = new Vector3(linSpeed, 0f, 0f);
            flock_list.Add(dgo);

            numberOfFishInTheFlock = (int)Random.Range(numberOfFishInTheFlockMin, numberOfFishInTheFlockMax);
            for (int j = 0; j < numberOfFishInTheFlock; j++)
            {
                InstantiateFish(j);
            }
        }
    }

    void CleanUp()
    {
        foreach (DynamicGameObject dgo in fish_list)
        {
            Destroy(dgo.go);
        }

        foreach (DynamicGameObject dgo in flock_list)
        {
            Destroy(dgo.go);
        }

        foreach (DynamicGameObject dgo in distractors_list)
        {
            Destroy(dgo.go);
        }

        fish_list.Clear();
        flock_list.Clear();
        distractors_list.Clear();
    }

    bool isWithinTheView(GameObject go)
    {
        //Animator ani = go.GetComponent<Animator>();
        Vector3 headPosition = go.transform.Find("Armature/Bone").transform.position;
        Vector3 tailPosition = go.transform.Find("Armature/Bone/Bone.001/Bone.002/Bone.003/Bone.004").transform.position;
        Vector3 viewPosHead = mainCam.WorldToViewportPoint(headPosition);
        Vector3 viewPosTail = mainCam.WorldToViewportPoint(tailPosition);

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

            string frame = sequenceImage.ToString();
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
        if(dgo.previousActivity == 0)
        {
            dgo.activity = 1;
            float angSpeed = Random.Range(minAngSpeed, maxAngSpeed);
            //float linSpeed = Mathf.Abs(angSpeed/10f);
            float linSpeed = Random.Range(minLinSpeed, maxLinSpeed);
            //dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", linSpeed);
            dgo.linSpeed = new Vector3(linSpeed, 0f, 0f);
            dgo.angSpeed = new Vector3(0, angSpeed, 0);
        }

        if(dgo.previousActivity == 1)
        {
            dgo.activity = 0;
            float linSpeed = Random.Range(minLinSpeed, maxLinSpeed);
            //dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", linSpeed);
            dgo.linSpeed = new Vector3(linSpeed, 0f, 0f);
        }
    }

    void Turn(DynamicGameObject dgo)
    {
        dgo.go.transform.Rotate(dgo.angSpeed * deltaTime);
        dgo.go.transform.position += dgo.go.transform.rotation * dgo.linSpeed * deltaTime;
    }

    void goStraight(DynamicGameObject dgo)
    {
        dgo.go.transform.position += dgo.go.transform.rotation * dgo.linSpeed * deltaTime;
    }

    void addNewSequence()
    {   
        sequenceNumber += 1;
        if (sequenceNumber != sequenceGoal + 1){
            sequenceImage = 0;
            string new_sequence = rootDir + "/" + datasetDir + "-" + sequenceNumber.ToString();

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
                "framerate = " + FPS.ToString() + "\n" +
                "seqlength = " + sequenceLength.ToString() + "\n" +
                "imwidth = " + imgWidth.ToString() + "\n" +
                "imheight = " + imgHeight.ToString() + "\n" +
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
        rootDir = datasetDir;
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
        rootDir = rootDir + controlString + "/" + datasetDir;


        //Create a parent folder, remove the old one if it exists
        if (System.IO.Directory.Exists(rootDir))
        {
            System.IO.Directory.Delete(rootDir, true);
            System.IO.Directory.CreateDirectory(rootDir);
        } else {
             System.IO.Directory.CreateDirectory(rootDir);
        }

        //Set up constant variables
        mainCam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        backgroundCam = GameObject.Find("Background Camera").GetComponent<Camera>();
        if (control.background == 0) {
            backgroundCam.enabled = false;
        }

        videoFiles = System.IO.Directory.GetFiles(videoDir,"*.avi");
        vp = GameObject.Find("Video player").GetComponent<VideoPlayer>();

        bakedMesh = new Mesh();
        screenshotTex = new Texture2D(imgWidth, imgHeight, TextureFormat.RGB24, false);

        //Set the seed for random
        Random.InitState(7);

        //Set delta time used for animating
        deltaTime = (float) 1/FPS;
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
        //InstantiateFish();
        InstantiateFlocks();
        addNewSequence();
    }


    void Update()
    {   
        //deltaTime = Time.deltaTime;
        if (sequenceImage == sequenceLength)
        {      
            CleanUp();

            if (control.background == 1) randomizeVideo();
            generateFogColor();
            randomizeBackgroundColor();
            if (control.fog == 1) randomizeFog();         
            if (control.distractors == 1) generateDistractors();
            InstantiateFlocks();
            //InstantiateFish();
            addNewSequence();
        } 

        if(vp.isPlaying || control.background == 0)
        {
            timePassed += deltaTime;

            sequenceImage += 1;
            foreach (DynamicGameObject dgo in flock_list)
            {
                /*if (Time.frameCount%20 == 0)
                {
                    updateActivity(dgo);
                }
                
                //updateActivity(dgo);
                if (dgo.activity == 0){
                    goStraight(dgo);
                } else {
                    Turn(dgo);
                }*/

                if (timePassed > 1)
                { 
                    if (dgo.activity == 1) updateActivity(dgo);
                    if (dgo.activity == 0 && Random.value > 0.5f) updateActivity(dgo);
                }

                if (dgo.activity == 0){
                    goStraight(dgo);
                } else {
                    Turn(dgo);
                }
            }

            if (control.distractors == 1) updateDistractors();
            if (timePassed > 1) timePassed = 0;
        }
    }


    void LateUpdate()
    {

        if (sequenceNumber == sequenceGoal+1)
        {  
            Debug.Log("All sequences were generated");
            UnityEditor.EditorApplication.isPlaying = false;

        } else {

            if(vp.isPlaying || control.background == 0)
            {
                foreach (DynamicGameObject dgo in fish_list)
                {
                    Vector4 bounds = GetBoundingBoxInCamera(dgo.go, mainCam);
                    SaveAnnotation(bounds, dgo.id);
                    //Debug.Log("Bounds" + bounds);
                }

                if (control.background == 1){
                    SaveGameView();
                } else {
                    SaveCameraView();
                }

                Debug.Log("Sequence Number " + sequenceNumber.ToString() 
                + " Sequence Image " + sequenceImage.ToString() 
                + "/" + sequenceLength.ToString());
            }
        }

    }
}
