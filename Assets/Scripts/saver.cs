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
    string dataDir = "data";
    
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




    // Start is called before the first frame update
    void Awake()
    {
        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        background_cam = GameObject.Find("Background Camera").GetComponent<Camera>();
        Fish = spawn_fish();
        vp = GameObject.Find("Video player").GetComponent<VideoPlayer>();
        vp.Prepare();

        if (System.IO.Directory.Exists(dataDir))
        {
            System.IO.Directory.Delete(dataDir, true);
            System.IO.Directory.CreateDirectory(dataDir);
        } else {
             System.IO.Directory.CreateDirectory(dataDir);
        }
    }

    void Start(){}

    // Update is called once per frame
    void Update()
    {
        //GetBackgroundTexture();
        //GetFishTexture();
        if(vp.isPlaying){
            SaveImage();
        }
        
    }
}
