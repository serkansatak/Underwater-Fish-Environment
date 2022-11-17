using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class generateImages : MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    [SerializeField] Material mat;

    GameObject fish;
    GameObject simArea;
    Camera mainCam;
    
    List<GameObject> distractors_list = new List<GameObject>(); 

    int imgIdx;
    string rootDir = "images/";
    //List<string> imgName = new List<string>();
    //string[] imgName = new string[] {"null.png", "b.png", "f.png", "d.png", "fd.png", "bf.png", "bd.png", "bfd.png"};

    Color fogColor;

    bool fog = false;
    bool distractors = false;
    bool background = false;

    string videoDir = "Assets/videos";
    string[] videoFiles;
    VideoPlayer vp;


    void generateDistractors()
    {
        //getNewCurrentDirection();
        int number_of_distractors = (int) Random.Range(50, 500);

        for (int i = 0; i < number_of_distractors; i++)
        {
            //DynamicGameObject dgo = new DynamicGameObject();
            //dgo.speed = Random.Range(1, 10);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = mainCam.ViewportToWorldPoint( new Vector3(
            UnityEngine.Random.Range(0.0f, 1f), 
            UnityEngine.Random.Range(0.0f, 1f),
            UnityEngine.Random.Range(10f, 50f)));

            sphere.name = "distractor_" + i.ToString();

            sphere.transform.parent = transform;
            sphere.transform.localScale = Vector3.one * Random.Range(0.01f, 1f);
            //dgo.go = sphere;
            Renderer rend = sphere.GetComponent<Renderer>();
            rend.material = mat;
            //float rnd_color_seed = Random.Range(75.0f, 225.0f);
            Color rnd_albedo = new Color(
                Random.Range(171f, 191f)/255,  
                Random.Range(192f, 212f)/255, 
                Random.Range(137f, 157f)/255,
                Random.Range(151f, 171f)/255);  
            rend.material.color = rnd_albedo;
            rend.material.SetFloat("_TranspModify", Random.Range(0.0f, 0.1f));
            distractors_list.Add(sphere);
        }
    }

    string getFilename()
    {
        string filename;
        if (!fog && !distractors && !background) 
        {
            filename = rootDir + "null.png";
            return filename;
        } 

        string imgName = "";
        if (fog) imgName += "f";
        if (distractors) imgName += "d";
        if (background) imgName += "b";
        imgName += ".png";
        filename = rootDir + imgName;

        return filename;
    }

    void randomizeFog()
    {
        simArea.SetActive(true);
        Renderer simAreaRenderer = simArea.GetComponent<Renderer>();
        //simAreaRenderer.material = mat;
        simAreaRenderer.material.color = fogColor;
        float fogIntensityMax = 0.1f;
        float fogIntensityMin = 0.8f;
        float fogIntensity = Random.Range(fogIntensityMin, fogIntensityMax);
        simAreaRenderer.material.SetFloat("_TranspModify", fogIntensity);
    }

    void generateFogColor()
    {
        if (Random.value < 0.5f)
        {
            //Base values 181, 202, 147, 161
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

    void printConditios()
    {
        string toPrint = "";
        toPrint += "Fog: " + fog.ToString() + " ";
        toPrint += "Distractors: " + distractors.ToString() + " ";
        toPrint += "Background: " + background.ToString();
        print(toPrint);
    }

    void randomizeVideo()
    {
        string random_file = videoFiles[Random.Range(0, videoFiles.Length)];
        vp.url = random_file;
        vp.Prepare();
    }

    void Awake()
    {
        videoFiles = System.IO.Directory.GetFiles(videoDir,"*.mp4");
        vp = GameObject.Find("Video player").GetComponent<VideoPlayer>();

        mainCam = GameObject.Find("Fish Camera").GetComponent<Camera>();   

        simArea = GameObject.Find("simArea");
        Vector3 simAreaSize = new Vector3(150, 60, 180);
        simArea.transform.position = new Vector3(0, 0, simAreaSize.z/2f);
        //simArea.transform.position = mainCam.transform.position;
        simArea.transform.localScale = new Vector3(150, 60, 180);
        simArea.SetActive(false);

        
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //imgName = ["null.png", "b.png", "f.png", "d.png", "fd.png", "bf.png", "bd.png", "bfd.png"];
        //imgIdx = 0;

        fish = Instantiate(fishPrefab);
        fish.transform.position = mainCam.ViewportToWorldPoint( new Vector3( 0.5f, 0.35f, 10f));
        fish.transform.rotation = Quaternion.Euler(0, 0, 0);

        generateFogColor();
        mainCam.backgroundColor = fogColor;
        //if (control.fog == 1) randomizeFog();  
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("s"))
        {
            print("Saving image");
            string filename = getFilename();
            print(filename);
            ScreenCapture.CaptureScreenshot(filename);
        }

        if (Input.GetKeyDown("b"))
        {
            if (background) 
            {
                background = false;
                vp.Stop();
    
            } 
            else 
            {
                background = true;
                randomizeVideo();
            }    
        }

        if (Input.GetKeyDown("f"))
        {
            if (fog) 
            {
                fog = false;
                simArea.SetActive(false);
            } 
            else 
            {
                fog = true;
                randomizeFog();
            } 
        }

        if (Input.GetKeyDown("d"))
        {
            if (distractors) 
            {
                distractors = false;
                foreach (GameObject go in distractors_list) Destroy(go);
                distractors_list.Clear();
            } 
            else 
            {
                distractors = true;
                generateDistractors();
            } 
        }
        
    }

    void LateUpdate()
    {
        printConditios();
    }
}
