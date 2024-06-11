namespace OpenCvSharp.Demo
{
	using UnityEngine;
	using OpenCvSharp;
	using System.Collections;
	using UnityEngine.UI;
    using System.Linq;

    public class handtracking : webcam_on
	{
	    private RawImage rawImage;
        private bool hand = false;
		// override 基底クラスのメソッドを上書きする
	    protected override void Awake()
		{
			base.Awake();
			this.forceFrontalCamera = true;
		}
    
		protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
		{
			Mat img = Unity.TextureToMat(input, TextureParameters);

			// HSV色空間に変換
			Mat imgHSV = new Mat ();
			Cv2.CvtColor (img, imgHSV, ColorConversionCodes.BGR2HSV);

            // HSVの閾値を指定
            var lower = new Scalar (5, 100, 50);
            var upper = new Scalar (15, 255, 255);

            // 指定した閾値で二値化
            Mat imghand = new Mat();
            Cv2.InRange(imgHSV, lower, upper, imghand);

            if(Cv2.CountNonZero(imghand) > 200)
            {
                Cv2.Circle(img, 20, 20, 10, Scalar.Yellow, -1);
                hand = true;
            }

            if(hand && Cv2.CountNonZero(imghand) < 50)
            {
                Debug.Log("NewBG");
                hand = false;
            }

            output = Unity.MatToTexture(img, output);
			return true;
		}
    }    
}