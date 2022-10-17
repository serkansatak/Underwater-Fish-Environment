using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mover : MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    //GameObject Fish;
    Camera main_cam;
    Vector3[] verts;
    float speed;
    float action;
    //float deltaTime;
    float time_passed;

    public class DynamicGameObject
    {
        public GameObject go;
        public int id;
        public int previous_activity; //used to prevent fish from turning twice in a row 
        public int activity; //0 for going straight, 1 for turning
        //public float lin_speed;
        //public float ang_speed;
        //public float speed;
        public Vector3 lin_speed;
        public Vector3 ang_speed;
        //public bool distractor;
    }

    List<DynamicGameObject> go_list;


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


    void updateActivity(DynamicGameObject dgo)
    {
        if(dgo.previous_activity == 0)
        {
            dgo.activity = 1;
            float ang_speed = Random.Range(-180f, 180f);
            //float lin_speed = Mathf.Abs(ang_speed/10f);
            float lin_speed = Random.Range(2f, 5f);
            dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", lin_speed);
            dgo.lin_speed = new Vector3(lin_speed, 0f, 0f);
            dgo.ang_speed = new Vector3(0, ang_speed, 0);
        }

        if(dgo.previous_activity == 1)
        {
            dgo.activity = 0;
            float lin_speed = Random.Range(2f, 5f);
            dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", lin_speed);
            dgo.lin_speed = new Vector3(lin_speed, 0f, 0f);
        }
    }

    void Turn(DynamicGameObject dgo)
    {
        dgo.go.transform.Rotate(dgo.ang_speed * Time.deltaTime);
        dgo.go.transform.position += dgo.go.transform.rotation * dgo.lin_speed * Time.fixedDeltaTime;
    }

    void goStraight(DynamicGameObject dgo)
    {
        dgo.go.transform.position += dgo.go.transform.rotation * dgo.lin_speed * Time.fixedDeltaTime;
    }


    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(7);
        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        //speed = Random.Range(0.5f, 1.8f);
        //GameObject fish_1 = spawn_fish();
        go_list = new List<DynamicGameObject>();
        
        for (int i = 0; i < 10; i++)
        {
            DynamicGameObject dgo = new DynamicGameObject();
            //speed = Random.Range(1.0f, 2.0f);
            GameObject go = spawn_fish();
            dgo.go = go;
            dgo.activity = 0;
            float lin_speed = Random.Range(2f, 5f);
            dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", lin_speed);
            dgo.lin_speed = new Vector3(lin_speed, 0f, 0f);
            //dgo.speed = speed;
            go_list.Add(dgo);   
        }
        Debug.Log("List length " + go_list.Count.ToString());
    }


    // Update is called once per frame
    void Update()
    {   
        time_passed += Time.deltaTime;
        Debug.Log("Time pass" + time_passed.ToString());

        //deltaTime = Time.fixedDeltaTime;
        //Debug.Log("Turning, Time Delta " + Time.deltaTime.ToString());
        
        foreach (DynamicGameObject dgo in go_list)
        {
            //if (Time.frameCount%30 == 0)
            if (time_passed > 1)
            { 
                if (dgo.activity == 1) updateActivity(dgo);
                if (dgo.activity == 0 && Random.value > 0.75f) updateActivity(dgo);
            }

            if (dgo.activity == 0){
                goStraight(dgo);
            } else {
                Turn(dgo);
            }
        }

        if (time_passed > 1) time_passed = 0;
        /*Quaternion rot = Fish.transform.rotation;
        Vector3 test = new Vector3(1f, 0f, 0f);
        //turn(Fish);
        //Fish.transform.Rotate(0, Time.deltaTime*Random.Range(1), 0 );
        Fish.transform.position += rot*test*Time.deltaTime*speed;*/
            
    }

}
