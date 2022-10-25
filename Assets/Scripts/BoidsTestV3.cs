//https://www.dawn-studio.de/tutorials/boids/
    //https://github.com/RealDawnStudio/unity-tutorial-boids

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
    Bounds simAreaBounds;
    //Vector3 simulationArea = new Vector3(60, 60, 60);
    Vector3 simAreaSize = new Vector3(120, 80, 80);
    int fishId = 0;
    Color fogColor;

    int numberOfSwarms = 1;
    int numberOfFish = 1;
    float animationSpeed = 1f;
    int numberOfRandomFish = 0;

    //default boids values
    float boidSpeed = 10f;
    float boidSteeringSpeed = 100f;
    float boidNoClumpingArea = 20f;
    float boidLocalArea = 10f;
    float boidSimulationArea = 30f;
    
    //default weights
    float K = 1f;
    float S = 1f;
    float M = 1f;
    float X = 1f;

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

            Vector3 cameraDirection = Vector3.zero;
            Vector3 backgroundDirection = Vector3.zero;
            Vector3 originDirection = Vector3.zero;

            Vector3 randomDirection = Vector3.zero;
            float randomWeight = 0;

            if (!b_i.randomBehaviour && Random.value > 0.75f)
            {
                numberOfRandomFish += 1;
                b_i.randomBehaviour = true;
                b_i.elapsedFrames = 0;
                b_i.goalFrames = (int) Random.Range(180f, 360f);
                b_i.framesToMaxSpeed = Mathf.RoundToInt(Random.Range(0.1f, 0.5f) * b_i.goalFrames);
                b_i.randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                b_i.randomDirection = b_i.randomDirection.normalized;
                b_i.randomWeight = Random.Range(5f, 10f);
                //b_i.randomSpeed = Random.Range(1.5f, 2f)*b_i.speed;
                b_i.randomSpeed = Random.Range(1.1f, 1.5f)*b_i.speed;
                b_i.randomSteeringSpeed = Random.Range(1.5f, 2f)*b_i.steeringSpeed;
                b_i.originalSpeed = b_i.speed;
                b_i.originalSteeringSpeed = b_i.steeringSpeed;
            }

            if (b_i.randomBehaviour)
            {
                if (b_i.elapsedFrames == b_i.goalFrames)
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
                    b_i.speed = updateSpeed(b_i, b_i.randomSpeed, b_i.originalSpeed);
                    b_i.steeringSpeed = updateSpeed(b_i, b_i.randomSteeringSpeed, b_i.originalSteeringSpeed);
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

            float distanceToCamera = Vector3.Distance(mainCam.transform.position, b_i.go.transform.position);
            float D = 10f; //distance weight, applied to both distance to the camera and distance to the background
            
            if (distanceToCamera < 20 )
            {
                //if (boidToTrack == -1) boidToTrack = i;
                cameraDirection = mainCam.transform.position - b_i.go.transform.position;
                cameraDirection = -cameraDirection;
                cameraDirection = cameraDirection.normalized;
            }

            if (b_i.go.transform.position.z > 20f)
            {
                backgroundDirection = background.transform.position - b_i.go.transform.position;
                backgroundDirection = -backgroundDirection;
                backgroundDirection = backgroundDirection.normalized;
                //backgroundDirection = new Vector3(0, 0, -1f);
            }

            if ( b_i.go.transform.position.z < 0f )
            {
                backgroundDirection = background.transform.position - b_i.go.transform.position;
                backgroundDirection = backgroundDirection.normalized;
                //backgroundDirection = new Vector3(0, 0, 1f);
            }

        
            steering += separationDirection*S;
            steering += alignmentDirection*M;
            steering += cohesionDirection*K;
            steering += leaderDirection*X;

            steering += cameraDirection*D;
            steering += backgroundDirection*D;
            steering += originDirection*D;

            steering += randomDirection*randomWeight;

            
            if (steering != Vector3.zero){
                    b_i.go.transform.rotation = Quaternion.RotateTowards(
                        b_i.go.transform.rotation, 
                        Quaternion.LookRotation(steering), 
                        b_i.steeringSpeed * time);
            }
            //b_i.go.transform.position += b_i.go.transform.TransformDirection(new Vector3(0, 0, boidSpeed)) * time;
            b_i.go.transform.position += b_i.go.transform.TransformDirection(new Vector3(b_i.speed, 0, 0))* time;

            /*Transform headTransform = b_i.go.transform.Find("Armature/Bone").transform;
            if (steering != Vector3.zero){
                    headTransform.rotation = Quaternion.RotateTowards(
                        headTransform.rotation, 
                        Quaternion.LookRotation(steering), 
                        boidSteeringSpeed * time);
            }
            headTransform.position += headTransform.TransformDirection(new Vector3(0, 0, boidSpeed)) * time;*/
            
            //checkForBoundaries(b_i);  
            checkForBoundaries(b_i); 
            //checkForBoundariesV2(b_i);  

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

            if (b_i.randomBehaviour)
            {
                Debug.Log("Boid " + i.ToString());

                /*Debug.Log("separation direction " + separationDirection.ToString());
                Debug.Log("alignemt direction " + alignmentDirection.ToString());
                Debug.Log("cohesion direction " + cohesionDirection.ToString());
                Debug.Log("leader direction " + leaderDirection.ToString());
                Debug.Log("camera direction " + cameraDirection.ToString());
                Debug.Log("background direction " + backgroundDirection.ToString());
                Debug.Log("random direction " + randomDirection.ToString());
                Debug.Log("steering " + steering.ToString());*/
                printDivider();
                //Debug.Break();
            }
            
        }
    }

    void checkForBoundaries(boidController b)
    {
        Vector3 boidPos = b.go.transform.position;
        bool inBounds = simAreaBounds.Contains(boidPos);
        
        if (!inBounds)
        {
            if (boidPos.x > simAreaBounds.max.x) boidPos.x -= simAreaBounds.extents.x* 2f;
            if (boidPos.x < simAreaBounds.min.x) boidPos.x += simAreaBounds.extents.x * 2f;

            if (boidPos.y > simAreaBounds.max.y) boidPos.y -= simAreaBounds.extents.y * 2f;
            if (boidPos.y < simAreaBounds.min.y) boidPos.y += simAreaBounds.extents.y * 2f;

            if (boidPos.z > simAreaBounds.max.z) boidPos.z -= simAreaBounds.extents.z * 2f;
            if (boidPos.z < simAreaBounds.min.z) boidPos.z += simAreaBounds.extents.z * 2f;
            
            b.go.SetActive(false);
            b.go.transform.position = boidPos;
            b.go.SetActive(true);
        }

    }

    //https://docs.unity3d.com/ScriptReference/Renderer-bounds.html
    //DEBUG
    void checkForBoundariesV2(boidController b)
    {
        Vector3 boidPos = b.go.transform.position;
        Bounds boidBounds = new Bounds();
        //float boundsOffset = 10f + boidPos.z; //to accumulate for difference between the centre and the head/tail of the fish
        float boundsOffset = 10f;
        boidBounds.center = new Vector3(0, 0, boidPos.z);
        //Vertical bounds (Y)
        float verticalFOV = mainCam.fieldOfView;
        float verticalExt = b.go.transform.position.z * Mathf.Tan(verticalFOV/2f*Mathf.Deg2Rad) + boundsOffset;
        //Horizontal bounds (X)
        float aspectRatio = 960f/544f;
        float horizontalFOV =  Camera.VerticalToHorizontalFieldOfView(verticalFOV, aspectRatio);
        float horizontalExt = b.go.transform.position.z * Mathf.Tan(horizontalFOV/2f*Mathf.Deg2Rad) + boundsOffset;
        float depthExt = b.go.transform.position.z/2f + boundsOffset ;
        boidBounds.extents = new Vector3(horizontalExt, verticalExt, depthExt);
        /*Debug.Log("boidBounds " + boidBounds.ToString());
        Debug.Log("verticalFOV " + verticalFOV.ToString());
        Debug.Log("horizontalFOV " + horizontalFOV.ToString());
        Debug.Break();*/

        bool inBounds = boidBounds.Contains(boidPos);
        
        if (!inBounds)
        {
            if (boidPos.x > boidBounds.max.x) boidPos.x -= boidBounds.extents.x* 2f;
            if (boidPos.x < boidBounds.min.x) boidPos.x += boidBounds.extents.x * 2f;

            if (boidPos.y > boidBounds.max.y) boidPos.y -= boidBounds.extents.y * 2f;
            if (boidPos.y < boidBounds.min.y) boidPos.y += boidBounds.extents.y * 2f;

            if (boidPos.z > 40f) boidPos.z = -10f;
            if (boidPos.z < -15f) boidPos.z = -10f;
            //b.go.SetActive(false);
            b.go.transform.position = boidPos;
            //b.go.SetActive(true);
        }
    }
   
    void printDivider()
    {
        Debug.Log("===============================\n===============================" + Random.value.ToString());
    }

    Vector3 GetRandomPositionInUnitSphere(Vector3 offset)
    {
        float radius = Random.Range(1f, boidLocalArea);
        Vector3 rndPos = Random.insideUnitSphere * radius + offset;
        if (rndPos.z < 0f) rndPos.z = -rndPos.z;
        return rndPos;
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
            Vector3 rnd_pos;

            if (i==0)
            {
                rnd_pos = GetRandomPositionInCamera(mainCam);
            }
            else
            {
                rnd_pos = GetRandomPositionInUnitSphere(boidsList.Last().go.transform.position);
            }
           
            rnd_pos = GetRandomPositionInCamera(mainCam);

            if(useFish) 
            {
                b.go = Instantiate(fishPrefab, rnd_pos, Quaternion.identity);
                //b.go.transform.localScale = Vector3.one * Random.Range(0.1f, 0.3f);
                b.go.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), Random.Range(-22.5f, 22.5f));
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
                b.go.transform.localPosition = rnd_pos;
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
