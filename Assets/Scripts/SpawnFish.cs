using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Class for spawning fish. On awake it selects a random number of fish to spawn that the user can set mix and max bounds.
// The radius of the placement of the fish - in a sphere around the moverObj which keeps all the fish prefabs
//The swim animation min and max bounds can also be set so each fish will have their animation faster or slower.
public class SpawnFish : MonoBehaviour
{

    [SerializeField] Vector2 numFishMinMax;
    [SerializeField] GameObject fishPrefab;
    [SerializeField] Vector2 radiusMinMax;
    [SerializeField] Vector2 swimAnimationMinMax;
    // Start is called before the first frame update
    void Awake()
    {
        //For each fish Instantiate the fish prefab in a unitSphere with a certain radius give it an initial orientation (currently just rotate it 90 degrees to be parallel to the camera)
        int numberOfFish = (int)Random.Range(numFishMinMax.x, numFishMinMax.y);
        for (int i = 0; i < numberOfFish; i++)
        {
            float radius = Random.Range(radiusMinMax.x, radiusMinMax.y);
            GameObject currFish = Instantiate(fishPrefab, Random.insideUnitSphere * radius + transform.position, Quaternion.identity);

            currFish.GetComponent<Animator>().SetFloat("SpeedFish", Random.Range(swimAnimationMinMax.x, swimAnimationMinMax.y));
            currFish.transform.rotation = Quaternion.Euler(0, 90, 0);
            currFish.transform.parent = transform; // Parent the fish to the moverObj

            currFish.name = "fish_" + i.ToString();//Name the prefab clone and then access the fishName script and give the same name to it so this way the cild containing the mesh will have the proper ID
            currFish.GetComponentInChildren<fishName>().fishN = "fish_" + i.ToString();
        }
    }


}
