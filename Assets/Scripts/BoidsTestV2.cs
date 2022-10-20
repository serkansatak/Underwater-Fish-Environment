//https://www.dawn-studio.de/tutorials/boids/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoidsTestV2: MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    bool useFish = false;

    Camera mainCam;

    float deltaTime;
    float timePassed;

    int numberOfFishMin = 5;
    int numberOfFishMax = 10;
    Vector3 flockCentre = Vector3.zero;
    //int numberOfFishInTheFlock;

    float animationSpeed = 1f;

    public class Boid
    {
        public GameObject go;
        public int id;
        public Vector3 k; //cohesion vector
        public Vector3 s; //separation vector
        public Vector3 m; //allignment (velocity matching) vector
        public Vector3 v = Vector3.zero; //combined steer
        public List<int> neighbours = new List<int>(); //list of list indexes of local neighbours based on the visibility range
        public Vector3 c; //centre of the local flock
    }
    List<Boid> boidsList = new List<Boid>();

    float visibilityRange = 5f;
    float S = .001f; 
    float K = 0f;
    float M = 0f;

    /*float maxLinSpeed = 2f;
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
    List<DynamicGameObject> flock_list = new List<DynamicGameObject>();*/

    void printDivider()
    {
        Debug.Log("===============================\n===============================" + Random.value.ToString());
    }


    Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(5f, 34f)));
        return world_pos;
    }

    Vector3 GetRandomPositionInUnitSphere(Vector3 offset)
    {
        float radius = Random.Range(1, visibilityRange/2);
        Vector3 rndPos = Random.insideUnitSphere * radius + offset;
        return rndPos;
    }


    void instantiateFish()
    { 
        int numberOfFish = (int) Random.Range(numberOfFishMin, numberOfFishMax);
        for (int i = 0; i < numberOfFish; i++)
        {
            Boid boid = new Boid();
            Vector3 rnd_pos;
            rnd_pos = GetRandomPositionInCamera(mainCam);
            flockCentre += rnd_pos;
           
            if(useFish) 
            {
                boid.go = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
                boid.go.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);

                boid.go.GetComponent<Animator>().SetFloat("SpeedFish", animationSpeed);
                boid.go.name = "fish_" + i.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
                boid.go.GetComponentInChildren<fishName>().fishN = "fish_" + i.ToString();

                //Visual randomisation
                SkinnedMeshRenderer renderer = boid.go.GetComponentInChildren<SkinnedMeshRenderer>();
                float rnd_color_seed = Random.Range(75.0f, 225.0f);
                Color rnd_albedo = new Color(
                    rnd_color_seed/255, 
                    rnd_color_seed/255, 
                    rnd_color_seed/255,
                    Random.Range(0.0f, 1.0f));
                renderer.material.color = rnd_albedo;
                renderer.material.SetFloat("_Metalic", Random.Range(0.1f, 0.5f));
                renderer.material.SetFloat("_Metalic/_Glossiness", Random.Range(0.1f, 0.5f));
            }
            else
            {
                boid.go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                boid.go.transform.position = rnd_pos;
                boid.go.transform.localScale = Vector3.one * 0.5f;
                boid.go.transform.parent = transform;
                boid.go.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            }
            
            flockCentre /= numberOfFish;
            boid.id = i;
            boidsList.Add(boid);
        }
    }

    void calculateSeparation(Boid b)
    {
        Vector3 separation = Vector3.zero;

        foreach (var idx in b.neighbours)
        {  
            separation = separation + b.go.transform.position - boidsList[idx].go.transform.position;
        }
        b.s = -separation;
    }

    void calculateCohesion(Boid b)
    {
        Vector3 cohesion = Vector3.zero;
        Vector3 centre = Vector3.zero;
        int numberOfNeighbours = b.neighbours.Count();
        
        if (numberOfNeighbours != 0)
        {
            foreach (var idx in b.neighbours)
            {  
                centre = centre + boidsList[idx].go.transform.position;
            }
            b.c = centre/numberOfNeighbours;
            b.k = b.c - b.go.transform.position;
        } 
        else
        {
            b.c = centre;
            b.k = cohesion;
        }
    }

    void calculateAllignment(Boid b)
    {
        Vector3 allignment = Vector3.zero;
        int numberOfNeighbours = b.neighbours.Count();

        if (numberOfNeighbours != 0)
        {
            foreach (var idx in b.neighbours)
            {  
                allignment = allignment + boidsList[idx].v;
            }
            b.m = allignment/numberOfNeighbours;
        } 
        else
        {
            b.m = allignment;
        }
    }

    void calculateSteering(Boid b)
    {
        b.v = b.v + S*b.s + K*b.k + M*b.m;
    }


    /*void calculateSeparationCohesionAllignmentVectors()
    {
        foreach (var boid in boidsList)
        {
            Vector3 boidPos = boid.go.transform.position;
            int numberOfNeighbours = boid.neighbours.Count();

            Vector3 separation = Vector3.zero;
            Vector3 cohesion = Vector3.zero;
            Vector3 allignment = Vector3.zero;
            Vector3 centre = Vector3.zero;

            foreach (var idx in boid.neighbours)
            {  
                Vector3 temp;
                Vector3 neighbourPos = boidsList[idx].go.transform.position; 
                Vector3 neighbourVel = boidsList[idx].v; 
                
                //separation
                temp = boidPos - neighbourPos;
                separation += temp;

                //calculate centre of a local flock, this is later used for calculating cohesion vector
                centre += neighbourPos;

                //allignment 
                allignment += neighbourVel;
            }

            boid.s = separation * -1;
            if (numberOfNeighbours == 0)
            {
                boid.c = Vector3.zero;
                boid.k = Vector3.zero;
                boid.m = Vector3.zero;
            } 
            else
            {
                boid.c = centre/numberOfNeighbours;
                boid.k = boid.c - boidPos;
                boid.m = allignment/numberOfNeighbours;
            }

            boid.v = boid.v + boid.s*S + boid.k*K + boid.m*M;
        }

    }*/

    void move(Boid b)
    {
        b.go.transform.position += b.v*Time.deltaTime;
    }


    // Start is called before the first frame update
    void Start()
    {
        mainCam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        instantiateFish();
        //getLocalFlockCenter();
        //calculateSeparationCohesionAllignmentVectors();
        foreach (var boid in boidsList)
        {
            calculateAllignment(boid);
            calculateCohesion(boid);
            calculateSeparation(boid);
            calculateSteering(boid);
        }
        /*calculateAllignment(boidsList[0]);
        calculateCohesion(boidsList[0]);
        calculateSeparation(boidsList[0]);
        calculateSteering(boidsList[0]);

        Debug.Log("Allignment " + boidsList[0].m.ToString());
        Debug.Log("Cohesion " + boidsList[0].k.ToString());
        Debug.Log("Separation " + boidsList[0].s.ToString());        
        Debug.Log("Steering " + boidsList[0].v.ToString());*/

       
    }

    // Update is called once per frame
    void Update()
    {
        //getNeighbours();
        //getLocalFlockCenter();
        //calculateSeparationCohesionAllignmentVectors();
        foreach (var boid in boidsList)
        {
            calculateAllignment(boid);
            calculateCohesion(boid);
            calculateSeparation(boid);
            calculateSteering(boid);
            move(boid);
        }
    }

    void LateUpdate()
    {
        //getNeighbours();
        //getLocalFlockCenter();
        //calculateSeparationCohesionAllignmentVectors();
    }
}
