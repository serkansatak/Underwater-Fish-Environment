using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

//Simple script that goes two the Assets/videos folder and selects a random video file and gives it to the Video Player
public class SetVideoBackground : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        /*string path = "Assets\\videos";
        
        string[] files = System.IO.Directory.GetFiles(path,"*.avi");
        string random_file = files[Random.Range(0, files.Length)];
        Debug.Log(files[Random.Range(0,files.Length)]);
        transform.GetComponent<VideoPlayer>().url = random_file;
        //transform.GetComponent<VideoPlayer>().url = "Assets/videos/converted/video_1_conv.ogv";*/
    }

}
