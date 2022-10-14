using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controlDistractors : MonoBehaviour
{
    public int number_of_distractors;
    public List<Spawner.DynamicGameObject> distractors_list; 
    public Vector3 current_direction;
    //Camera main_cam;
    [SerializeField] Material mat;

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-49, 43);
        //float x = Random.Range(-59, -49);
        float y = Random.Range (-20, 22);
        //float y = Random.Range (-30, -20);
        float z = Random. Range (0, 24);
        return new Vector3(x, y, z);
    }

    void getNewCurrentDirection()
    {
        current_direction = new Vector3 (Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        current_direction.Normalize();
    }

    /*public void randomizeNumberOfDistractors()
    {
        number_of_distractors = (int) Random.Range(500, 1000);
    }*/
     
    public void generateDistractors()
    {
        getNewCurrentDirection();
        number_of_distractors = (int) Random.Range(500, 1000);
        distractors_list = new List<Spawner.DynamicGameObject>(); 

        for (int i = 0; i < number_of_distractors; i++)
        {
            Spawner.DynamicGameObject dgo = new Spawner.DynamicGameObject();
            dgo.speed = Random.Range(1, 10);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = GetRandomPosition();

            sphere.transform.parent = transform;
            sphere.transform.localScale = Vector3.one * Random.Range(0.01f, 1f);
            dgo.go = sphere;
            Renderer rend = sphere.GetComponent<Renderer>();
            rend.material = mat;
            //float rnd_color_seed = Random.Range(75.0f, 225.0f);
            Color rnd_albedo = new Color(
                Random.Range(171f, 191f)/255,  
                Random.Range(192f, 212f)/255, 
                Random.Range(137f, 157f)/255,
                Random.Range(151f, 171f)/255);  
            rend.material.color = rnd_albedo;
            rend.material.SetFloat("_TranspModify", Random.Range(0.25f, 0.5f));
            distractors_list.Add(dgo);
        }
    }

    public void updateDistractors()
    {
        //int distractors_to_create = 0;

        for (int i = distractors_list.Count - 1; i >= 0; i--)
        {
            Spawner.DynamicGameObject distractor = distractors_list[i];
            distractor.go.transform.position += current_direction*Time.deltaTime*distractor.speed;
            if (distractor.go.transform.position.x > 45f || distractor.go.transform.position.x < -55f || 
                distractor.go.transform.position.y > 25f || distractor.go.transform.position.y < -25f ||
                distractor.go.transform.position.z > 25f || distractor.go.transform.position.z < -10f )
            {
                /*distractors_list.RemoveAt(i);
                Destroy(distractor.go);
                distractors_to_create += 1;*/
                distractor.go.transform.position = GetRandomPosition();

            }
        }

        /*for (int i = 0; i < distractors_to_create; i++)
        {
            generateDistractor(true);
        }*/
    }

    public void CleanUp()
    {
        foreach (Spawner.DynamicGameObject dgo in distractors_list)
        {
            Destroy(dgo.go);
        }
        distractors_list.Clear();
    }
       
    void Awake()
    {
        //randomizeNumberOfDistractors();
        //main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
    }

    void Start()
    {
        //generateDistractors();
    }

    void Update()
    {   
        /*if (Time.frameCount == 200)
        {
            cleanUp();
        } else {
            updateDistractors();
        }*/
    }
}
