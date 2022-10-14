//https://answers.unity.com/questions/1180994/combining-render-textures.html 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class saver : MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    GameObject Fish;
    Camera main_cam;
    Camera background_cam;
    VideoPlayer vp;
    int img_height = 544;
    int img_width = 960;
    string dataDir = "data";
    string videoDir = "Assets/videos";
    string[] videoFiles;
    
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
        //currFish.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
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

    Texture2D GetBackgroundTexture()
    {
        string filename = dataDir + "/" + Time.frameCount.ToString() + "_bg.png";
        RenderTexture rt = RenderTexture.GetTemporary(background_cam.pixelWidth, background_cam.pixelHeight, 24);
        background_cam.targetTexture = rt;
        RenderTexture.active = rt;
        background_cam.Render();

        Texture2D bgTex = new Texture2D(background_cam.pixelWidth, background_cam.pixelHeight, TextureFormat.RGB24, false);
        //screenshotTex.Reinitialize(main_cam.pixelWidth, main_cam.pixelHeight);
        bgTex.ReadPixels(new Rect(0, 0, background_cam.pixelWidth, background_cam.pixelHeight), 0, 0);

        background_cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        RenderTexture.ReleaseTemporary(rt);
        rt = null;
        Destroy(rt);

        byte[] bytes = bgTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, bytes);

        return bgTex;

    }

    Texture2D GetFishTexture()
    {
        string filename = dataDir + "/" + Time.frameCount.ToString() + ".png";
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

        byte[] bytes = screenshotTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, bytes);
        
        return screenshotTex;
    }

    void SaveImage()
    {
        string filename = dataDir + "/" + Time.frameCount.ToString() + ".png";
        ScreenCapture.CaptureScreenshot(filename);
    }

     void SaveCameraView()
    {
        //Camera cam1 = background_cam;
        //Camera cam2 = main_cam;
        string filename = dataDir + "/" + Time.frameCount.ToString() + ".png";
        
        RenderTexture screenRenderTexture = RenderTexture.GetTemporary(img_width, img_height, 24);
        main_cam.targetTexture = screenRenderTexture;
        //background_cam.targetTexture = screenRenderTexture;
        //background_cam.Render();
        main_cam.Render();
        RenderTexture.active = screenRenderTexture;

        Texture2D screenshotTex = new Texture2D(img_width, img_height, TextureFormat.RGB24, false);
        //screenshotTex.Resize(Screen.width, Screen.height);
        screenshotTex.ReadPixels(new Rect(0, 0, img_width, img_height), 0, 0);

        //background_cam.targetTexture = null;
        //main_cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        RenderTexture.ReleaseTemporary(screenRenderTexture);
        screenRenderTexture = null;
        Destroy(screenRenderTexture);

        byte[] byteArray = screenshotTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, byteArray);
        Destroy(screenshotTex);
    }

    void randomizeVideo()
    {
        //vp.Stop();
        string random_file = videoFiles[Random.Range(0, videoFiles.Length)];
        //Debug.Log(files[Random.Range(0,files.Length)]);
        vp.url = random_file;
        //vp.url = "Assets/videos/converted/video_1_conv.ogv";
        vp.Prepare();
    }


    // Start is called before the first frame update
    void Awake()
    {
        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        background_cam = GameObject.Find("Background Camera").GetComponent<Camera>();
        background_cam.enabled = false;

        Fish = spawn_fish();

        videoFiles = System.IO.Directory.GetFiles(videoDir,"*.avi");
        vp = GameObject.Find("Video player").GetComponent<VideoPlayer>();
        randomizeVideo();

        if (System.IO.Directory.Exists(dataDir))
        {
            System.IO.Directory.Delete(dataDir, true);
            System.IO.Directory.CreateDirectory(dataDir);
        } else {
             System.IO.Directory.CreateDirectory(dataDir);
        }
    }

    void Start(){
        
    }

    // Update is called once per frame
    void Update()
    {
        //GetBackgroundTexture();
        //GetFishTexture();
        if(vp.isPlaying){
            //SaveImage();
            SaveCameraView();
        }
        
    }
}
