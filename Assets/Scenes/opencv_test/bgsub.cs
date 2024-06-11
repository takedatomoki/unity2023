namespace OpenCvSharp.Demo
{
	using UnityEngine;
	using OpenCvSharp;
	using System.Collections;
	using UnityEngine.UI;

    public class bgsub : webcam_on
	{
	    private RawImage rawImage;	
	    protected override void Awake()
		{
			base.Awake();
			this.forceFrontalCamera = true;
		}
    
		protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
		{
            //texture2Dをmatに変換		
            Mat img = Unity.TextureToMat(input, TextureParameters);

			//背景差分の処理
			Mat imgGray = new Mat ();
			Cv2.CvtColor (img, imgGray, ColorConversionCodes.BGR2GRAY);

            //matをtexture2Dに変換
            output = Unity.MatToTexture(imgGray, output);
			return true;
		}
    }    
}