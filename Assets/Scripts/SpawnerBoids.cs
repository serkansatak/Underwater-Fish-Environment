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

public class SpawnerBoids : MonoBehaviour
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
    conditionsControl control = new conditionsControl(0, 0, 1);

    Vector2 numFishMinMax = new Vector2(4, 50);
    int numberOfSwarms = 1;
    //Vector2 numSwarmsMinMax = new Vector2(2, 10);
    Camera mainCam;
    Camera backgroundCam;
    Color fogColor;
    VideoPlayer vp;
    int FPS = 15;
    float deltaTime;
    int fishId;
    //float timePassed;

    Bounds simAreaBounds;
    Vector3 simAreaSize = new Vector3(150, 60, 180);

    float animationSpeed = 1f;
    
    int numberOfRandomFish = 0;
    Vector2 numOfRandomFishMinMax;
    int maxNumOfRandomFish;

    //default boids values
    float boidSpeed = 10f; //10f
    float boidSteeringSpeed = 100f; //100
    float boidNoClumpingArea = 20f;
    float boidLocalArea = 10f;
    //float boidSimulationArea = 30f;
    
    //default weights
    float K = 1f;
    float S = 1f;
    float M = 1f;
    float X = 1f;

    public class boidController
    {
        public GameObject go;
        public SkinnedMeshRenderer renderer;

        //identification data
        public int id;
        public int swarmIndex;

        //random movement
        public bool randomBehaviour = false;
        
        //public int elapsedFrames;
        //public int goalFrames;
        //public int framesToMaxSpeed;

        public float elapsedTime;
        public float goalTime;
        public float timeToMaxSpeed;

        public Vector3 randomDirection;
        public float randomWeight;
        public float randomSpeed;
        public float randomSteeringSpeed;

        //original values are used to revert back into non-random behaviour
        public float originalSpeed;
        public float originalSteeringSpeed;

        //default behaviour values
        public float noClumpingArea;
        public float localArea;
        public float speed;
        public float steeringSpeed;
    }
    List<boidController> boidsList = new List<boidController>();

    string videoDir = "Assets/videos";
    string[] videoFiles;
    string rootDir;
    string datasetDir = "BrackishMOT_Synth";
    string imageFolder;
    string gtFolder;
    string gtFile;
    int sequence_number = 0;
    int sequence_image;
    int sequence_goal = 50;
    //int sequence_length = 100;
    int sequence_length = 150;

    int img_height = 544;
    int img_width = 960;
    RenderTexture screenRenderTexture;
    Texture2D screenshotTex;

    Mesh bakedMesh;

    public class DynamicGameObject
    {
        public GameObject go;
        public float speed; //used for distractors
    }

    int number_of_distractors;
    List<DynamicGameObject> distractors_list = new List<DynamicGameObject>(); 
    Vector3 current_direction;
    [SerializeField] Material mat;

    void getNewCurrentDirection()
    {
        current_direction = new Vector3 (Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        current_direction.Normalize();
    }

    public void generateDistractors()
    {
        getNewCurrentDirection();
        number_of_distractors = (int) Random.Range(500, 1000);

        for (int i = 0; i < number_of_distractors; i++)
        {
            DynamicGameObject dgo = new DynamicGameObject();
            dgo.speed = Random.Range(1, 10);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = GetRandomPositionInCamera(mainCam);
            sphere.name = "distractor_" + i.ToString();

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

    void updateDistractors(float time)
    {
        print("updateDistractors");
        for (int i = distractors_list.Count - 1; i >= 0; i--)
        {
            DynamicGameObject distractor = distractors_list[i];
            Vector3 distPos = distractor.go.transform.position;
            distPos += current_direction*time*distractor.speed;
            /*bool inBounds = simAreaBounds.Contains(distPos);
        
            if (!inBounds)
            {
                if (distPos.x > simAreaBounds.max.x) distPos.x -= simAreaBounds.extents.x * 2f;
                if (distPos.x < simAreaBounds.min.x) distPos.x += simAreaBounds.extents.x * 2f;

                if (distPos.y > simAreaBounds.max.y) distPos.y -= simAreaBounds.extents.y * 2f;
                if (distPos.y < simAreaBounds.min.y) distPos.y += simAreaBounds.extents.y * 2f;

                if (distPos.z > simAreaBounds.max.z) distPos.z -= simAreaBounds.extents.z * 2f;
                if (distPos.z < simAreaBounds.min.z) distPos.z += simAreaBounds.extents.z * 2f;

                distractor.go.transform.position = distPos;
            }*/
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
        Vector3 world_pos = cam.ViewportToWorldPoint( new Vector3(
            UnityEngine.Random.Range(0.1f, 0.8f), 
            UnityEngine.Random.Range(0.1f, 0.8f), 
            UnityEngine.Random.Range(20f, 40f)));
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
        //Camera cam1 = backgroundCam;
        //Camera cam2 = mainCam;
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
        mainCam.targetTexture = screenRenderTexture;
        mainCam.Render();
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

    void instantiateFish(int swarmIdx)
    { 
        fishId = 0;
        int numberOfFish = (int) Random.Range(numFishMinMax.x, numFishMinMax.y);

        for (int i = 0; i < numberOfFish; i++)
        {
            boidController b = new boidController();
            b.go = Instantiate(fishPrefab);
            b.go.transform.position = GetRandomPositionInCamera(mainCam);
            b.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0);
            b.go.transform.localScale = Vector3.one * Random.Range(0.5f, 1f);
            b.go.GetComponent<Animator>().SetFloat("SpeedFish", animationSpeed);
            b.go.name = "fish_" + fishId.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
            b.go.GetComponentInChildren<fishName>().fishN = "fish_" + fishId.ToString();

            //Visual randomisation
            SkinnedMeshRenderer renderer = b.go.GetComponentInChildren<SkinnedMeshRenderer>();
            b.renderer = renderer;
            float rnd_color_seed = Random.Range(75.0f, 225.0f);
            Color rnd_albedo = new Color(
                rnd_color_seed/255, 
                rnd_color_seed/255, 
                rnd_color_seed/255,
                Random.Range(0.0f, 1.0f));
            renderer.material.color = rnd_albedo;
            renderer.material.SetFloat("_Metalic", Random.Range(0.1f, 0.5f));
            renderer.material.SetFloat("_Metalic/_Glossiness", Random.Range(0.1f, 0.5f));
   
            b.id = fishId;
            fishId++;
            b.swarmIndex = swarmIdx;
            b.speed = boidSpeed;
            b.steeringSpeed = boidSteeringSpeed;
            b.localArea = boidLocalArea;
            b.noClumpingArea = boidNoClumpingArea;
            
            boidsList.Add(b);
        }
    }

    void CleanUp()
    {
        foreach (boidController b in boidsList)
        {
            Destroy(b.go);
        }

        foreach (DynamicGameObject dgo in distractors_list)
        {
            Destroy(dgo.go);
        }

        boidsList.Clear();
        distractors_list.Clear();
    }

    bool isWithinTheView(GameObject go)
    {
        
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

        if (go.transform.position.z > 50f || go.transform.position.z < -10f){
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


    void addNewSequence()
    {   
        sequence_number += 1;
        if (sequence_number != sequence_goal + 1){
            sequence_image = 0;
            string new_sequence = rootDir + "/" + datasetDir + "-" + sequence_number.ToString();

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

    float updateSpeed(boidController b, float rndSpeed, float initSpeed)
    {
        float deltaSpeed = rndSpeed - initSpeed;
        float speedIncrement;
        float newSpeed;
    
        if (b.elapsedTime < b.timeToMaxSpeed)
        {
            speedIncrement = b.elapsedTime/b.timeToMaxSpeed*deltaSpeed;
        } 
        else if (b.elapsedTime == b.timeToMaxSpeed) 
        {
            speedIncrement = deltaSpeed;
        }   
        else 
        {
            speedIncrement = b.elapsedTime/b.goalTime*deltaSpeed*-1f;
        }

        newSpeed = initSpeed + speedIncrement;
        
        return newSpeed;
    }

    void simulateMovement(List<boidController> boids, float time)
    {
        //maxNumOfRandomFish = (int) Random.Range(1f, numOfFish/2f);
        maxNumOfRandomFish = 2;

        for (int i = 0; i < boids.Count; i++)
        {
            boidController b_i = boids[i];

            Vector3 steering = Vector3.zero;
            Vector3 separationDirection = Vector3.zero;
            int separationCount = 0;
            Vector3 alignmentDirection = Vector3.zero;
            int alignmentCount = 0;
            Vector3 cohesionDirection = Vector3.zero;
            int cohesionCount = 0;
            Vector3 leaderDirection = Vector3.zero;
            boidController leaderBoid = boids[0];
            float leaderAngle = 180f;

            Vector3 randomDirection = Vector3.zero;
            float randomWeight = 0;

            if (!b_i.randomBehaviour && Random.value > .9f && numberOfRandomFish != maxNumOfRandomFish)
            {
                numberOfRandomFish += 1;
                b_i.randomBehaviour = true;

                //b_i.elapsedFrames = 0;
                //b_i.goalFrames = (int) Random.Range(180f, 360f);
                //b_i.framesToMaxSpeed = Mathf.RoundToInt(Random.Range(0.1f, 0.5f) * b_i.goalFrames);

                b_i.elapsedTime = 0f;
                b_i.goalTime = 1f;
                b_i.timeToMaxSpeed = Random.Range(0.1f, 0.5f);
                
                /*float rndValue = Random.Range(-1f, 1f);
                float cond = Random.value;
                if (cond < 0.33f)
                {
                    b_i.randomDirection = new Vector3(rndValue, 0, 0);
                } 
                else if (cond < 0.66f)
                {
                    b_i.randomDirection = new Vector3(0, rndValue, 0);
                }
                else 
                {
                    b_i.randomDirection = new Vector3(0, 0, rndValue);
                } */
                b_i.randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                b_i.randomDirection = b_i.randomDirection.normalized;

                b_i.randomWeight = Random.Range(1f, 10f);
                b_i.randomSteeringSpeed = Random.Range(2f, 10f)*b_i.steeringSpeed;
                //b_i.randomSpeed = b_i.randomSteeringSpeed/10f;
                b_i.originalSpeed = b_i.speed;
                b_i.originalSteeringSpeed = b_i.steeringSpeed;
            }

            if (b_i.randomBehaviour)
            {
                //if (b_i.elapsedFrames == b_i.goalFrames)
                if (b_i.elapsedTime > b_i.goalTime)
                {
                    numberOfRandomFish -= 1;
                    b_i.randomBehaviour = false;
                    b_i.speed = b_i.originalSpeed;
                    b_i.steeringSpeed = b_i.originalSteeringSpeed;
                }
                else
                {
                    randomDirection = b_i.randomDirection;
                    randomWeight = b_i.randomWeight;
                    //b_i.speed = updateSpeedTime(b_i, b_i.randomSpeed, b_i.originalSpeed);
                    b_i.steeringSpeed = updateSpeed(b_i, b_i.randomSteeringSpeed, b_i.originalSteeringSpeed);

                    b_i.elapsedTime += time;
                    //b_i.elapsedFrames += 1;
                }
            } 

            if (!b_i.randomBehaviour)
            {
                for (int j = 0; j < boids.Count; j++)
                {
                    boidController b_j = boids[j];
                    if (b_i == b_j) continue;

                    float distance = Vector3.Distance(b_j.go.transform.position, b_i.go.transform.position);
                    if (distance < boidNoClumpingArea)
                    {
                        separationDirection += b_j.go.transform.position - b_i.go.transform.position;
                        separationCount++;
                    }

                    if (distance < boidLocalArea && b_j.swarmIndex == b_i.swarmIndex)
                    {
                        alignmentDirection += b_j.go.transform.forward;
                        alignmentCount++;

                        cohesionDirection += b_j.go.transform.position - b_i.go.transform.position;
                        cohesionCount++;

                        //identify leader
                        float angle = Vector3.Angle(b_j.go.transform.position - b_i.go.transform.position, b_i.go.transform.forward);
                        if (angle < leaderAngle && angle < 90f)
                        {
                            leaderBoid = b_j;
                            leaderAngle = angle;
                        }
                    }
                }
            
                if (separationCount > 0) separationDirection /= separationCount;
                separationDirection = -separationDirection;
                separationDirection = separationDirection.normalized;

                if (alignmentCount > 0) alignmentDirection /= alignmentCount;
                alignmentDirection = alignmentDirection.normalized;

                if (cohesionCount > 0) cohesionDirection /= cohesionCount;
                cohesionDirection -= b_i.go.transform.position;
                cohesionDirection = cohesionDirection.normalized;

                if (leaderBoid != null) 
                {
                    leaderDirection = leaderBoid.go.transform.position - b_i.go.transform.position;
                    leaderDirection = leaderDirection.normalized;
                }
            }

        
            Vector3 boundsDirection = Vector3.zero;
            float distanceToSimArea = Vector3.Distance(simAreaBounds.center, b_i.go.transform.position);
            boundsDirection = simAreaBounds.center - b_i.go.transform.position;
            boundsDirection = boundsDirection.normalized;

            steering += boundsDirection;
            steering += separationDirection*S;
            steering += alignmentDirection*M;
            steering += cohesionDirection*K;
            steering += leaderDirection*X;
            steering += randomDirection*randomWeight;

            if (randomDirection != Vector3.zero)
            {
                steering = randomDirection*randomWeight;
                //print("randomDirection " + randomDirection.ToString());
                //print("randomWeight " + randomWeight.ToString());
                //print("steering " + steering.ToString());
            }

            if (!simAreaBounds.Contains(b_i.go.transform.position))
            {
                steering = boundsDirection*distanceToSimArea;
            }

            if (steering != Vector3.zero)
            {
                    b_i.go.transform.rotation = Quaternion.RotateTowards(
                        b_i.go.transform.rotation, 
                        Quaternion.LookRotation(steering), 
                        b_i.steeringSpeed * time);
            }
           
            b_i.go.transform.position += b_i.go.transform.TransformDirection(new Vector3(b_i.speed, 0, 0))* time;
        }
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

        GameObject background = GameObject.Find("backgroundTransparent");
        background.SetActive(false);

        GameObject simArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        simArea.transform.position = new Vector3(0, 0, simAreaSize.z/2f);
        simArea.transform.localScale = simAreaSize;
        UnityEngine.Physics.SyncTransforms();
        simAreaBounds = simArea.GetComponent<Collider>().bounds;
        simArea.SetActive(false);

        videoFiles = System.IO.Directory.GetFiles(videoDir,"*.avi");
        vp = GameObject.Find("Video player").GetComponent<VideoPlayer>();

        bakedMesh = new Mesh();
        screenshotTex = new Texture2D(img_width, img_height, TextureFormat.RGB24, false);

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
        //randomizeBackgroundColor();
        mainCam.backgroundColor = fogColor;
        if (control.fog == 1) randomizeFog();         
        if (control.distractors == 1) generateDistractors();
        for (int i = 0; i < numberOfSwarms; i++)
        {
            instantiateFish(i);
        }
        addNewSequence();
    }


    void Update()
    {   
        //deltaTime = Time.deltaTime;
        if (sequence_image == sequence_length)
        {      
            CleanUp();

            if (control.background == 1) randomizeVideo();
            generateFogColor();
            //randomizeBackgroundColor();
            mainCam.backgroundColor = fogColor;
            if (control.fog == 1) randomizeFog();         
            if (control.distractors == 1) generateDistractors();
            for (int i = 0; i < numberOfSwarms; i++)
            {
                instantiateFish(i);
            }
            addNewSequence();
        } 

        if(vp.isPlaying || control.background == 0)
        {
            sequence_image += 1;

            simulateMovement(boidsList, deltaTime);

            if (control.distractors == 1) updateDistractors(deltaTime);
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
                foreach (boidController b in boidsList)
                {
                    Vector4 bounds = GetBoundingBoxInCamera(b.go, mainCam);
                    SaveAnnotation(bounds, b.id);
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
