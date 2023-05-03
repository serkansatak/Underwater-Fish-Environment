
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor.Media;
using UnityEditor;
 
public class ExtractFrames : MonoBehaviour
{
    // Fields which are visible on Unity Inspector
    // Names of variables indicates what they are for.
    #region Fields and Checklist
    [SerializeField] GameObject fishPrefab;
    [SerializeField] Material distractorMaterial;
    public bool useTurbidity;
    public bool useDistractors;
    public Vector2 numFishMinMax = new Vector2(4, 50);
    public Vector2 numDistractorMinMax = new Vector2(250,2500);
    #endregion

    // Condition Control for generator.
    // Fields are related to ConditionControl
    #region Condition Controls
    conditionsControl control;
    int controlIdx = 0;
    List<conditionsControl> controlList = new List<conditionsControl>();
    #endregion
    
    // Structs which are going to be used in this context.
    #region Helpful Structs and Inner Classes
    struct boundingBox{
        public int top;
        public int bottom;
        public int left;
        public int right;
        public int height;
        public int width;
        public bool save;
    }

    struct conditionsControl{
        public int turbidity;
        public int distractors;

        public conditionsControl(int c1, int c2){
            this.turbidity = c1;
            this.distractors = c2;
        }
    }

    struct VideoPlayerAttr {
        public uint FPS;
        public uint height;
        public uint width;
        public uint bitrate;
        
        public VideoPlayerAttr(uint FPS, uint height, uint width, uint bitrate){
            this.FPS = FPS;
            this.height = height;
            this.width = width;
            this.bitrate = bitrate;
        }
    }
    #endregion

    // Camera related attributes
    #region Camera Attributes
    Camera mainCam;
    Color turbidColor;
    #endregion

    // VideoPlayer - Encoder 
    #region Video Player/Encoder Related
    VideoPlayer vp;
    VideoPlayerAttr vpAttr;
    int sequence_length;
    string videoFileName;
    private MediaEncoder videoEncoder;
    private VideoTrackEncoderAttributes encoderAttributes;
    RenderTexture screenRenderTexture;
    Texture2D screenshotTex;
    #endregion
    
    string videoDir = "Assets/Videos";
    string[] videoFiles;
    string rootDir;
    string datasetDir;
    string imageFolder;
    Mesh bakedMesh;
    float deltaTime;

    // Simulation Area
    GameObject simArea;
    System.Random sysRand = new System.Random();
    Renderer simAreaRenderer;
    Bounds simAreaBounds;
    Vector3 simAreaSize = new Vector3(150, 60, 180);

    string normalizedTurbidIntensity;
    string numberOfDistractors;
    int number_of_distractors;
    List<GameObject> distractors_list = new List<GameObject>(); 
    int sequence_number = 0;
    int sequence_goal = 1;

    string gtFolder;
    string gtFile;
    int sequence_image = 0;

    // Unity functions like Awake-Start-Update-LateUpdate
    #region Unity Functions
    void Awake()
    {
        generateControlList();
        control = controlList[controlIdx];
        // Set Camera
        mainCam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        // Get videoFiles and set VideoPlayer
        vp = GameObject.Find("Video player").GetComponent<VideoPlayer>();
        // Setup folder structure
        setupFolderStructure();
    }

    void Start() 
    {
        setVideoProperties();
        startNewSequence();    
        // Set screenshot Texture and deltaTime
        screenshotTex = new Texture2D((int)vpAttr.width, (int)vpAttr.height, TextureFormat.RGB24, false);
        deltaTime = (float) 1/vpAttr.FPS;
        //vp.Play();
    }

    
    void Update()
    {   /*
        vp.Pause();
        print("control turbidity " + control.turbidity.ToString());
        print("control distractors " + control.distractors.ToString());
        if (sequence_image == sequence_length)
        {      
            CleanUp();
            startNewSequence();
        }
        else
        {
            vp.Play();
            sequence_image += 1;
            if (control.distractors == 1) updateDistractors();
        }
        */
        vp.Play();
    }
    

    void conditionUpdate()
    {
        controlIdx++;
        if (controlIdx == controlList.Count)
        {
            // CleanUp
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
            startNewSequence();
        }
        vp.frame = sequence_image;
    }

    
    void LateUpdate()
    {
        /*
        if (sequence_image == sequence_length)
        {
            conditionUpdate();
        }
        else {
            {
                SaveCameraView();
                //Debug.Log("Sequence Number " + sequence_number.ToString() 
                //+ " Sequence Image " + sequence_image.ToString() 
                //+ "/" + sequence_length.ToString());
                Debug.Log("Clock Speed : " + vp.clockTime.ToString());
            }
        }
        //vp.Play();
        */
    }
    #endregion

