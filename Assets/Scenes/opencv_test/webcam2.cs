using UnityEngine;
using UnityEngine.UI;

public class webcam2 : MonoBehaviour {

    public RawImage rawImage;

    WebCamTexture webCamTexture;

    void Start () {
        webCamTexture = new WebCamTexture();
        rawImage.texture = webCamTexture;
        webCamTexture.Play();
	}
	
	void Update (){
	}
}