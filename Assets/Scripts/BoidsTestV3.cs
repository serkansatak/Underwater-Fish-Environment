//https://www.dawn-studio.de/tutorials/boids/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoidsTestV3: MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    bool useFish = false;
    Camera mainCam;
    int fishId = 0;

    int numberOfSwarms = 3;
    int numberOfFish = 5;
    //int numberOfFishMin = 5;
    //int numberOfFishMax = 10;
    float animationSpeed = 1f;

    /*public class Boid
    {
        public GameObject go;
        public int id;
        public Vector3 k; //cohesion vector
        public Vector3 s; //separation vector
        public Vector3 m; //allignment (velocity matching) vector
        public Vector3 v = Vector3.zero; //combined steer
        public List<int> neighbours = new List<int>(); //list of list indexes of local neighbours based on the visibility range
        public Vector3 c; //centre of the local flock
    }*/

    //Default boids values
    //public int spawnBoids = 100;
    public float boidSpeed = .01f;
    public float boidSteeringSpeed = .01f;
    public float boidNoClumpingArea = 5f;
    public float boidLocalArea = 5f;
    public float boidSimulationArea = 1f;

    public float K = 1f;
    public float S = 1f;
    public float M = 1f;
    public float X = 1f;

    public class boidController
    {
        public GameObject go;
        public int id;
        public int swarmIndex;
        public float noClumpingRadius;
        public float localAreaRadius;
        public float speed;
        public float steeringSpeed;
    }
    List<boidController> boidsList = new List<boidController>();

    public void simulateMovement(List<boidController> boids, float time)
    {
        for (int i = 0; i < boids.Count(); i++)
        {
            Vector3 steering = Vector3.zero;

            boidController b_i = boids[i];
            Vector3 separationDirection = Vector3.zero;
            int separationCount = 0;
            Vector3 alignmentDirection = Vector3.zero;
            int alignmentCount = 0;
            Vector3 cohesionDirection = Vector3.zero;
            int cohesionCount = 0;
            Vector3 leaderDirection = Vector3.zero;
            var leaderBoid = boids[0];
            var leaderAngle = 180f;

            for (int j = 0; j < boids.Count(); j++)
            {
                boidController b_j = boids[j];

                if (b_i == b_j) continue;

                var distance = Vector3.Distance(b_j.go.transform.position, b_i.go.transform.position);

                if (distance < boidNoClumpingArea)
                {
                    separationDirection += b_j.go.transform.position - b_i.go.transform.position;
                    separationCount++;
                }

                if (distance < boidLocalArea && b_j.swarmIndex == b_i.swarmIndex)
                {
                    alignmentDirection += b_j.go.transform.forward;
                    alignmentCount++;

                    cohesionDirection += b_j.go.transform.position - b_i.go.transform.position;
                    cohesionCount++;

                    //identify leader
                    var angle = Vector3.Angle(b_j.go.transform.position - b_i.go.transform.position, b_i.go.transform.forward);
                    if (angle < leaderAngle && angle < 90f)
                    {
                        leaderBoid = b_j;
                        leaderAngle = angle;
                    }
                }
            }

            if (separationCount > 0) separationDirection /= separationCount;
            separationDirection = -separationDirection;
            separationDirection = separationDirection.normalized;

            if (alignmentCount > 0) alignmentDirection /= alignmentCount;
            alignmentDirection = alignmentDirection.normalized;

            if (cohesionCount > 0) cohesionDirection /= cohesionCount;
            cohesionDirection -= b_i.go.transform.position;
            cohesionDirection = cohesionDirection.normalized;

            if (leaderBoid != null) 
            {
                leaderDirection = leaderBoid.go.transform.position - b_i.go.transform.position;
                leaderDirection = leaderDirection.normalized;
            }

            steering = separationDirection*S + alignmentDirection*M + cohesionDirection*K + leaderDirection*X;

            if (steering != Vector3.zero){
                    b_i.go.transform.rotation = Quaternion.RotateTowards(
                        b_i.go.transform.rotation, 
                        Quaternion.LookRotation(steering), 
                        boidSteeringSpeed * time);
            }
            b_i.go.transform.position += b_i.go.transform.TransformDirection(new Vector3(0, 0, boidSpeed)) * time;
            checkForBoundaries(b_i);    
        }
    }

    public void checkForBoundaries(boidController b)
    {
        Vector3 boidPos = b.go.transform.position;

        if (boidPos.x > boidSimulationArea)
                boidPos.x -= boidSimulationArea * 2;
        else if (boidPos.x < -boidSimulationArea)
            boidPos.x += boidSimulationArea * 2;

        if (boidPos.y > boidSimulationArea)
            boidPos.y -= boidSimulationArea * 2;
        else if (boidPos.y < -boidSimulationArea)
            boidPos.y += boidSimulationArea * 2;

        if (boidPos.z > boidSimulationArea)
            boidPos.z -= boidSimulationArea * 2;
        else if (boidPos.z < -boidSimulationArea)
            boidPos.z += boidSimulationArea * 2;

        b.go.transform.position = boidPos;
    }
   
    void printDivider()
    {
        Debug.Log("===============================\n===============================" + Random.value.ToString());
    }

    Vector3 GetRandomPositionInUnitSphere(Vector3 offset)
    {
        float radius = Random.Range(1, boidLocalArea);
        Vector3 rndPos = Random.insideUnitSphere * radius + offset;
        return rndPos;
    }


    Vector3 GetRandomPositionInCamera(Camera cam)
    {
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(5f, 34f)));
        return world_pos;
    }

    void instantiateFish(int swarmIdx)
    { 
        //int numberOfFish = (int) Random.Range(numberOfFishMin, numberOfFishMax);
        Color swarm_color = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f));

        for (int i = 0; i < numberOfFish; i++)
        {
            boidController b = new boidController();
            Vector3 rnd_pos;

            if (i==0)
            {
                rnd_pos = GetRandomPositionInCamera(mainCam);
            }
            else
            {
                rnd_pos = GetRandomPositionInUnitSphere(boidsList.Last().go.transform.position);
            }
           
            if(useFish) 
            {
                b.go = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
                b.go.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);

                b.go.GetComponent<Animator>().SetFloat("SpeedFish", animationSpeed);
                b.go.name = "fish_" + i.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
                b.go.GetComponentInChildren<fishName>().fishN = "fish_" + i.ToString();

                //Visual randomisation
                SkinnedMeshRenderer renderer = b.go.GetComponentInChildren<SkinnedMeshRenderer>();
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
                b.go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                b.go.transform.position = rnd_pos;
                b.go.transform.rotation = Random.rotation;
                b.go.transform.localScale = Vector3.one * 0.5f;
                b.go.transform.parent = transform;
                b.go.GetComponent<Renderer>().material.color = swarm_color;
                //if (swarmIdx == 0) b.go.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                //if (swarmIdx == 1) b.go.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
                //if (swarmIdx == 2) b.go.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            }
            b.id = fishId;
            fishId++;
            b.swarmIndex = swarmIdx;
            b.speed = boidSpeed;
            b.steeringSpeed = boidSteeringSpeed;
            b.localAreaRadius = boidLocalArea;
            b.noClumpingRadius = boidNoClumpingArea;
            
            boidsList.Add(b);
        }
    }

    void Awake()
    {
        mainCam = GameObject.Find("Fish Camera").GetComponent<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfSwarms; i++)
        {
            instantiateFish(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        simulateMovement(boidsList, Time.deltaTime);
    }

    void LateUpdate()
    {
        
    }
}