    void startNewSequence()
    {
        generateTurbidColor();
        mainCam.backgroundColor = new Color(0,0,0,0);
        if (control.turbidity == 1) {mainCam.backgroundColor=turbidColor; randomizeTurbid();};         
        if (control.distractors == 1) generateDistractors();
        addNewSequence();
    }

    void CleanUp()
    {
        // Encoder boşaltılacak, vp başa sarılacak, distractor list boşaltılacak
        foreach (GameObject go in distractors_list)
        {
            Destroy(go);
        }
        distractors_list.Clear();
        //simArea.SetActive(false);
    }

    void Prepared(VideoPlayer vp) => vp.Pause();
 
    void FrameReady(VideoPlayer videoPlayer, long frameIndex)
    {
        sequence_image++;
        videoPlayer.frame = frameIndex;
        Debug.Log("FrameReady " + frameIndex);
        var textureToCopy = vp.texture;
        SaveCameraView();
        if (sequence_image == sequence_length) 
        {
            sequence_image = 0;
            conditionUpdate();
        }
        frameIndex ++;
    }

    void SaveCameraView()
    {
        string filename = imageFolder + "/00000" + sequence_image.ToString() + ".jpg";
        
        screenRenderTexture = RenderTexture.GetTemporary((int)vpAttr.width, (int)vpAttr.height, 24);
        mainCam.targetTexture = screenRenderTexture;
        mainCam.Render();
        RenderTexture.active = screenRenderTexture;

        screenshotTex.ReadPixels(new Rect(0, 0, (int)vpAttr.width, (int)vpAttr.height), 0, 0);
        RenderTexture.active = null; 
        // JC: added to avoid errors
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

        byte[] byteArray = screenshotTex.EncodeToJPG();
        System.IO.File.WriteAllBytes(filename, byteArray);
    }

    public void setVideoProperties() 
    {
        //vp.playOnAwake = false;
        //vp.Play();
        //vp.renderMode = VideoRenderMode.APIOnly;
        vp.prepareCompleted += Prepared;
        vp.sendFrameReadyEvents = true;
        vp.frameReady += FrameReady;
        vp.Prepare();

        long fileSize = new System.IO.FileInfo(vp.clip.originalPath).Length;
        float duration = (float)vp.frameCount / vp.frameRate;
        float bitrate_ = fileSize / duration * 8.0f / 1000000.0f; // in Mbps
        uint bitrate = (uint)bitrate_ * 1000000;

        this.sequence_length = (int)vp.frameCount;
        this.vpAttr = new VideoPlayerAttr((uint)vp.frameRate, vp.height, vp.width, bitrate);
    }

    void addNewSequence()
    {   

        
        sequence_image = 0;

        string new_sequence = rootDir;

        if (System.IO.Directory.Exists(new_sequence))
        {
            System.IO.Directory.Delete(new_sequence, true);
            System.IO.Directory.CreateDirectory(new_sequence);
        } else {
            System.IO.Directory.CreateDirectory(new_sequence);
        }

        imageFolder = new_sequence + "/img";
        gtFolder = new_sequence + "/gt";
        System.IO.Directory.CreateDirectory(imageFolder);
        System.IO.Directory.CreateDirectory(gtFolder);
        gtFile = gtFolder + "/gt.txt";
        string iniFile = new_sequence + "/seqinfo.ini";
        
        if (control.distractors == 0) numberOfDistractors = "0";
        if (control.turbidity == 0) normalizedTurbidIntensity = "0";

        string seqInfo = "[Sequence]\n" + 
            "name=" + datasetDir.ToString() +"\n" +
            "imDir=img\n" +
            "frameRate=" + vpAttr.FPS.ToString() + "\n" +
            "seqLength=" + sequence_length.ToString() + "\n" +
            "imWidth=" + vpAttr.width.ToString() + "\n" +
            "imHeight=" + vpAttr.height.ToString() + "\n" +
            "imExt=.jpg\n" +
            "turbidIntensity=" + normalizedTurbidIntensity + "\n" +
            "numberOfDistractors=" + numberOfDistractors + "\n" ;
        
        using (StreamWriter writer = new StreamWriter(iniFile, true))
        {
            writer.Write(seqInfo);
        }

        Debug.Log($"Sequence name : {new_sequence}");
        setVideoEncoder(new_sequence);
        
        
    }

