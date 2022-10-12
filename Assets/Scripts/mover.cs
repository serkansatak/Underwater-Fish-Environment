using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mover : MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    GameObject Fish;
    Camera main_cam;
    Vector3[] verts;
    float speed;


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
        currFish.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
        currFish.GetComponent<Animator>().SetFloat("SpeedFish", speed);

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

    Vector3 moving(GameObject go)
    {
        Vector3 vec = Vector3.forward;
        if (Random.value > 0.5)
        {
            Debug.Log("Moving");
            Quaternion rot = Quaternion.Euler(
                Random.Range(-90f, 90f),
                Random.Range(-90f, 90f),
                Random.Range(-90f, 90f));
            vec = rot * Vector3.forward;
        } else {
            Debug.Log("NotMoving");
        }

        return vec;
    }


    // Start is called before the first frame update
    void Start()
    {
        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        speed = Random.Range(0.5f, 1.8f);
        Fish = spawn_fish();
    }

    // Update is called once per frame
    void Update()
    {   
        Quaternion rot = Fish.transform.rotation;
        Vector3 test = new Vector3(1f, 0f, 0f);
        if (Time.frameCount == 30){
            speed = Random.Range(0.5f, 1.8f);
            Debug.Log(speed);
        }

        Fish.transform.position += rot*test*Time.deltaTime*speed;
            
    }
}
