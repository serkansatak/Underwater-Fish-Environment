//https://www.dawn-studio.de/tutorials/boids/
    //https://github.com/RealDawnStudio/unity-tutorial-boids

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class boidsClean: MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    bool useFish = true;
    Camera mainCam;
    GameObject background;
    Bounds simAreaBounds;
    //Vector3 simulationArea = new Vector3(60, 60, 60);
    Vector3 simAreaSize = new Vector3(90, 60, 60);
    int fishId = 0;
    Color fogColor;

    int numberOfSwarms = 1;
    int numberOfFish = 10;
    float animationSpeed = 1f;
    int numberOfRandomFish = 0;

    //default boids values
    float boidSpeed = 10f; //10f
    float boidSteeringSpeed = 1000f; //100
    float boidNoClumpingArea = 20f;
    float boidLocalArea = 10f;
    float boidSimulationArea = 30f;
    
    //default weights
    float K = 1f;
    float S = 1f;
    float M = 1f;
    float X = 1f;

    float minDistToCamera = 20f;

    public class boidController
    {
        public GameObject go;
        public SkinnedMeshRenderer renderer;

        //identification data
        public int id;
        public int swarmIndex;

        //random movement
        public bool randomBehaviour = false;
        
        public int elapsedFrames;
        public int goalFrames;
        public int framesToMaxSpeed;

        public float elapsedTime;
        public float goalTime;
        public float timeToMaxSpeed;

        public Vector3 randomDirection;
        public float randomWeight;
        public float randomSpeed;
        public float randomSteeringSpeed;

        //original values are used to revert back into non-random behaviour
        public float originalSpeed;
        public float originalSteeringSpeed;

        //default behaviour values
        public float noClumpingArea;
        public float localArea;
        public float speed;
        public float steeringSpeed;
    }
    List<boidController> boidsList = new List<boidController>();

    float updateSpeed(boidController b, float rndSpeed, float initSpeed)
    {
        float deltaSpeed = rndSpeed - initSpeed;
        float speedIncrement;
        float newSpeed;
    
        if (b.elapsedFrames < b.framesToMaxSpeed)
        {
            speedIncrement = (float) b.elapsedFrames/b.framesToMaxSpeed*deltaSpeed;
        } 
        else if (b.elapsedFrames == b.framesToMaxSpeed) 
        {
            speedIncrement = deltaSpeed;
        }   
        else 
        {
            //Debug.Break();
            speedIncrement = (float) b.elapsedFrames/b.goalFrames*deltaSpeed*-1f;
        }

        /*Debug.Log("rndSpeed " + rndSpeed.ToString());
        Debug.Log("initSpeed " + initSpeed.ToString());
        Debug.Log("deltaSpeed " + deltaSpeed.ToString());
        Debug.Log("elapsedFramse " + b.elapsedFrames.ToString());
        Debug.Log("framesToMaxSpeed " + b.framesToMaxSpeed.ToString());
        Debug.Log("goalFrames " + b.goalFrames.ToString());
        Debug.Log("speedIncrement " + speedIncrement.ToString());
        printDivider();*/
        newSpeed = initSpeed + speedIncrement;
        
        return newSpeed;
    }

    float updateSpeedTime(boidController b, float rndSpeed, float initSpeed)
    {
        float deltaSpeed = rndSpeed - initSpeed;
        float speedIncrement;
        float newSpeed;
    
        if (b.elapsedTime < b.timeToMaxSpeed)
        {
            speedIncrement = b.elapsedTime/b.timeToMaxSpeed*deltaSpeed;
        } 
        else if (b.elapsedTime == b.timeToMaxSpeed) 
        {
            speedIncrement = deltaSpeed;
        }   
        else 
        {
            //Debug.Break();
            speedIncrement = b.elapsedTime/b.goalTime*deltaSpeed*-1f;
        }

        Debug.Log("rndSpeed " + rndSpeed.ToString());
        Debug.Log("initSpeed " + initSpeed.ToString());
        Debug.Log("deltaSpeed " + deltaSpeed.ToString());
        Debug.Log("elapsedTime " + b.elapsedTime.ToString());
        Debug.Log("timeToMaxSpeed " + b.timeToMaxSpeed.ToString());
        Debug.Log("goalTime " + b.goalTime.ToString());
        Debug.Log("speedIncrement " + speedIncrement.ToString());
        printDivider();
        newSpeed = initSpeed + speedIncrement;
        
        return newSpeed;
    }


    void simulateMovement(List<boidController> boids, float time)
    {
        for (int i = 0; i < boids.Count(); i++)
        {
            boidController b_i = boids[i];

            Vector3 steering = Vector3.zero;
            Vector3 separationDirection = Vector3.zero;
            int separationCount = 0;
            Vector3 alignmentDirection = Vector3.zero;
            int alignmentCount = 0;
            Vector3 cohesionDirection = Vector3.zero;
            int cohesionCount = 0;
            Vector3 leaderDirection = Vector3.zero;
            boidController leaderBoid = boids[0];
            float leaderAngle = 180f;

            Vector3 randomDirection = Vector3.zero;
            float randomWeight = 0;

            if (!b_i.randomBehaviour && Random.value > 1.1f)
            {
                numberOfRandomFish += 1;
                b_i.randomBehaviour = true;

                b_i.elapsedFrames = 0;
                b_i.goalFrames = (int) Random.Range(180f, 360f);
                b_i.framesToMaxSpeed = Mathf.RoundToInt(Random.Range(0.1f, 0.5f) * b_i.goalFrames);

                b_i.elapsedTime = 0f;
                b_i.goalTime = 1f;
                b_i.timeToMaxSpeed = Random.Range(0.1f, 0.5f);
                
                //b_i.randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                //b_i.randomDirection = b_i.randomDirection.normalized;
                b_i.randomDirection = Random.insideUnitSphere * Random.Range(1f, 5f) + b_i.go.transform.position;
                b_i.randomDirection = b_i.randomDirection.normalized;

                b_i.randomWeight = Random.Range(2f, 5f);
                b_i.randomSpeed = Random.Range(2f, 3f)*b_i.speed;
                b_i.randomSteeringSpeed = Random.Range(2f, 3f)*b_i.steeringSpeed;
                b_i.originalSpeed = b_i.speed;
                b_i.originalSteeringSpeed = b_i.steeringSpeed;
            }

            if (b_i.randomBehaviour)
            {
                //if (b_i.elapsedFrames == b_i.goalFrames)
                if (b_i.elapsedTime > b_i.goalTime)
                {
                    numberOfRandomFish -= 1;
                    b_i.randomBehaviour = false;
                    b_i.speed = b_i.originalSpeed;
                    b_i.steeringSpeed = b_i.originalSteeringSpeed;
                }
                else
                {
                    randomDirection = b_i.randomDirection;
                    randomWeight = b_i.randomWeight;
                    b_i.speed = updateSpeedTime(b_i, b_i.randomSpeed, b_i.originalSpeed);
                    b_i.steeringSpeed = updateSpeedTime(b_i, b_i.randomSteeringSpeed, b_i.originalSteeringSpeed);

                    b_i.elapsedTime += time;
                    b_i.elapsedFrames += 1;
                }
            } 

            if (!b_i.randomBehaviour)
            {
                for (int j = 0; j < boids.Count(); j++)
                {
                    boidController b_j = boids[j];

                    if (b_i == b_j) continue;

                    float distance = Vector3.Distance(b_j.go.transform.position, b_i.go.transform.position);

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
                        float angle = Vector3.Angle(b_j.go.transform.position - b_i.go.transform.position, b_i.go.transform.forward);
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
            }

            Vector3 cameraDirection = Vector3.zero;
            float distanceToCamera = Vector3.Distance(mainCam.transform.position, b_i.go.transform.position);
            float cameraDirWeight = minDistToCamera/distanceToCamera;
            /*if (cameraDirWeight > 1f) 
            {
                cameraDirection = mainCam.transform.position - b_i.go.transform.position;
                cameraDirection = cameraDirection;
                cameraDirection = -cameraDirection;
                cameraDirection = cameraDirection.normalized*cameraDirWeight;
                print("cameraDirection " + cameraDirection.ToString());
            }*/
            cameraDirection = mainCam.transform.position - b_i.go.transform.position;
            cameraDirection = cameraDirection;
            cameraDirection = -cameraDirection;
            cameraDirection = cameraDirection.normalized*cameraDirWeight;
            print("cameraDirection " + cameraDirection.ToString());
            steering += cameraDirection;
        
            Vector3 boundsDirection = Vector3.zero;
            if (!simAreaBounds.Contains(b_i.go.transform.position))
            {
                if (b_i.go.transform.position.x > simAreaBounds.max.x) boundsDirection.x = -simAreaBounds.extents.x * 2f;
                if (b_i.go.transform.position.x < simAreaBounds.min.x) boundsDirection.x = simAreaBounds.extents.x * 2f;

                if (b_i.go.transform.position.y > simAreaBounds.max.y) boundsDirection.y = -simAreaBounds.extents.y * 2f;
                if (b_i.go.transform.position.y < simAreaBounds.min.y) boundsDirection.y = simAreaBounds.extents.y * 2f;

                if (b_i.go.transform.position.z > simAreaBounds.max.z) boundsDirection.z = -simAreaBounds.extents.z * 2f;
                if (b_i.go.transform.position.z < simAreaBounds.min.z) boundsDirection.z = simAreaBounds.extents.z * 2f;
                
                boundsDirection = boundsDirection.normalized;

                float weightXDir = Mathf.Abs((b_i.go.transform.position.x - simAreaBounds.center.x)/simAreaBounds.extents.x);
                float weightYDir = Mathf.Abs((b_i.go.transform.position.y - simAreaBounds.center.y)/simAreaBounds.extents.y);
                float weightZDir = Mathf.Abs((b_i.go.transform.position.z - simAreaBounds.center.z)/simAreaBounds.extents.z);
                boundsDirection.x = boundsDirection.x*weightXDir;
                boundsDirection.y = boundsDirection.y*weightYDir;
                boundsDirection.z = boundsDirection.z*weightZDir;
               
                b_i.steeringSpeed = 10000f;
                print("boundsDirection " + boundsDirection.ToString());
            }
            else
            {
                b_i.steeringSpeed = boidSteeringSpeed;
            }
            steering += boundsDirection;

            //Apply boid behaviour if the fish is within the simulation area bounds and far enough from the camera
            if (boundsDirection == Vector3.zero)
            {
                steering += separationDirection*S;
                steering += alignmentDirection*M;
                steering += cohesionDirection*K;
                steering += leaderDirection*X;
            }

            if (randomDirection != Vector3.zero) steering += randomDirection*randomWeight;

            if (steering != Vector3.zero)
            {
                    b_i.go.transform.rotation = Quaternion.RotateTowards(
                        b_i.go.transform.rotation, 
                        Quaternion.LookRotation(steering), 
                        b_i.steeringSpeed * time);
            }
           
            b_i.go.transform.position += b_i.go.transform.TransformDirection(new Vector3(b_i.speed, 0, 0))* time;
            
            print("steering " + steering.ToString());
            //printDivider();
        }
    }
   
    void printDivider()
    {
        print("===============================\n===============================" + Random.value.ToString());
    }

    Vector3 GetRandomPositionInCamera(Camera cam)
    {
        //Vector3 world_pos = cam.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(0.1f, 0.9f), UnityEngine.Random.Range(10f, 34f)));
        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(
            UnityEngine.Random.Range(0.1f, 0.9f), 
            UnityEngine.Random.Range(0.1f, 0.9f), 
            simAreaSize.z/2f + 10f));
        return worldPos;
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
            if(useFish) 
            {
                b.go = Instantiate(fishPrefab);
                b.go.transform.position = GetRandomPositionInCamera(mainCam);
                //b.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-22.5f, 22.5f));
                b.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0);
                //b.go.transform.localScale = Vector3.one * Random.Range(40f, 60f);
                b.go.GetComponent<Animator>().SetFloat("SpeedFish", animationSpeed);
                b.go.name = "fish_" + fishId.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
                b.go.GetComponentInChildren<fishName>().fishN = "fish_" + fishId.ToString();

                //Visual randomisation
                SkinnedMeshRenderer renderer = b.go.GetComponentInChildren<SkinnedMeshRenderer>();
                b.renderer = renderer;
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
                b.go.name = "fish_" + fishId.ToString();
                b.go.transform.localPosition = GetRandomPositionInCamera(mainCam);
                //b.go.transform.rotation = Random.rotation;
                //b.go.transform.localScale = Vector3.one * 0.5f;
                b.go.GetComponent<Renderer>().material.color = swarm_color;
            }

            b.id = fishId;
            fishId++;
            b.swarmIndex = swarmIdx;
            b.speed = boidSpeed;
            b.steeringSpeed = boidSteeringSpeed;
            b.localArea = boidLocalArea;
            b.noClumpingArea = boidNoClumpingArea;
            
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
     
        GameObject simArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        simArea.transform.position = new Vector3(0, 0, simAreaSize.z/2f);
        simArea.transform.localScale = simAreaSize;
        UnityEngine.Physics.SyncTransforms();
        simAreaBounds = simArea.GetComponent<Collider>().bounds;
        simArea.SetActive(false);
        //float distanceToSimArea = Vector3.Distance(go.transform.position, mainCam.transform.position);
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
