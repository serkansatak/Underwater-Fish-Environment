using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Flock_test : MonoBehaviour
{
    int number_of_flocks;
    //int number_of_fish;
    Camera main_cam;
    //Vector3[] verts;
    //float speed;
    //float action;
    //float deltaTime;
    float time_passed;
    [SerializeField] GameObject fishPrefab;

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
    List<DynamicGameObject> flock_list;    
    List<DynamicGameObject> go_list;


    Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(5f, 24f)));
        return world_pos;
    }

    void createFlock()
    {
        number_of_flocks = (int) Random.Range(1f, 5f);
        for (int i = 0; i < number_of_flocks; i++)
        {
            DynamicGameObject dgo = new DynamicGameObject();
            dgo.go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = verts[i];
            //sphere.transform.localScale = Vector3.one * 0.1f;
            dgo.go.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            dgo.go.transform.parent = transform;
            dgo.go.transform.position = GetRandomPositionInCamera(main_cam);
            dgo.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));

            dgo.id = i;
            dgo.activity = 0;
            float lin_speed = Random.Range(2f, 5f);
            dgo.lin_speed = new Vector3(lin_speed, 0f, 0f);
            flock_list.Add(dgo);

            int number_of_fish_in_the_flock = (int) Random.Range(2f, 5f);
            for (int j = 0; j < number_of_fish_in_the_flock; j++)
            {
                spawn_fish();
            }
        }
    }

    void spawn_fish()
    {
        //float radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
        //Vector3 rnd_pos = GetRandomPositionInCamera(main_cam);
        Vector3 rnd_pos = Random.insideUnitSphere * 5f + flock_list.Last().go.transform.position;
        GameObject currFish = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
        //currFish.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));
        //currFish.transform.rotation = Random.rotation;
        currFish.transform.parent = flock_list.Last().go.transform; // Parent the fish to the moverObj
        currFish.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
        currFish.GetComponent<Animator>().SetFloat("SpeedFish", flock_list.Last().lin_speed.x);

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
        
        //return currFish;
    }

    void goStraight(DynamicGameObject dgo)
    {
        dgo.go.transform.position += dgo.go.transform.rotation * dgo.lin_speed * Time.fixedDeltaTime;
    }


    // Start is called before the first frame update
    void Start()
    {
        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();

        flock_list = new List<DynamicGameObject>();
        createFlock();
        spawn_fish();
        
        //number_of_fish = (int) Random.Range(2, 10);
        

        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (DynamicGameObject dgo in flock_list)
        {
            goStraight(dgo);

        }
        
    }
}
