namespace OpenCvSharp.Demo
{
	using UnityEngine;
	using OpenCvSharp;
	using System.Collections;
	using UnityEngine.UI;
    public class bgsubMOG : webcam_on
	{
	    private RawImage rawImage;	
	    protected override void Awake()
		{
			base.Awake();
			this.forceFrontalCamera = true;
		}
    
		protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
		{
			Mat img = Unity.TextureToMat(input, TextureParameters);

			//Convert image to grayscale
			Mat subimg = new Mat ();
            using (BackgroundSubtractorMOG mog = BackgroundSubtractorMOG.Create()) 
			mog.Apply(img, subimg, -1);

            output = Unity.MatToTexture(subimg, output);
			return true;
		}
    }    
}