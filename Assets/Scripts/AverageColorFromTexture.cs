
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor.Media;
using UnityEditor;

public class AverageColorFromTexture : MonoBehaviour
{
    public Light lSource;

    private Color targetColor;
    private VideoPlayer videoPlayer;
    private VideoSource videoSource;
    private Renderer rend;
    private Texture tex;
    private AudioSource audioSource;

    void Start()
    {
        videoFrame = new Texture2D(2, 2);
        Application.runInBackground = true;
        StartCoroutine(playVideo());
    }

    IEnumerator playVideo()
    {
        

        videoPlayer = GameObject.Find("Video player").GetComponent<VideoPlayer>();

        //Disable Play on Awake for both Video and Audio
        videoPlayer.playOnAwake = false;
        //Set video To Play then prepare Audio to prevent Buffering
        videoPlayer.Prepare();

        //Wait until video is prepared
        while (!videoPlayer.isPrepared)
        {
            Debug.Log("Preparing Video");
            yield return null;
        }
        Debug.Log("Done Preparing Video");

        //Assign the Texture from Video to Material texture
        tex = videoPlayer.texture;
        rend.material.mainTexture = tex;

        //Enable new frame Event
        videoPlayer.sendFrameReadyEvents = true;

        //Subscribe to the new frame Event
        videoPlayer.frameReady += OnNewFrame;

        //Play Video
        videoPlayer.Play();

        //Play Sound
        Debug.Log("Playing Video");
        while (videoPlayer.isPlaying)
        {
            Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)videoPlayer.time));
            yield return null;
        }
        Debug.Log("Done Playing Video");
    }

    //Initialize in the Start function
    Texture2D videoFrame;

    void OnNewFrame(VideoPlayer source, long frameIdx)
    {
        Debug.Log("OnNewFrame " + (float)videoPlayer.time);
    }

    Color32 CalculateAverageColorFromTexture(Texture2D tex)
    {
        Color32[] texColors = tex.GetPixels32();
        int total = texColors.Length;
        float r = 0;
        float g = 0;
        float b = 0;

        for (int i = 0; i < total; i++)
        {
            r += texColors[i].r;
            g += texColors[i].g;
            b += texColors[i].b;
        }
        return new Color32((byte)(r / total), (byte)(g / total), (byte)(b / total), 0);
    }
}