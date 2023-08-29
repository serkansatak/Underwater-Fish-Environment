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
    
    // Other public attributes 
    #region Public Attr Dump
    string[] videoFiles;
    string rootDir;
    string datasetDir;
    string imageFolder;
    Mesh bakedMesh;
    float deltaTime;

    // Simulation Area

    System.Random sysRand = new System.Random();
    
    GameObject simArea;
    Renderer simAreaRenderer;
    Bounds simAreaBounds;
    Vector3 simAreaSize = new Vector3(150, 60, 180);

    string normalizedTurbidIntensity;
    string numberOfDistractors;
    int number_of_distractors;
    List<GameObject> distractors_list = new List<GameObject>(); 

    string gtFolder;
    string gtFile;
    int sequence_image = 0;
    bool sequenceSet = false;
    bool vpSet = false;
    bool distractorsSet = false;
    bool sequenceDone = false;
    bool setVidEncoder = true;
    #endregion

    // Unity Native function overrides
    #region Unity Functions (Awake - Start - Update)
    void Awake()
    {
        generateControlList();
        mainCam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        
        simArea = GameObject.Find("SimulationArea");
        simArea.transform.position = new Vector3(0, 0, simAreaSize.z/2f);
        simArea.transform.localScale = simAreaSize;
        UnityEngine.Physics.SyncTransforms();
        simAreaBounds = simArea.GetComponent<Collider>().bounds;
        simArea.SetActive(false);

        vp = GameObject.Find("Video player").GetComponent<VideoPlayer>();
        setVideoProperties();

        screenshotTex = new Texture2D((int)vpAttr.width, (int)vpAttr.height, TextureFormat.RGB24, false);
        deltaTime = (float) 1/vpAttr.FPS;
    }

    void Start()
    {   Application.runInBackground = true;
        Application.targetFrameRate = 10;
        Debug.Log($"FPS : {vpAttr.FPS * vp.playbackSpeed}");
        vp.Play();
        vp.Pause();
    }

    void Update()
    {
        if (!sequenceDone){
            StartCoroutine(FrameUpdate());
        }
        else {
            sequenceDone = false;
            Debug.Log("Control Idx : " + controlIdx);
            vpSet = false;
            sequenceSet = false;
            controlIdx++;  
        }
    }
    #endregion

    // New sequence starter and CleanUp
    #region Sequence and Clean-Up
    void CleanUp()
    {
        vp.Stop();
        // Encoder boşaltılacak, vp başa sarılacak, distractor list boşaltılacak
        foreach (GameObject go in distractors_list)
        {
            Destroy(go);
        }
        distractors_list.Clear();
        if (videoEncoder != null)
        {
            videoEncoder.Dispose();
            videoEncoder = null;
        }
        sequence_image = 0;
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

        if (setVidEncoder) 
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

            string vidName = this.generateVideoName();

            videoFileName = new_sequence + $"/{vidName}.mp4";

            Debug.Log(videoFileName);
            Debug.Log(datasetDir);
            videoEncoder = new MediaEncoder(videoFileName, encoderAttributes);
        }
        
    }
    #endregion

    // Functions related to frame extraction
    #region Frame Extraction
    void OnFrameReady(VideoPlayer vp_, long frameIdx)
    {
        if (control.distractors == 1){ updateDistractors();}
        vp_.frame = frameIdx;
        int tmpIdx = (int)frameIdx + 1;
        string filename = imageFolder + "/" + tmpIdx.ToString().PadLeft(4,'0') + ".jpg";

        screenRenderTexture = RenderTexture.GetTemporary((int)vpAttr.width, (int)vpAttr.height, 24);
        mainCam.targetTexture = screenRenderTexture;
        mainCam.Render();
        RenderTexture.active = screenRenderTexture;

        screenshotTex.ReadPixels(new Rect(0, 0, (int)vpAttr.width, (int)vpAttr.height), 0, 0);
        RenderTexture.active = null; // JC: added to avoid errors
        screenRenderTexture = null;
        RenderTexture.ReleaseTemporary(screenRenderTexture);
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
        if (setVidEncoder)
        {
            videoEncoder.AddFrame(videoTex);
        }
        byte[] byteArray = screenshotTex.EncodeToJPG();
        System.IO.File.WriteAllBytes(filename, byteArray);

        Debug.Log("Control Number " + System.Convert.ToString(controlIdx, 2).PadLeft(2,'0')
        + " Sequence Image " + tmpIdx.ToString() 
        + "/" + sequence_length.ToString());
        if (tmpIdx == sequence_length){
            vp_.Stop();
            sequenceDone=true;
        }
    }

    public IEnumerator FrameUpdate() 
    {
        sequenceDone = false;
        Debug.Log("come on");
        if (!sequenceSet)
        {
            if (controlIdx == controlList.Count)
            {
                CleanUp();
                Debug.Log("All sequences were generated");
                UnityEditor.EditorApplication.isPlaying = false;
            }
            else
            {
                control = controlList[controlIdx];
                setupFolderStructure();
                CleanUp();
                generateTurbidColor();
                if (control.turbidity == 1) {mainCam.backgroundColor=turbidColor; randomizeTurbid();};         
                if (control.distractors == 1) generateDistractors();
                addNewSequence();
                sequenceSet = true;
            }
        }
        if (!vpSet)
        {
            Debug.Log("Waiting for VideoPlayer...");
            yield return new WaitForSeconds(2f);
            vpSet = true;
        }
        if (!vp.isPrepared){
            Debug.Log("isPrepared ??");
            yield return null;
        }

        bool distractorCheck = control.distractors==1 ? true : false;
        bool tmpBool = false;
        
        vp.Play();
        Debug.Log("SeqImage After : " + sequence_image);
        Debug.Log("TmpBool " + tmpBool);
    }
    #endregion

    // Functions Related to VideoPlayer and Video Player Attributes
    #region VideoPlayer Settings and vpAttr
    void Prepared(VideoPlayer vp_) => vp_.Pause();

    void setVideoProperties() 
    {
        vp.sendFrameReadyEvents = true;
        vp.frameReady += OnFrameReady;
        vp.prepareCompleted += Prepared;

        long fileSize = new System.IO.FileInfo(vp.clip.originalPath).Length;
        float duration = (float)vp.frameCount / vp.frameRate;
        float bitrate_ = fileSize / duration * 8.0f / 1000000.0f; // in Mbps
        uint bitrate = (uint)bitrate_ * 1000000;

        Debug.Log("Main Cam Height : " + mainCam.orthographicSize * 2f);
        Debug.Log("Main Cam Width : " + mainCam.orthographicSize * mainCam.aspect * 2f);

        this.sequence_length = (int)vp.frameCount;
        this.vpAttr = new VideoPlayerAttr((uint)vp.frameRate, vp.height, vp.width, bitrate);
    }
    #endregion

    // Statistical Distribution Functions
    #region Distribution Functions

    public float NextExp(double lamda, bool reversed = false)
    {
        double x = (double)sysRand.Next(0,5000) / (double)1000;
        double y;
        if (reversed)
        {
            y = lamda * Math.Exp((x-5)*lamda);
            y = System.Math.Min(y,1);
            y = System.Math.Max(y,0);
        }
        else
        {   
            y = lamda * Math.Exp(-x * lamda);
            y = System.Math.Min(y,1);
            y = System.Math.Max(y,0);
        }
        return (float)y;
    }

    public double LogNormal(double x, double mu, double sigma)
    {
        double lnx = System.Math.Log(x);
        double exp_part = System.Math.Exp( -(System.Math.Pow((lnx-mu), 2) / (2*System.Math.Pow(sigma,2)) ) );
        double reg_part = 1 / (x * sigma * System.Math.Sqrt(2 * System.Math.PI) );
        double y = exp_part * reg_part;
        return y;
    }

    public float NextLogNormal(double mu = 0.25, double sigma = 1, bool scaleToOne = true, double cutoff = 0)
    {
        //double x = (double)Random.Range((float)0.0001, (float)5);
        if (cutoff == 0) cutoff = 0.01;
        
        Tuple<double, double> cutoffValues = getBoundaryValues(cutoff, mu, sigma);

        //int val1 = (int)(cutoffValues.Item1*1000);
        //int val2 = (int)(cutoffValues.Item2*1000);

        //double x = (double)sysRand.Next(val1, val2) / (double)1000;


        double x = (double)sysRand.NextDouble() * (cutoffValues.Item2 - cutoffValues.Item1) + cutoffValues.Item1;

        if (x == 0) x = 0.0001;
        double y = LogNormal(x, mu, sigma);

        Tuple<double,double> peaks = getPeakValue(mu, sigma, 0.01);

        if (scaleToOne) 
        {
            y = (y-cutoff) / (peaks.Item2-cutoff);
            //y = System.Math.Min(y, 1);
        }
        return (float)y;
    }

    public float GetRandomLogNormal(float lowerBound, float upperBound, bool reversed = false, double mean = 0.25, double stdDev = 1, double cutoff = 0)
    {
        float randLN = NextLogNormal((double)mean, (double)stdDev, true, cutoff);
        if (reversed)
        {
            return upperBound - (randLN * (upperBound - lowerBound));
        } else 
        {
            return randLN * (upperBound - lowerBound) + lowerBound;
        }
    }

    public Tuple<double,double> getPeakValue(double mean, double stdDev, double step=0.01)
    {
        double peakValue = 0;
        double peak = 0;
        int counter = 0;
        double i = 0;
        while(true)
        {
            double y = LogNormal(i, mean, stdDev);
            if (y > peak)
            {
                peak = y;
                peakValue = i;
                counter = 0;
            }
            else {
                counter+= 1;
            }
            i += step;
            if (counter > 50) break;
        }
        return Tuple.Create(peakValue, peak);
    }

    public Tuple<double, double> getBoundaryValues(double cutoff, double mean, double stdDev, double step=0.01)
    {
        double i = 0;
        List<double> values = new List<double>();
        
        for (int j=0; j<800; j++)
        {
            double y = LogNormal(i, mean, stdDev);

            if (values.Count == 0){
                if (y>cutoff) values.Add(i);
            }
            else if (values.Count == 1){
                if (y<cutoff) values.Add(i);
            }
            else {
                break;
            }

            i += step;
        }

        if (values.Count == 0) {
            Debug.Log("No values found");
            Debug.Log("Cutoff : " + cutoff);
            Debug.Log("Mean : " + mean);
            Debug.Log("StdDev : " + stdDev);
        }

        return Tuple.Create(values[0], values[1]);
    }

    #endregion

    // Distractor related functions
    #region Distractor Functions
    void updateDistractors()
    {
        foreach (GameObject go in distractors_list)
        {
            go.transform.position += new Vector3(
                UnityEngine.Random.Range(-1f, -1f),
                UnityEngine.Random.Range(-0.01f, 0.01f),
                0
            );

            getDistractorsBackToScene(go);

            // go.transform.position += mainCam.ViewportToWorldPoint( new Vector3(
            // UnityEngine.Random.Range(0.0f, 0.00001f), 
            // UnityEngine.Random.Range(0.0f, 0.00001f),
            // UnityEngine.Random.Range(0, 0)));
        }
        distractorsSet = true;
    }

    

    void getDistractorsBackToScene(GameObject go)
    {
        float camHeight = mainCam.orthographicSize * 2f;
        float camWidth = camHeight * mainCam.aspect;

        float camLeft = mainCam.transform.position.x - (camWidth / 2f);
        float camRight = mainCam.transform.position.x + (camWidth / 2f);
        float camTop = mainCam.transform.position.y + (camHeight / 2f);
        float camBottom = mainCam.transform.position.y - (camHeight / 2f);

        Bounds objectBounds = new Bounds(go.transform.position, go.transform.localScale);

        if (objectBounds.min.x < camLeft * 2) 
        {
            // Move object to the right side of the screen
            float newX = go.transform.position.x + camWidth *2;
            go.transform.position = new Vector3(newX, go.transform.position.y, go.transform.position.z);
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
            GetRandomLogNormal(0,1,false,0.25,0.4,0.1),
            UnityEngine.Random.Range(10f, 30f)));

            sphere.name = "distractor_" + i.ToString();

            sphere.transform.parent = transform;
            sphere.transform.localScale = Vector3.one * GetRandomLogNormal(0.2f, 0.5f);
            Renderer rend = sphere.GetComponent<Renderer>();
            rend.material = distractorMaterial;
            Color rnd_white = new Color(
                Random.Range(220f, 225f)/255,
                Random.Range(220f, 255f)/255,
                Random.Range(220f, 255f)/255,
                Random.Range(100f, 200f)/255 
            );
            rend.material.color = rnd_white;
            rend.material.SetFloat("_TranspModify", Random.Range(0f, 1f));
            distractors_list.Add(sphere);
        }
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
        float turbidIntensityMin = 0.3f;
        float turbidIntensityMax = 0.5f;
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

    string generateVideoName()
    {
        string controlString = "";
        if (control.turbidity != 0 || control.distractors != 0) controlString += "_";

        if (control.turbidity == 1){
            controlString += "T";
        }

        if (control.distractors == 1){
            controlString += "D";
        }
        return datasetDir.Remove(datasetDir.Length - 1, 1) + controlString;
    }
}
