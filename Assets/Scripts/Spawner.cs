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
    //Texture2D screenshotTex;

    List<GameObject> fish_inst;
    string datasetDir;
    string gt_txt;
    Color fogColor;

    void generateFogColor()
    {
        //181, 202, 147, 161
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

    Texture2D GetBackgroundTexture()
    {
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
        return bgTex;

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

    void SaveCameraView()
    {
        Camera cam1 = background_cam;
        Camera cam2 = main_cam;
        string filename = datasetDir + "/" + Time.frameCount.ToString() + ".png";
        
        RenderTexture backgroudTexture = new RenderTexture(Screen.width, Screen.height, 16);
        cam1.targetTexture = backgroudTexture;
        RenderTexture.active = backgroudTexture;
        cam1.Render();

        RenderTexture fishTexture = new RenderTexture(Screen.width, Screen.height, 16);
        cam2.targetTexture = fishTexture;
        RenderTexture.active = fishTexture;
        cam2.Render();

        Texture2D renderedTexture = new Texture2D(Screen.width, Screen.height);
        renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = null;
        byte[] byteArray = renderedTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(filename, byteArray);
    }

    void SaveImage()
    {
        string filename = datasetDir + "/" + Time.frameCount.ToString() + ".png";
        ScreenCapture.CaptureScreenshot(filename);
    }

    void SaveView()
    {
        Camera cam;
        cam = background_cam;
        RenderTexture rt_bg = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 24);
        cam.targetTexture = rt_bg;
        RenderTexture.active = rt_bg;
        cam.Render();
        Texture2D screenshotTex_bg = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);
        screenshotTex_bg.ReadPixels(new Rect(0, 0, cam.pixelWidth, cam.pixelHeight), 0, 0);

        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        RenderTexture.ReleaseTemporary(rt_bg);
        rt_bg = null;
        Destroy(rt_bg);

        cam = main_cam;
        RenderTexture rt = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 24);
        Graphics.Blit(screenshotTex_bg, rt);
        cam.targetTexture = rt;
        RenderTexture.active = rt;
        cam.Render();
        Texture2D screenshotTex = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);
        screenshotTex.ReadPixels(new Rect(0, 0, cam.pixelWidth, cam.pixelHeight), 0, 0);

        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        RenderTexture.ReleaseTemporary(rt);
        rt = null;
        Destroy(rt);

        byte[] bytes = screenshotTex.EncodeToPNG();
        string filename = datasetDir + "/" + Time.frameCount.ToString() + ".png";
        System.IO.File.WriteAllBytes(filename, bytes);


    }

    Texture2D CombineTextures(Texture2D _textureA, Texture2D _textureB)
	{
		//Create new textures
		Texture2D textureResult = new Texture2D(_textureA.width, _textureA.height);
		//create clone form texture
		textureResult.SetPixels(_textureA.GetPixels());
		//Now copy texture B in texutre A
		for (int x = 0; x<_textureB.width; x++)
		{
			for (int y = 0; y<_textureB.height; y++)
			{
				Color c = _textureB.GetPixel(x, y);
				if (c.a > 0.0f) //Is not transparent
				{
					//Copy pixel colot in TexturaA
					textureResult.SetPixel(x, y, c);
				}
			}
		}
		//Apply colors
		textureResult.Apply();
		return textureResult;
	}

    void SaveTexture(Texture2D tex)
    {
        string filename = datasetDir + "/" + Time.frameCount.ToString() + ".png";
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
        fish_inst = new List<GameObject>();
        int numberOfFish = (int)Random.Range(numFishMinMax.x, numFishMinMax.y);
        for (int i = 0; i < numberOfFish; i++)
        {   
            //float radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
            Vector3 rnd_pos = GetRandomPositionInCamera(main_cam);
            GameObject currFish = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
            currFish.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));
            //currFish.transform.rotation = Random.rotation;
            currFish.transform.parent = transform; // Parent the fish to the moverObj
            currFish.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
            currFish.GetComponent<Animator>().SetFloat("SpeedFish", Random.Range(swimAnimationMinMax.x, swimAnimationMinMax.y));

            currFish.name = "fish_" + i.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
            currFish.GetComponentInChildren<fishName>().fishN = "fish_" + i.ToString();
            fish_inst.Add(currFish);

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

        }
    }

    void CleanUp()
    {
        foreach (GameObject go in fish_inst)
        {
            Destroy(go);
        }
    }

    Vector4 GetBoundingBoxInCamera(GameObject go, Camera cam)
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

        int min_x, min_y, max_x, max_y;
        min_x = (int)min.x;
        min_y = (int)min.y;
        max_x = (int)max.x;
        max_y = (int)max.y;
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
    void SaveAnnotation(Vector4 bbox)
    {
        string frame = Time.frameCount.ToString();
        string id = "-1";
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
        using (StreamWriter writer = new StreamWriter(gt_txt, true))
        {
            writer.Write(annotation);
        }
        //Debug.Log(annotation);
    }

    void Awake()
    {
        datasetDir = "data";
        if (System.IO.Directory.Exists(datasetDir))
        {
            System.IO.Directory.Delete(datasetDir, true);
            System.IO.Directory.CreateDirectory(datasetDir);
        }

        gt_txt = Path.Combine(datasetDir + "/gt.csv");
        //Debug.Log(gt_txt);

        if (File.Exists(gt_txt))
        {
            File.Delete(gt_txt);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        //SET UP VARIABLES
        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        background_cam = GameObject.Find("Background Camera").GetComponent<Camera>();
        //screenshotTex = new Texture2D(main_cam.pixelWidth, main_cam.pixelHeight, TextureFormat.RGB24, false);
        //Fog
    }

    void Update()
    {
        Debug.Log("Iteration " + Time.frameCount.ToString());
        generateFogColor();
        randomizeBackgroundColor();
        randomizeFog(); 
        InstantiateFish();

        foreach (GameObject go in fish_inst)
        {
            Vector4 bounds = GetBoundingBoxInCamera(go, main_cam);
            SaveAnnotation(bounds);
            //Debug.Log("Bounds" + bounds);
        }
        //SaveCameraRGB(main_cam);
        //SaveCameraRGB();
        //SaveCameraView();
        //SaveImage();
        //SaveRGB();
        //SaveBackground();
        //SaveView();
        //Texture2D bg = GetBackgroundTexture();
        Texture2D fish = GetFishTexture();
        //Texture2D combined = CombineTextures(fish, bg);
        SaveTexture(fish);
        CleanUp();
    }

    /*

             foreach (Transform childObject in transform)
        {
            // First we get the Mesh attached to the child object
            Mesh mesh = childObject.gameObject.GetComponent<SkinnedMeshRenderer>().mesh;
 
            // If we've found a mesh we can use it to add a collider
            if (mesh != null)
            {                      
                // Add a new MeshCollider to the child object
                MeshCollider meshCollider = childObject.gameObject.AddComponent<MeshCollider>();
 
                // Finaly we set the Mesh in the MeshCollider
                meshCollider.sharedMesh = mesh;
            }
        }

    [SerializeField] GameObject fishPrefab;
    [SerializeField] Vector2 numFishMinMax;
    [SerializeField] Vector2 swimAnimationMinMax;
    // Start is called before the first frame update7

    public Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(12.5f, 17.5f)));
        return world_pos;
    }

    void Awake()
    {
        //Random fish
        Camera cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        GameObject currFish = Instantiate(fishPrefab, GetRandomPositionInCamera(cam), Random.rotation);
        currFish.transform.localScale = new Vector3(Random.value, Random.value, Random.value);
        currFish.GetComponent<Animator>().SetFloat("SpeedFish", Random.Range(swimAnimationMinMax.x, swimAnimationMinMax.y));
        currFish.transform.parent = transform;

        //Fog
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        Color rnd_col = new Color(Random.value, Random.value, Random.value, Random.value);
        RenderSettings.fogColor = rnd_col;
        RenderSettings.fogDensity = Random.Range(0.01f, 0.03f);
        RenderSettings.fog = true;



    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
