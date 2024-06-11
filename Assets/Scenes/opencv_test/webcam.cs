using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class webcam : MonoBehaviour
 {
//    int width = 1920;
//    int height = 1080;
//    int fps = 30;
    WebCamTexture webcamTexture;

    void Start () {
        WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture(devices[1].name);//, this.width, this.height, this.fps);
        GetComponent<RawImage> ().material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }
}