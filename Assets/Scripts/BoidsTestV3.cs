//https://www.dawn-studio.de/tutorials/boids/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoidsTestV3: MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    bool useFish = true;
    Camera mainCam;
    GameObject background;
    int fishId = 0;
    Color fogColor;

    int numberOfSwarms = 3;
    int numberOfFish = 10;
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
    float boidSpeed = 10f;
    float boidSteeringSpeed = 100f;
    float boidNoClumpingArea = 20f;
    float boidLocalArea = 20f;
    float boidSimulationArea = 30f;

    float K = 1f;
    float S = 1f;
    float M = 1f;
    float X = 1f;

    public class boidController
    {
        public GameObject go;
        //identification data
        public int id;
        public int swarmIndex;
        //randomization data
        public bool randomBehaviour;
        public int elapsedFrames;
        public int goalFrames;
        public Vector3 randomDirection;
        public float randomWeight;

        public float noClumpingRadius;
        public float localAreaRadius;
        public float speed;
        public float steeringSpeed;
    }
    List<boidController> boidsList = new List<boidController>();

    int boidToTrack = -1;

    float getDistance(Vector3 v1, Vector3 v2)
    {
        Vector3 distance = Vector3.zero;
        distance.x = v1.x-v2.x;
        distance.y = v1.y-v2.y;
        distance.z = v1.z-v2.z;

        distance.x = distance.x * distance.x;
        distance.y = distance.y * distance.y;
        distance.z = distance.z * distance.z;

        float distMag = Mathf.Sqrt(distance.x + distance.y + distance.z);
        return distMag;
    }

    void simulateMovement(List<boidController> boids, float time)
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

            Vector3 cameraDirection = Vector3.zero;
            Vector3 backgroundDirection = Vector3.zero;

            Vector3 randomDirection = Vector3.zero;
            float randomWeight = 0;
            if (!b_i.randomBehaviour || Random.value > 0.25)
            {
                b_i.randomBehaviour = true;
                b_i.elapsedFrames = 0;
                b_i.goalFrames = (int) Random.Range(180f, 360f);
                b_i.randomDirection = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
                b_i.randomDirection = b_i.randomDirection.normalized;
                b_i.randomWeight = Random.Range(0.5f, 10f);
                //b_i.goalVector = new Vector3(Random.Range(1, 10f))
            }

            if (b_i.randomBehaviour)
            {
                if (b_i.elapsedFrames == b_i.goalFrames)
                {
                    b_i.randomBehaviour = false;
                }
                else
                {
                    b_i.elapsedFrames += 1;
                    randomDirection = b_i.randomDirection;
                    randomWeight = b_i.randomWeight;
                }
            } 

            if (!b_i.randomBehaviour)
            {
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

            var distanceToCamera = Vector3.Distance(mainCam.transform.position, b_i.go.transform.position);
            var myDistanceToCamera = getDistance(mainCam.transform.position, b_i.go.transform.position);
            float D = 10f; //distance weight, applied to both distance to the camera and distance to the background
            if (distanceToCamera < 20 )
            {
                if (boidToTrack == -1) boidToTrack = i;
                cameraDirection = mainCam.transform.position - b_i.go.transform.position;
                cameraDirection = -cameraDirection;
                cameraDirection = cameraDirection.normalized;
            }

            if (b_i.go.transform.position.z > 20f)
            {
                /*backgroundDirection = background.transform.position - b_i.go.transform.position;
                backgroundDirection = -backgroundDirection;
                backgroundDirection = backgroundDirection.normalized;*/
                backgroundDirection = new Vector3(0, 0, -1f);
            }

            if ( b_i.go.transform.position.z < -5f )
            {
                /*backgroundDirection = background.transform.position - b_i.go.transform.position;
                backgroundDirection = backgroundDirection.normalized;*/
                backgroundDirection = new Vector3(0, 0, 1f);
            }

        
            steering += separationDirection*S;
            steering += alignmentDirection*M;
            steering += cohesionDirection*K;
            steering += leaderDirection*X;
            steering += cameraDirection*D;
            steering += backgroundDirection*D;
            steering += randomDirection*randomWeight;

            
            if (steering != Vector3.zero){
                    b_i.go.transform.rotation = Quaternion.RotateTowards(
                        b_i.go.transform.rotation, 
                        Quaternion.LookRotation(steering), 
                        boidSteeringSpeed * time);
            }
            //b_i.go.transform.position += b_i.go.transform.TransformDirection(new Vector3(0, 0, boidSpeed)) * time;
            b_i.go.transform.position += b_i.go.transform.TransformDirection(new Vector3(boidSpeed, 0, 0))* time;

            /*Transform headTransform = b_i.go.transform.Find("Armature/Bone").transform;
            if (steering != Vector3.zero){
                    headTransform.rotation = Quaternion.RotateTowards(
                        headTransform.rotation, 
                        Quaternion.LookRotation(steering), 
                        boidSteeringSpeed * time);
            }
            headTransform.position += headTransform.TransformDirection(new Vector3(0, 0, boidSpeed)) * time;*/

            checkForBoundaries(b_i);  

            /*if (i == boidToTrack) 
            {

                Debug.Log("Boid " + i.ToString() + " distanceToCamera " + distanceToCamera.ToString());
                Debug.Log("Boid " + i.ToString() + " myDistanceToCamera " + myDistanceToCamera.ToString());
                Debug.Log("Boid local position " + b_i.go.transform.localPosition);
                Debug.Log("Boid position " + b_i.go.transform.position);
                Debug.Log("mainCam position " + mainCam.transform.position);
                
                //Debug.Log("cameraDirection " + cameraDirection.ToString());
                b_i.go.name = "THIS ONE";
                Debug.Break();
            }*/
            //Debug.Log("Boid " + i.ToString());
            //Debug.Log("distance_to_camera " + distanceToCamera.ToString());
            //printDivider();
        }
    }

    public void checkForBoundaries(boidController b)
    {
        Vector3 boidPos = b.go.transform.position;
        bool destroyGo = false;

        if (boidPos.x > boidSimulationArea)
        {
            boidPos.x -= boidSimulationArea * 2;
            destroyGo = true;
        }
        else if (boidPos.x < -boidSimulationArea)
        {
            boidPos.x += boidSimulationArea * 2;
            destroyGo = true;
        }

        if (boidPos.y > boidSimulationArea)
        {
            boidPos.y -= boidSimulationArea * 2;
            destroyGo = true;

        }  
        else if (boidPos.y < -boidSimulationArea)
        {
            boidPos.y += boidSimulationArea * 2;
            destroyGo = true;
        }

        if (boidPos.z > boidSimulationArea)
        {
            boidPos.z -= boidSimulationArea * 2;
            destroyGo = true;
        }
        else if (boidPos.z < -boidSimulationArea)
        {
            boidPos.z += boidSimulationArea * 2;
            destroyGo = true;
        }
           
        if (destroyGo)
        {
            /*Destroy(b.go);
            boidsList.Remove(b);*/
            Vector3 scale = b.go.transform.localScale;
            b.go.transform.localScale = Vector3.zero;
            b.go.transform.position = boidPos;
            b.go.transform.localScale = scale;
        }

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
        Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(10f, 34f)));
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
                //b.go.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
                //b.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-22.5f, 22.5f));
                b.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0);
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

    void generateFogColor()
    {
        //Base values 181, 202, 147, 161
        fogColor = new Color(
            Random.Range(171f, 191f)/255,  
            Random.Range(192f, 212f)/255, 
            Random.Range(137f, 157f)/255,
            Random.Range(151f, 171f)/255);
    }

    void Awake()
    {
        mainCam = GameObject.Find("Fish Camera").GetComponent<Camera>();
        generateFogColor();
        mainCam.backgroundColor = fogColor;
        background = GameObject.Find("backgroundTransparent");
        background.SetActive(false);
        //background.GetComponent<Renderer>().active = false;
        /*GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(0, 0, 50);
        go.transform.localScale = new Vector3(180, 80, 80);*/
        /*go.transform.localScale.x = 90f;
        go.transform.localScale.y = 40f;
        go.transform.localScale.z = 24f;*/
        /*GameObject background_plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        background_plane.transform.localScale = new Vector3(1000, 1000, 1);
        background_plane.transform.position = new Vector3(0, 0, 100);*/
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfSwarms; i++)
        {
            instantiateFish(i);
        }

        /*GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = mainCam.transform.position;
        go.transform.localScale = Vector3.one * 15f;*/
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount%180 == 0) Debug.Log("Speed");
        simulateMovement(boidsList, Time.deltaTime);
    }

    void LateUpdate()
    {
        
    }
}
