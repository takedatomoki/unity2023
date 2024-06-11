namespace OpenCvSharp.Demo
{
	using UnityEngine;
	using OpenCvSharp;
	using System.Collections;
	using UnityEngine.UI;


    public class grayscale_test : webcam_on
	{
	    private RawImage rawImage;
		// override 基底クラスのメソッドを上書きする
	    protected override void Awake()
		{
			base.Awake();
			this.forceFrontalCamera = true;
		}
    
		protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
		{
			Mat img = Unity.TextureToMat(input, TextureParameters);

			//Convert image to grayscale
			Mat imgGray = new Mat ();
			Cv2.CvtColor (img, imgGray, ColorConversionCodes.BGR2GRAY);

            output = Unity.MatToTexture(imgGray, output);
			return true;
		}
    }    
}