    void setVideoEncoder(string new_sequence)
    {
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
            
            encoderAttributes = new VideoTrackEncoderAttributes(h264Attr)
            {
                frameRate = new MediaRational((int)vpAttr.FPS),
                width = (uint)vpAttr.width,
                height = (uint)vpAttr.height,
                targetBitRate = (uint)vpAttr.bitrate,
            };

            videoFileName = new_sequence + "/output.mp4";
            videoEncoder = new MediaEncoder(videoFileName, encoderAttributes);
    }

    

    // Statistical Distribution Functions
    #region Distribution Functions
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
    #endregion

    // Distractor related functions
    #region Distractor Functions
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
    public void generateDistractors()
    {
        number_of_distractors = (int) Random.Range(numDistractorMinMax[0], numDistractorMinMax[1]);
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
            /*
            Color rnd_albedo = new Color(
                Random.Range(171f, 191f)/255,  
                Random.Range(192f, 212f)/255, 
                Random.Range(137f, 157f)/255,
                Random.Range(151f, 171f)/255);  
            
            rend.material.color = rnd_albedo;
            */
            rend.material.SetFloat("_TranspModify", Random.Range(0f, 1f));
            distractors_list.Add(sphere);
        }

        Debug.Log("Number of distractors: " + numberOfDistractors);
    }
        
    #endregion

    // Turbidity related functions
    #region Turbid Functions
    void randomizeTurbid()
    {
        simArea.SetActive(true);
        Renderer simAreaRenderer = simArea.GetComponent<Renderer>();
        simAreaRenderer.material = distractorMaterial;
        simAreaRenderer.material.color = turbidColor;
        float turbidIntensityMin = 0.1f;
        float turbidIntensityMax = 0.8f;
        float turbidIntensity = Random.Range(turbidIntensityMin, turbidIntensityMax);
        simAreaRenderer.material.SetFloat("_TranspModify", turbidIntensity);
        float normalizedIntensity = (turbidIntensity - turbidIntensityMin)/(turbidIntensityMax-turbidIntensityMin);
        normalizedTurbidIntensity = normalizedIntensity.ToString();
    }

    void generateTurbidColor()
    {
        if (Random.value < 0.5f)
        {
            turbidColor = new Color(
                Random.Range(171f, 191f)/255,  
                Random.Range(192f, 212f)/255, 
                Random.Range(137f, 157f)/255,
                Random.Range(151f, 171f)/255);
        }
        else
        {
            float rnd_color_seed = Random.Range(75.0f, 225.0f);
            turbidColor = new Color(
                rnd_color_seed/255, 
                rnd_color_seed/255, 
                rnd_color_seed/255,
                Random.Range(0.0f, 1.0f));
        }
    }
    #endregion

    // Member functions related to FS and ControlList
    #region File System and Control List
    void generateControlList()
    {
        conditionsControl controlVariant;
        //00
        controlVariant.turbidity = 0;
        controlVariant.distractors = 0;
        controlList.Add(controlVariant);
        
        if (useDistractors)
        {
            //01
            controlVariant.turbidity = 0;
            controlVariant.distractors = 1;
            controlList.Add(controlVariant);
        }
 
        if (useTurbidity)
        {
            //10
            controlVariant.turbidity = 1;
            controlVariant.distractors = 0;
            controlList.Add(controlVariant);
        }

        if (useTurbidity && useDistractors)
        {
            //11
            controlVariant.turbidity = 1;
            controlVariant.distractors = 1;
            controlList.Add(controlVariant);
        }        
        sequence_goal = controlList.Count;
    }

    void setupFolderStructure()
    {
        datasetDir = Path.GetFileNameWithoutExtension(vp.clip.originalPath) + "/";
        string controlString = "";
        rootDir = "synthData/" + datasetDir + "Synth";

        if (control.turbidity != 0 || control.distractors != 0) controlString += "_";

        if (control.turbidity == 1){
            controlString += "T";
        }

        if (control.distractors == 1){
            controlString += "D";
        }
        rootDir = rootDir + controlString;

        if (System.IO.Directory.Exists(rootDir))
        {
            System.IO.Directory.Delete(rootDir, true);
            System.IO.Directory.CreateDirectory(rootDir);
        } else {
             System.IO.Directory.CreateDirectory(rootDir);
        }

    }
    #endregion
}