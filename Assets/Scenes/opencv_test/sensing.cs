namespace OpenCvSharp.Demo
{
	using UnityEngine;
	using OpenCvSharp;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using System.Linq;
	using System.IO;


	public class sensing : webcam_on
	{
		private Mat bg_img;
		private bool hasBG = false;
		private int frame_cnt = 10;     // 10 frame 後を背景画像としておく

		private Mat bgrimg = new Mat();
		private Mat prebgrimg = new Mat();
		private string filePath; 
		private bool mouse = false;
		private bool thresholdResult;

		protected override void Awake()
		{
			base.Awake();
			this.forceFrontalCamera = true;
			filePath = Path.Combine(Application.dataPath, "Scenes/opencv_test/threshold_result.txt");
		}

		void Start()
        {
			// 背景画像の定義
			bg_img = new Mat();

			//string filePath = "threshold_result.txt";
		}

		// Our sketch generation function
		protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
		{
			Mat img = Unity.TextureToMat(input, TextureParameters);
			Mat img2 = new Mat();
			img2 = img.Clone();
			Mat alphaimg = new Mat();
			Cv2.CvtColor(img, alphaimg, ColorConversionCodes.BGR2BGRA);

			// グレースケール
			Mat grayimg = new Mat();
			Cv2.CvtColor(img, grayimg, ColorConversionCodes.BGR2GRAY);

			if ((!img.Empty() && !hasBG && Time.frameCount > frame_cnt)||(Input.GetMouseButtonDown(1)))
			{
				// 10 frame 経過すると bg_img に画像が格納される
				bg_img = grayimg;
				hasBG = true;
				Debug.Log("背景画像を格納した");
            }

			// それ以降は img に画像が入っているので
			// bg_img と img の加工をここに書く
			if (hasBG)
			{
				if(File.Exists(filePath)&&Input.GetMouseButtonDown(0))
				{
					Debug.Log($"pushed mouse");
					mouse = true;
					// ファイルからデータを読み込む
					string content = File.ReadAllText(filePath);
					// 文字列をboolに変換
					thresholdResult = bool.Parse(content);					
					// HSV変換
					Mat hsvimg = new Mat();
					Cv2.CvtColor (img, hsvimg, ColorConversionCodes.BGR2HSV);
					// 差の絶対値を計算
					Mat diff = new Mat ();
					Cv2.Absdiff (grayimg, bg_img, diff);

					// 50で二値化
					Mat bndiff = new Mat ();
					Cv2.Threshold(diff, bndiff, 50, 255, ThresholdTypes.Binary);

					Cv2.CvtColor(bndiff, bgrimg, ColorConversionCodes.GRAY2BGRA);

					if (prebgrimg.Empty())
					{
    					prebgrimg = new Mat(bgrimg.Size(), MatType.CV_8UC4, Scalar.All(0));
					}

					//Cv2.AddWeighted(bgrimg, 0.5, prebgrimg, 0.5, 0.0, bgrimg);

					prebgrimg.Release();
					prebgrimg = bgrimg.Clone();
					//Cv2.CvtColor(bndiff, bgrimg, ColorConversionCodes.GRAY2BGR);
					
					// 二値化の白い領域を赤色に変える
					for(int y = 0; y < prebgrimg.Rows; y++)
					{
						for(int x = 0; x < prebgrimg.Cols; x++)
						{
							byte b0 = 0;
							byte b1 = 255;
							byte b2 = 165;
							byte b3 = 182;
							byte b4 = 106;
							byte a0 = 0;
							byte a1 = 10;
							Vec4b pix = prebgrimg.At<Vec4b>(y, x);

							if((pix[0] == b0) && (pix[1] == b0) && (pix[2] == b0))
							{
								// 白
								pix[0] = b0;
								pix[1] = b0;
								pix[2] = b0;
								pix[3] = a0;
								prebgrimg.Set<Vec4b>(y, x, pix);
							}
							else if(!thresholdResult)
							{
								pix[0] = pix[0];
								pix[1] = pix[1];
								pix[2] = pix[2];
								pix[3] = pix[3];
							}
							else
							{
								if(thresholdResult)
								{
									// 赤
									pix[0] = b0;
									pix[1] = b0;
									pix[2] = b1;
									pix[3] = a1;						
									prebgrimg.Set<Vec4b>(y, x, pix);
									//Debug.Log($"深");
								}
							}
						}
					}
					for(int y = 0; y < alphaimg.Rows; y++)
					{
						for(int x = 0; x < alphaimg.Cols; x++)
						{
							byte a2 = 255;
							Vec4b pix = alphaimg.At<Vec4b>(y, x);
							pix[3] = a2;
						}
					}
				}




				if(!thresholdResult&&mouse)
				{
					//Debug.Log($"insufficient");
					Cv2.PutText(img2, "insufficient", new Point(300, 50), HersheyFonts.HersheyComplex, 1.5, new Scalar(0, 165, 255, 255));
				}
				else
				{
					Cv2.AddWeighted(bgrimg, 0.5, prebgrimg, 0.5, 0.0, bgrimg);
					img2 = alphaimg + prebgrimg;
				}
				// 画像(動画)の表示
				output = Unity.MatToTexture(img2, output);
			}
			return true;
		}
	}
}