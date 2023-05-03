
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor.Media;
using UnityEditor;
using System;
using Random = UnityEngine.Random;

public class SpawnerBoids : MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    [SerializeField] Material distractorMaterial;

    public bool useBackground;
    public bool useFog;
    public bool useDistractors;

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
    conditionsControl control;
    int controlIdx = 0;
    List<conditionsControl> controlList = new List<conditionsControl>();
    
    struct boundingBox{
        public int top;
        public int bottom;
        public int left;
        public int right;
        public int height;
        public int width;
        public bool save;
    }


    public Vector2 numFishMinMax = new Vector2(4, 50);
    int numberOfSwarms = 1;
    int numberOfFish;

    Camera mainCam;
    Color fogColor;
    VideoPlayer vp;

    int FPS = 15;
    float deltaTime;
    int fishId;

    GameObject simArea;
    System.Random sysRand = new System.Random();
    Renderer simAreaRenderer;
    Bounds simAreaBounds;
    Vector3 simAreaSize = new Vector3(150, 60, 180);

    float animationSpeed = 1f;   
    int numberOfRandomFish;
 
    //default boids values
    float boidSpeed = 10f;
    float boidSteeringSpeed = 100f; 
    float boidNoClumpingArea = 20f;
    float boidLocalArea = 10f;
    
    //default weights
    float K = 1f;
    float S = 1f;
    float M = 1f;
    float L = 1f;
 
    public class boidController
    {
        public GameObject go;
        public SkinnedMeshRenderer renderer;

        //identification data
        public int id;
        public int swarmIndex;

        //random movement
        public bool randomBehaviour = false;
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

        //default behaviour values, not used for anything yet
        public float noClumpingArea;
        public float localArea;
        public float speed;
        public float steeringSpeed;
    }
    List<boidController> boidsList = new List<boidController>();

    string videoDir = "Assets/Videos";
    string[] videoFiles;
    string rootDir;
    string datasetDir = "brackishMOTSynth";
    string imageFolder;


    // Video 
    string videoFileName;
    private MediaEncoder videoEncoder;
    private int bitrate = 345678;
    private VideoTrackEncoderAttributes videoAttributes;
    //

    string gtFolder;
    string gtFile;
    int sequence_number = 0;
    int sequence_image;
    public int sequence_goal = 5;
    public int sequence_length = 25;

    int img_height = 544;
    int img_width = 960;
    RenderTexture screenRenderTexture;
    Texture2D screenshotTex;

    Mesh bakedMesh;
 
    int number_of_distractors;
    List<GameObject> distractors_list = new List<GameObject>(); 

    string normalizedFogIntensity;
    string numberOfDistractors;
    string spawnedFish;
    string backgroundSequence;

    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

    public void generateDistractors()
    {
        number_of_distractors = (int) Random.Range(500, 5000);
        numberOfDistractors = number_of_distractors.ToString();

        for (int i = 0; i < number_of_distractors; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = mainCam.ViewportToWorldPoint( new Vector3(
            UnityEngine.Random.Range(0.0f, 1f), 
            UnityEngine.Random.Range(0.0f, 1f),
            UnityEngine.Random.Range(10f, 50f)));

            sphere.name = "distractor_" + i.ToString();

            sphere.transform.parent = transform;
            sphere.transform.localScale = Vector3.one * GetRandomLogNormal(0.01f, 0.4f);
            Renderer rend = sphere.GetComponent<Renderer>();
            rend.material = distractorMaterial;
            Color rnd_white = new Color(
                GetRandomLogNormal(220f, 225f)/255,
                GetRandomLogNormal(220f, 255f)/255,
                GetRandomLogNormal(220f, 255f)/255,
                GetRandomLogNormal(151f, 220f)/255 
            );
            rend.material.color = rnd_white;
            
            Color rnd_albedo = new Color(
                Random.Range(171f, 191f)/255,  
                Random.Range(192f, 212f)/255, 
                Random.Range(137f, 157f)/255,
                Random.Range(151f, 171f)/255);  
            
            rend.material.color = rnd_albedo;
            
            rend.material.SetFloat("_TranspModify", Random.Range(0f, 1f));
            distractors_list.Add(sphere);
        }
    }

    public static float NextLogNormal(double mu = 0.0001, double sigma = 0.5, bool scaleToOne = true)
    {
        double x = (double)Random.Range((float)0.0001, (float)5);
        double lnx = System.Math.Log(x);
        double exp_part = System.Math.Exp( -(System.Math.Pow((lnx-mu), 2) / (2*System.Math.Pow(sigma,2)) ) );
        double reg_part = 1 / (x * sigma * System.Math.Sqrt(2 * System.Math.PI) );
        double y = exp_part * reg_part;

        if (scaleToOne) 
        {
            y /= 0.904;
            y = System.Math.Min(y, 1);
        }

        return (float)y;
    }

    public float GetRandomLogNormal(float lowerBound, float upperBound, bool reversed = false, double mean = 0.001, double stdDev = 0.5)
    {
        float randLN = NextLogNormal((double)mean, (double)stdDev, true);
        if (reversed)
        {
            return upperBound - (randLN * (upperBound - lowerBound));
        } else 
        {
            return randLN * (upperBound - lowerBound) + lowerBound;
        }
    }


    void updateDistractors()
    {
        foreach (GameObject go in distractors_list)
        {
            go.transform.position = mainCam.ViewportToWorldPoint( new Vector3(
            UnityEngine.Random.Range(0.0f, 1f), 
            UnityEngine.Random.Range(0.0f, 1f),
            UnityEngine.Random.Range(10f, 50f)));
        }
    }

    void generateFogColor()
    {
        if (Random.value < 0.5f)
        {
            fogColor = new Color(
                Random.Range(171f, 191f)/255,  
                Random.Range(192f, 212f)/255, 
                Random.Range(137f, 157f)/255,
                Random.Range(151f, 171f)/255);
        }
        else
        {
            float rnd_color_seed = Random.Range(75.0f, 225.0f);
            fogColor = new Color(
                rnd_color_seed/255, 
                rnd_color_seed/255, 
                rnd_color_seed/255,
                Random.Range(0.0f, 1.0f));
        }
    }
    
    public Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint( new Vector3(
            UnityEngine.Random.Range(0.0f, 1f), 
            UnityEngine.Random.Range(0.0f, 1f), 
            UnityEngine.Random.Range(20f, 60f)));
        return world_pos;
    }

    void SaveCameraView()
    {
        string filename;
        if (sequence_image > 99999){
            filename = imageFolder + "/" + sequence_image.ToString() + ".jpg";
        } else if (sequence_image > 9999) {
            filename = imageFolder + "/0" + sequence_image.ToString() + ".jpg";
        } else if (sequence_image > 999) {
            filename = imageFolder + "/00" + sequence_image.ToString() + ".jpg";
        } else if (sequence_image > 99) {
            filename = imageFolder + "/000" + sequence_image.ToString() + ".jpg";
        } else if (sequence_image > 9) {
            filename = imageFolder + "/0000" + sequence_image.ToString() + ".jpg";
        } else {
            filename = imageFolder + "/00000" + sequence_image.ToString() + ".jpg";
        }
        
        screenRenderTexture = RenderTexture.GetTemporary(img_width, img_height, 24);
        mainCam.targetTexture = screenRenderTexture;
        mainCam.Render();
        RenderTexture.active = screenRenderTexture;

        screenshotTex.ReadPixels(new Rect(0, 0, img_width, img_height), 0, 0);
        RenderTexture.active = null; // JC: added to avoid errors
        RenderTexture.ReleaseTemporary(screenRenderTexture);
        screenRenderTexture = null;
        Destroy(screenRenderTexture);

        // Video -- add frame.
        
        Texture2D videoTex = new Texture2D(screenshotTex.width, screenshotTex.height, TextureFormat.RGBA32, false);

        Color[] pixels = screenshotTex.GetPixels();
        Color[] newPixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            newPixels[i] = new Color(pixel.r, pixel.g, pixel.b, 1.0f); // set alpha channel to 1.0
        }

        videoTex.SetPixels(newPixels);
        videoTex.Apply();
        
        videoEncoder.AddFrame(videoTex);

        Debug.Log("Video Time : " + vp.clockTime);
        //

        byte[] byteArray = screenshotTex.EncodeToJPG();
        System.IO.File.WriteAllBytes(filename, byteArray);
    }

    void randomizeFog()
    {
        simArea.SetActive(true);
        Renderer simAreaRenderer = simArea.GetComponent<Renderer>();
        simAreaRenderer.material = distractorMaterial;
        simAreaRenderer.material.color = fogColor;
        float fogIntensityMax = 0.1f;
        float fogIntensityMin = 0.8f;
        float fogIntensity = Random.Range(fogIntensityMin, fogIntensityMax);
        simAreaRenderer.material.SetFloat("_TranspModify", fogIntensity);
        float normalizedIntensity = (fogIntensity - fogIntensityMin)/(fogIntensityMax-fogIntensityMin);
        normalizedFogIntensity = normalizedIntensity.ToString();
    }

    void instantiateFish(int swarmIdx)
    { 
        fishId = 1;
        int numberOfFish = (int) Random.Range(numFishMinMax.x, numFishMinMax.y);
        spawnedFish = numberOfFish.ToString();

        for (int i = 0; i < numberOfFish; i++)
        {
            boidController b = new boidController();
            b.go = Instantiate(fishPrefab);
            b.go.SetActive(false);
            b.go.transform.position = GetRandomPositionInCamera(mainCam);
            b.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0);
            b.go.transform.localScale = Vector3.one * Random.Range(0.5f, 1f);
            b.go.GetComponent<Animator>().SetFloat("SpeedFish", animationSpeed);
            b.go.name = "fish_" + fishId.ToString();
            b.go.GetComponentInChildren<fishName>().fishN = "fish_" + fishId.ToString();

            //Visual randomisation
            SkinnedMeshRenderer renderer = b.go.GetComponentInChildren<SkinnedMeshRenderer>();
            renderer.forceMatrixRecalculationPerRender = true;
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

            b.go.SetActive(true);
        }
    }

    void CleanUp()
    {
        foreach (boidController b in boidsList)
        {
            Destroy(b.go);
        }

        foreach (GameObject go in distractors_list)
        {
            Destroy(go);
        }

        boidsList.Clear();
        distractors_list.Clear();
        numberOfRandomFish = 0;
        simArea.SetActive(false);
    }

    bool isWithinTheView(GameObject go)
    {
        if (go.transform.position.z < -10f || go.transform.position.z > 75f){
            return false;
        }

        Vector3 headPosition = go.transform.Find("Armature/Bone").transform.position;
        Vector3 tailPosition = go.transform.Find("Armature/Bone/Bone.001/Bone.002/Bone.003/Bone.004").transform.position;
        Vector3 viewPosHead = mainCam.WorldToViewportPoint(headPosition);
        Vector3 viewPosTail = mainCam.WorldToViewportPoint(tailPosition);
        float minViewPos = 0.0f;
        float maxViewPos = 1.0f;

        if (viewPosHead.x <= minViewPos && viewPosTail.x <= minViewPos ||  
            viewPosHead.x >= maxViewPos && viewPosTail.x >= maxViewPos ){
            return false;
        }
        
        if (viewPosHead.y <= minViewPos && viewPosTail.y <= minViewPos ||  
            viewPosHead.y >= maxViewPos && viewPosTail.y >= maxViewPos ){
            return false;
        }

        return true;
    }


    boundingBox GetBoundingBoxInCamera(GameObject go, Camera cam)
    {
        bool withinTheView = isWithinTheView(go);
        boundingBox bb = new boundingBox();
        bb.save = false;

        if (withinTheView)
        { 
            bb.save = true;
            Vector3[] verts = GetMeshVertices(go);
            Vector2[] pixelCoordinates = new Vector2[verts.Length];

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = cam.WorldToScreenPoint(verts[i]);
                pixelCoordinates[i].x = verts[i].x;
                pixelCoordinates[i].y = verts[i].y;
            }

            Vector2 min = pixelCoordinates[0];
            Vector2 max = pixelCoordinates[0];
            foreach (Vector2 pixel in pixelCoordinates)
            {   
                if (pixel.x >= 0 && pixel.x <= img_width && pixel.y >= 0 && pixel.y <= img_height)
                {
                    min = Vector2.Min(min, pixel);
                    max = Vector2.Max(max, pixel);
                }
            }
            
            float minHeight = min.y;
            float maxHeight = max.y;
            min.y = img_height - maxHeight;
            max.y = img_height - minHeight;

            bb.left = (int) min.x;
            bb.right = (int) max.x;
            bb.top = (int) min.y;
            bb.bottom = (int) max.y;

            float temp;
            bb.height = (int) bb.bottom - bb.top;
            temp = bb.top + bb.height;
            if (temp > img_height) bb.height = img_height - bb.top;

            bb.width = (int) bb.right - bb.left;
            temp = bb.left + bb.width;
            if (temp > img_width) bb.width = img_width - bb.left;
        }
        return bb;
    }

    Vector3[] GetMeshVertices(GameObject go)
    {
        SkinnedMeshRenderer skinMeshRend = go.GetComponentInChildren<SkinnedMeshRenderer>();
        skinMeshRend.BakeMesh(bakedMesh, true);
        Vector3[] verts_local = bakedMesh.vertices;
        Transform rendererOwner = skinMeshRend.transform;

        for (int j = 0; j < verts_local.Length; j++)
        {
            verts_local[j] = rendererOwner.localToWorldMatrix.MultiplyPoint3x4(verts_local[j]);
        }

        return verts_local;
    }

    void SaveAnnotation(boundingBox bbox, int go_id)
    {   
        if (bbox.save)
        {   
            string frame = sequence_image.ToString();
            string id = go_id.ToString();
            string left = bbox.left.ToString();
            string top = bbox.top.ToString();
            string width = bbox.width.ToString();
            string height = bbox.height.ToString();


            string confidence = "1";
            string class_id = "5";
            string visibility = "1";

            string annotation = frame + ","
                + id + ","
                + left + ","
                + top + ","
                + width + ","
                + height + ","
                + confidence + ","
                + class_id + ","
                + visibility + "\n";

            using (StreamWriter writer = new StreamWriter(gtFile, true))
            {
                writer.Write(annotation);
            }
        }
    }

    void addNewSequence()
    {   

        sequence_number += 1;
        if (sequence_number != sequence_goal + 1){
            sequence_image = 0;

            string seq_name;
            if (sequence_number < 10)
            {
                seq_name = datasetDir + "-0" + sequence_number.ToString();
            }
            else
            {
                seq_name = datasetDir + "-" + sequence_number.ToString();
            }

            string new_sequence = rootDir + "/" + seq_name;

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
            
            if (control.background == 0) backgroundSequence = "plain";
            if (control.distractors == 0) numberOfDistractors = "0";
            if (control.fog == 0) normalizedFogIntensity = "0";

            string seqInfo = "[Sequence]\n" + 
                "name=" + seq_name.ToString() +"\n" +
                "imDir=img1\n" +
                "frameRate=" + FPS.ToString() + "\n" +
                "seqLength=" + sequence_length.ToString() + "\n" +
                "imWidth=" + img_width.ToString() + "\n" +
                "imHeight=" + img_height.ToString() + "\n" +
                "imExt=.jpg\n" +
                "fogIntensity=" + normalizedFogIntensity + "\n" +
                "numberOfDistractors=" + numberOfDistractors + "\n" +
                "spawnedFish=" + spawnedFish + "\n" +
                "backgroundSequence=" + backgroundSequence + "\n";
            
            using (StreamWriter writer = new StreamWriter(iniFile, true))
            {
                writer.Write(seqInfo);
            }



            Debug.Log($"Sequence name : {new_sequence}");
            // Video -- initialize encoder while initializing new sequenceW
            if (videoEncoder != null) 
            {
                videoEncoder.Dispose();
                videoEncoder = null;
            }

            
            
            H264EncoderAttributes h264Attr = new H264EncoderAttributes
            {
                //gopSize = 25,
                //numConsecutiveBFrames = 2,
                profile = VideoEncodingProfile.H264High
            };
            
            
            videoAttributes = new VideoTrackEncoderAttributes(h264Attr)
            {
                frameRate = new MediaRational(FPS),
                width = (uint)img_width,
                height = (uint)img_height,
                targetBitRate = (uint)bitrate,
            };

            videoFileName = new_sequence + "/output.mp4";
            videoEncoder = new MediaEncoder(videoFileName, videoAttributes);
        } 
    }

    void setVideoProperties()
    {
        img_height = (int)vp.height;
        img_width = (int)vp.width;
        FPS = (int)vp.frameRate;

        long fileSize = new System.IO.FileInfo(vp.clip.originalPath).Length;
        float duration = (float)vp.frameCount / vp.frameRate;
        float bitrate_ = fileSize / duration * 8.0f / 1000000.0f; // in Mbps
        this.bitrate = (int)bitrate_ * 1000000;
        sequence_length = (int)vp.frameCount;

       // videoAttributes = vp.GetVideoTrackAttributes(0);
    }

    void randomizeVideo()
    {
        string random_file = videoFiles[Random.Range(0, videoFiles.Length)];
        vp.url = random_file;
        string backgroundSequenceFull = random_file;
        backgroundSequence = backgroundSequenceFull.Replace("Assets/videos\\", "");
        backgroundSequence = backgroundSequence.Replace(".mp4", "");
        
        vp.Prepare();
        //setVideoProperties();

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
        else 
        {
            speedIncrement = b.elapsedTime/b.goalTime*deltaSpeed*-1f;
        }

        newSpeed = initSpeed + speedIncrement;
        
        return newSpeed;
    }

    void simulateMovement(List<boidController> boids, float time)
    {
        int maxNumOfRandomFish = (int) Random.Range(1f, numberOfFish/2f);

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

            if (!b_i.randomBehaviour && Random.value > .99f && numberOfRandomFish < maxNumOfRandomFish)
            {
                numberOfRandomFish += 1;
                b_i.randomBehaviour = true;

                b_i.elapsedTime = 0f;
                b_i.goalTime = Random.Range(1f, 2f);
                b_i.timeToMaxSpeed = Random.Range(0.1f, 0.5f);

                b_i.randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                b_i.randomDirection = b_i.randomDirection.normalized;

                b_i.randomWeight = Random.Range(1f, 10f);
                b_i.randomSteeringSpeed = Random.Range(2f, 5f)*b_i.steeringSpeed;
                b_i.originalSpeed = b_i.speed;
                b_i.originalSteeringSpeed = b_i.steeringSpeed;
            }

            if (b_i.randomBehaviour)
            {
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
                    b_i.steeringSpeed = updateSpeed(b_i, b_i.randomSteeringSpeed, b_i.originalSteeringSpeed);
                    b_i.elapsedTime += time;
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

            Vector3 cameraDirection = mainCam.transform.position - b_i.go.transform.position;
            cameraDirection = -cameraDirection.normalized;

            if (randomDirection != Vector3.zero && simAreaBounds.Contains(b_i.go.transform.position))
            {
                float cameraDistance = Vector3.Distance(mainCam.transform.position, b_i.go.transform.position);
                steering += cameraDirection*cameraDistance;
                steering += randomDirection*randomWeight;
                
                Debug.DrawRay(b_i.go.transform.position, steering, Color.red);
            } 
            else
            {
                Vector3 boundsDirection = Vector3.zero;
                boundsDirection = simAreaBounds.center - b_i.go.transform.position;
                boundsDirection = boundsDirection.normalized;
                steering += boundsDirection;
                steering += cameraDirection;
                steering += separationDirection*S;
                steering += alignmentDirection*M;
                steering += cohesionDirection*K;
                steering += leaderDirection*L;
                Debug.DrawRay(b_i.go.transform.position, steering, Color.green);
            }

            b_i.go.transform.rotation = Quaternion.RotateTowards(
                b_i.go.transform.rotation, 
                Quaternion.LookRotation(steering), 
                b_i.steeringSpeed * time);
           
            b_i.go.transform.position += b_i.go.transform.TransformDirection(new Vector3(b_i.speed, 0, 0))* time;
        }
    }

    void generateControlList()
    {
        conditionsControl controlVariant;

        //000
        controlVariant.background = 0; 
        controlVariant.fog = 0;
        controlVariant.distractors = 0;
        controlList.Add(controlVariant);
        
        if (useDistractors)
        {
            //001
            controlVariant.background = 0; 
            controlVariant.fog = 0;
            controlVariant.distractors = 1;
            controlList.Add(controlVariant);
        }

        
        if (useFog)
        {
            //010
            controlVariant.background = 0; 
            controlVariant.fog = 1;
            controlVariant.distractors = 0;
            controlList.Add(controlVariant);
        }

        if (useFog && useDistractors)
        {
            //011
            controlVariant.background = 0; 
            controlVariant.fog = 1;
            controlVariant.distractors = 1;
            controlList.Add(controlVariant);
        }        


        if (useBackground)
        {
            //100
            controlVariant.background = 1; 
            controlVariant.fog = 0;
            controlVariant.distractors = 0;
            controlList.Add(controlVariant);
        }

        
        if (useBackground && useDistractors)
        {
            //101
            controlVariant.background = 1; 
            controlVariant.fog = 0;
            controlVariant.distractors = 1;
            controlList.Add(controlVariant);
        }

        if (useBackground && useFog)
        {
            //110
            controlVariant.background = 1; 
            controlVariant.fog = 1;
            controlVariant.distractors = 0;
            controlList.Add(controlVariant);
        }
        
        if (useBackground && useFog && useDistractors)
        {
            //111
            controlVariant.background = 1; 
            controlVariant.fog = 1;
            controlVariant.distractors = 1;
            controlList.Add(controlVariant);
        }

    }

    void setupFolderStructure()
    {
        string controlString = "";
        rootDir = "synthData/" + datasetDir;

        if (control.background != 0 || control.fog != 0 || control.distractors != 0) controlString += "_";

        if (control.background == 1){
            controlString += "B";
        } 

        if (control.fog == 1){
            controlString += "F";
        }

        if (control.distractors == 1){
            controlString += "D";
        }
        rootDir = rootDir + controlString + "/train/";

        if (System.IO.Directory.Exists(rootDir))
        {
            System.IO.Directory.Delete(rootDir, true);
            System.IO.Directory.CreateDirectory(rootDir);
        } else {
             System.IO.Directory.CreateDirectory(rootDir);
        }

    }

    void getNewBoidParameters()
    {
        K = Random.Range(0.75f, 1.25f);
        S = Random.Range(0.75f, 1.25f);
        M = Random.Range(0.75f, 1.25f);
        L = Random.Range(0.75f, 1.25f);
        boidNoClumpingArea = Random.Range(7.5f, 12.5f);
        boidLocalArea = Random.Range(15f, 25f);
    }

    void Awake()
    {
        watch.Start();
        generateControlList();
        control = controlList[controlIdx];
        setupFolderStructure();

        mainCam = GameObject.Find("Fish Camera").GetComponent<Camera>();
    
        simArea = GameObject.Find("Simulation area");
        simArea.transform.position = new Vector3(0, 0, simAreaSize.z/2f);
        simArea.transform.localScale = simAreaSize;
        UnityEngine.Physics.SyncTransforms();
        simAreaBounds = simArea.GetComponent<Collider>().bounds;
        simArea.SetActive(false);

        videoFiles = System.IO.Directory.GetFiles(videoDir,"*.mp4");
        vp = GameObject.Find("Video player").GetComponent<VideoPlayer>();
        setVideoProperties();

        Debug.Log($"Video Height: {img_height} -- Width: {img_width} -- FPS: {FPS} -- BitRate: {bitrate}");

        bakedMesh = new Mesh();

        screenshotTex = new Texture2D(img_width, img_height, TextureFormat.RGB24, false);

        deltaTime = (float) 1/FPS;
    }
    
    void Start()
    {
        Application.targetFrameRate = 25;
        //Set up a first scene
        if (control.background == 1) randomizeVideo();
        generateFogColor();
        mainCam.backgroundColor = fogColor;
        if (control.fog == 1) randomizeFog();         
        if (control.distractors == 1) generateDistractors();
        getNewBoidParameters();
        for (int i = 0; i < numberOfSwarms; i++)
        {
            instantiateFish(i);
        }
        addNewSequence();
    }

    void Update()
    {   
        print("control background " + control.background.ToString());
        print("control fog " + control.fog.ToString());
        print("control distractors " + control.distractors.ToString());
        if (sequence_image == sequence_length)
        {      
            CleanUp();

            if (control.background == 1) randomizeVideo();
            generateFogColor();
            mainCam.backgroundColor = fogColor;
            if (control.fog == 1) randomizeFog();         
            if (control.distractors == 1) generateDistractors();
            getNewBoidParameters();
            for (int i = 0; i < numberOfSwarms; i++)
            {
                instantiateFish(i);
            }
            addNewSequence();
        } 
        
        if(vp.isPlaying || control.background == 0)
        {
            sequence_image += 1;
            //vp.Pause();
            simulateMovement(boidsList, deltaTime);
            if (control.distractors == 1) updateDistractors();
        }
    }


    void LateUpdate()
    {
        if (sequence_number == sequence_goal+1)
        {  
            controlIdx++;
            Debug.Log($"Execution time: {watch.Elapsed} seconds");
            watch.Restart();
            if (controlIdx == controlList.Count)
            {
                Debug.Log("All sequences were generated");
                UnityEditor.EditorApplication.isPlaying = false;
                videoEncoder.Dispose();
                videoEncoder = null;
            }
            else
            {
                Debug.Log("New simulation conditions");
                control = controlList[controlIdx];
                sequence_number = 0;
                setupFolderStructure();
                //Set up a new scene with new control conditions
                CleanUp();
                if (control.background == 1) randomizeVideo();
                generateFogColor();
                mainCam.backgroundColor = fogColor;
                if (control.fog == 1) randomizeFog();         
                if (control.distractors == 1) generateDistractors();
                getNewBoidParameters();
                for (int i = 0; i < numberOfSwarms; i++)
                {
                    instantiateFish(i);
                }
                addNewSequence();
            }

        } else {

            if(vp.isPlaying || control.background == 0)
            {
                foreach (boidController b in boidsList)
                {
                    boundingBox bb = GetBoundingBoxInCamera(b.go, mainCam);
                    SaveAnnotation(bb, b.id);
                }

                SaveCameraView();

                Debug.Log("Sequence Number " + sequence_number.ToString() 
                + " Sequence Image " + sequence_image.ToString() 
                + "/" + sequence_length.ToString());
            }
        }

    }

}

