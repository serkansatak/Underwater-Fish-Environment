using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Flock_test : MonoBehaviour
{
    int number_of_flocks;
    //int number_of_fish;
    Camera mainCam;
    //Vector3[] verts;
    //float speed;
    //float action;
    //float deltaTime;
    float time_passed;
    [SerializeField] GameObject fishPrefab;
    float deltaTime;
    float timePassed;

    float maxLinSpeed = 2f;
    float minLinSpeed = 1f;
    float maxAngSpeed = -180f;
    float minAngSpeed =  180f;
    float animationSpeed = 1f;

    int numberOfFlocksMin = 1;
    int numberOfFlocksMax = 10;
    int numberOfFlocks;
    int numberOfFishInTheFlockMin = 5;
    int numberOfFishInTheFlockMax = 10;
    int numberOfFishInTheFlock;
    int radiusMin = 3;
    int radiusMax = 9;
    int fishID = 0;

    public class DynamicGameObject
    {
        public GameObject go;
        public int id;
        public int previousActivity; //used to prevent fish from turning twice in a row 
        public int activity; //0 for going straight, 1 for turning
        //public float linSpeed;
        //public float ang_speed;
        //public float speed;
        public Vector3 linSpeed;
        public Vector3 angSpeed;
        //public bool distractor;
    }
    List<DynamicGameObject> fish_list = new List<DynamicGameObject>(); //RENAME TO FISH LIST
    List<DynamicGameObject> flock_list = new List<DynamicGameObject>();


    Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(5f, 24f)));
        return world_pos;
    }

    Vector3 GetRandomPositionInUnitSphere(Camera cam, Vector3 offset)
    {
        float radius = Random.Range(radiusMin, radiusMax);
        Vector3 rnd_skew = new Vector3(
            Random.Range(0.5f, radius),
            Random.Range(0.5f, radius), 
            Random.Range(0.5f, radius));

        Vector3 spherePos = Random.insideUnitSphere * radius + offset;
        Vector3 skewedSpherePos = new Vector3(
            spherePos.x*rnd_skew.x,
            spherePos.y*rnd_skew.y,
            spherePos.z*rnd_skew.z);
        return skewedSpherePos;
    }


     //TODO - Add pose within Unity Sphere for some images in order to simulate flocks
    void InstantiateFish()
    { 
        //float radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
        DynamicGameObject dgo = new DynamicGameObject();
        Vector3 rnd_pos = GetRandomPositionInUnitSphere(mainCam, flock_list.Last().go.transform.position);
        dgo.go = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
        //dgo.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));
        dgo.go.transform.parent = flock_list.Last().go.transform; // Parent the fish to the moverObj
        dgo.go.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
        
        /*float speed = Random.Range(swimAnimationMinMax.x, swimAnimationMinMax.y);
        currFish.GetComponent<Animator>().SetFloat("SpeedFish", speed);*/
        float linSpeed = Random.Range(minLinSpeed, maxLinSpeed);
        //dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", );
        dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", animationSpeed);
        dgo.linSpeed = new Vector3(linSpeed, 0f, 0f);

        dgo.go.name = "fish_" + fishID.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
        dgo.go.GetComponentInChildren<fishName>().fishN = "fish_" + fishID.ToString();

        //Visual randomisation
        SkinnedMeshRenderer renderer = dgo.go.GetComponentInChildren<SkinnedMeshRenderer>();
        float rnd_color_seed = Random.Range(75.0f, 225.0f);
        Color rnd_albedo = new Color(
            rnd_color_seed/255, 
            rnd_color_seed/255, 
            rnd_color_seed/255,
            Random.Range(0.0f, 1.0f));
        renderer.material.color = rnd_albedo;
        renderer.material.SetFloat("_Metalic", Random.Range(0.1f, 0.5f));
        renderer.material.SetFloat("_Metalic/_Glossiness", Random.Range(0.1f, 0.5f));
        
        //dgo.go = currFish;
        dgo.id = fishID;
        fishID += 1;
        dgo.activity = 0;
        //dgo.speed = speed;
        fish_list.Add(dgo);
    }

    void InstantiateFlocks()
    {
        numberOfFlocks = (int) Random.Range(numberOfFlocksMin, numberOfFlocksMax);
        for (int i = 0; i < numberOfFlocks; i++)
        {
            DynamicGameObject dgo = new DynamicGameObject();
            dgo.go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = verts[i];
            //sphere.transform.localScale = Vector3.one * 0.1f;
            //dgo.go.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            dgo.go.GetComponent<Renderer>().enabled = true;
            dgo.go.transform.parent = transform;
            dgo.go.transform.position = GetRandomPositionInCamera(mainCam);
            //dgo.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-45f, 45f));

            dgo.id = i;
            dgo.activity = 0;
            float linSpeed = Random.Range(minLinSpeed, maxLinSpeed);
            dgo.linSpeed = new Vector3(linSpeed, 0f, 0f);
            flock_list.Add(dgo);

            numberOfFishInTheFlock = (int)Random.Range(numberOfFishInTheFlockMin, numberOfFishInTheFlockMax);
            for (int j = 0; j < numberOfFishInTheFlock; j++)
            {
                InstantiateFish();
            }
        }
    }

    void updateActivity(DynamicGameObject dgo)
    {
        if(dgo.previousActivity == 0)
        {
            dgo.activity = 1;
            float angSpeed = Random.Range(minAngSpeed, maxAngSpeed);
            //float linSpeed = Mathf.Abs(angSpeed/10f);
            float linSpeed = Random.Range(minLinSpeed, maxLinSpeed);
            //dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", linSpeed);
            dgo.linSpeed = new Vector3(linSpeed, 0f, 0f);
            dgo.angSpeed = new Vector3(0, angSpeed, 0);
        }

        if(dgo.previousActivity == 1)
        {
            dgo.activity = 0;
            float linSpeed = Random.Range(minLinSpeed, maxLinSpeed);
            //dgo.go.GetComponent<Animator>().SetFloat("SpeedFish", linSpeed);
            dgo.linSpeed = new Vector3(linSpeed, 0f, 0f);
        }
    }

    void Turn(DynamicGameObject dgo)
    {
        dgo.go.transform.Rotate(dgo.angSpeed * deltaTime);
        dgo.go.transform.position += dgo.go.transform.rotation * dgo.linSpeed * deltaTime;
    }

    void goStraight(DynamicGameObject dgo)
    {
        dgo.go.transform.position += dgo.go.transform.rotation * dgo.linSpeed * deltaTime;
    }


    // Start is called before the first frame update
    void Start()
    {
        mainCam = GameObject.Find("Fish Camera").GetComponent<Camera>();

        //flock_list = new List<DynamicGameObject>();
        InstantiateFlocks();
        //spawn_fish();
        //number_of_fish = (int) Random.Range(2, 10);
        

        
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime = Time.deltaTime;
        timePassed += deltaTime;
        foreach (DynamicGameObject dgo in flock_list)
        {
            /*if (Time.frameCount%20 == 0)
            {
                updateActivity(dgo);
            }
            
            //updateActivity(dgo);
            if (dgo.activity == 0){
                goStraight(dgo);
            } else {
                Turn(dgo);
            }*/

            if (timePassed > 1)
            { 
                if (dgo.activity == 1) updateActivity(dgo);
                if (dgo.activity == 0 && Random.value > 0.5f) updateActivity(dgo);
            }

            if (dgo.activity == 0){
                goStraight(dgo);
            } else {
                Turn(dgo);
            }
        }

        //if (control.distractors == 1) updateDistractors();
        if (timePassed > 1) timePassed = 0;

        /*foreach (DynamicGameObject dgo in flock_list)
        {
            goStraight(dgo);

        }*/
        
    }
}
