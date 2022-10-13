using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controlDistractors : MonoBehaviour
{
    int number_of_distractors;
    List<Spawner.DynamicGameObject> distractors_list; 
    Vector3 current_direction;
    Camera main_cam;

    [SerializeField] Material mat;

    public Vector3 GetRandomStartingPosition()
    {
        float x = Random.Range(-49, 43);
        //float x = Random.Range(-59, -49);
        float y = Random.Range (-20, 22);
        //float y = Random.Range (-30, -20);
        float z = Random. Range (0, 24);
        return new Vector3(x, y, z);
    }

    public Vector3 GetRandomRespawnPosition()
    {
        float x, y, z;

        if (current_direction.x < 0){
            //x = Random.Range(45, 50);
            x = 43f;
        } else {
            //x = Random.Range(-60, -55);
            x = -49f;
        }

        /*if (current_direction.y > 0) {
            y = Random.Range(25, 30);
        } else {
            y = Random.Range(-30, -25);
        }*/
        y = Random.Range (-20, 22);
        z = Random. Range (0, 24);
        /*if (current_direction.z > 0) {
            z = Random.Range(-20, -15);
        } else {
            z = Random.Range(30, 35);
        }*/
        return new Vector3(x, y, z);
    }

   
    void Awake()
    {
        //current_direction = new Vector3 (Random.value, Random.value, Random.value);
        current_direction = new Vector3 (Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        current_direction.Normalize();
        /*if (Random.value > 0.5f){
            current_direction = current_direction * -1;
        }*/
        number_of_distractors = (int) Random.Range(500, 1000);

        main_cam = GameObject.Find("Fish Camera").GetComponent<Camera>();
    }

    void generateDistractor(bool respawn)
    {
        Spawner.DynamicGameObject dgo = new Spawner.DynamicGameObject();
        dgo.speed = Random.Range(1, 5);
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.position = GetRandomStartingPosition();
        if (respawn == false) {
            sphere.transform.position = GetRandomStartingPosition();
        } else {
            sphere.transform.position = GetRandomRespawnPosition();
        }

        sphere.transform.parent = transform;
        sphere.transform.localScale = Vector3.one * Random.Range(0.1f, 0.5f);
        dgo.go = sphere;
        //sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        Renderer rend = sphere.GetComponent<Renderer>();
        rend.material = mat;
        float rnd_color_seed = Random.Range(75.0f, 225.0f);
        /*Color rnd_albedo = new Color(
            rnd_color_seed/255, 
            rnd_color_seed/255, 
            rnd_color_seed/255,
            Random.Range(0.0f, 1.0f));*/
        Color rnd_albedo = new Color(
            Random.Range(171f, 191f)/255,  
            Random.Range(192f, 212f)/255, 
            Random.Range(137f, 157f)/255,
            Random.Range(151f, 171f)/255);
            
        rend.material.color = rnd_albedo;
        rend.material.SetFloat("_TranspModify", Random.Range(0.25f, 0.5f));
        distractors_list.Add(dgo);
    }

        // Start is called before the first frame update
    void Start()
    {
        distractors_list = new List<Spawner.DynamicGameObject>(); 
        for (int i = 0; i < number_of_distractors; i++)
        {
            generateDistractor(false);
        }
    }

    // Update is called once per frame
    void Update()
    {   
        Debug.Log(Time.frameCount.ToString() + " Distractors_List " + distractors_list.Count.ToString());
        int distractors_to_create = 0;
        //List<Spawner.DynamicGameObject> objectsToDestroy = new List<Spawner.DynamicGameObject>();
        /*foreach (var distractor in distractors_list)
        {
            Quaternion rot = distractor.go.transform.rotation;
            distractor.go.transform.position += current_direction*Time.deltaTime*distractor.speed;
            Vector3 position = main_cam.WorldToViewportPoint(distractor.go.transform.position);
            if (position.x > 1f || position.x < 0f || position.y > 1f || position.y < 0f )
            {
                objectsToDestroy.Add(distractor);
            }
        }*/
        for (int i = distractors_list.Count - 1; i >= 0; i--)
        {
            Spawner.DynamicGameObject distractor = distractors_list[i];
            //Quaternion rot = distractor.go.transform.rotation;
            distractor.go.transform.position += current_direction*Time.deltaTime*distractor.speed;
            //Vector3 position = main_cam.WorldToViewportPoint(distractor.go.transform.position);
            /*if (position.x > 1.5f || position.x < 0f || 
                position.y > 1.5f || position.y < 0f ||
                distractor.go.transform.position.z > 25f || distractor.go.transform.position.z < -10f )*/
            if (distractor.go.transform.position.x > 45f || distractor.go.transform.position.x < -55f || 
                distractor.go.transform.position.y > 25f || distractor.go.transform.position.y < -25f ||
                distractor.go.transform.position.z > 25f || distractor.go.transform.position.z < -10f )
            {
                distractors_list.RemoveAt(i);
                Destroy(distractor.go);
                distractors_to_create += 1;

            }
        }

        for (int i = 0; i < distractors_to_create; i++)
        {
            generateDistractor(true);
        }


    }
}
