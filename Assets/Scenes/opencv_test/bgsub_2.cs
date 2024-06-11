namespace OpenCvSharp.Demo
{
	using UnityEngine;
	using OpenCvSharp;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using System.Linq;

	public class CircleInfo
	{
		public Point Center { get; set; }
		public int Radius { get; set; }
		public Point Endpoint { get; set; }
		public int YOffset { get; set; }


		public CircleInfo(Point center, int radius, Point endpoint, int Yoffset = 0)
		{
			Center = center;
			Radius = radius;
			Endpoint = endpoint;
			YOffset = Yoffset;
		}
	}
	public class LineInfo
	{
		public Point Start { get; set; }
		public Point End { get; set; }
		public Scalar Color { get; set; }


		public LineInfo(Point start, Point end, Scalar color)
		{
			Start = start;
			End = end;
			Color = color;
		}
	}
	public class bgsub_2 : webcam_on
	{
		private Mat bg_img;
		private bool hasBG = false;
		private bool hascc = false;
		private bool sockets = false;
		private bool near = false;
		private int frame_cnt = 10;     // 10 frame 後を背景画像としておく
		private Point point1;
		private Point point2;
		public ConnectedComponents sockets_cc;
		public Texture2D texture;
		private int [,] blobArray = new int[2, 200];
		private Mat breadboard;
		private List<int> label = new List<int>();
		private int labelcount;
		private int n = 0;
		private bool circlemove = false;
		private int label1;
		private int label2;
		private Mat bgrimg = new Mat();
		private Mat prebgrimg = new Mat();
		private bool hastuuden = false;
		private int i = -1;
		private List<CircleInfo> circles = new List<CircleInfo>();
		private	List<LineInfo> lines = new List<LineInfo>();
		private bool dengenon = true;
		private int numparts = 0;
		private bool notdoutuu = false;


		protected override void Awake()
		{
			base.Awake();
			this.forceFrontalCamera = true;
		}

		void Start()
        {
			// 背景画像の定義
			bg_img = new Mat();

			// ブレッドボードの穴の位置の読み込み

			breadboard = OpenCvSharp.Unity.TextureToMat (this.texture);

			// ラベリング
        	Mat gray = new Mat ();
        	Cv2.CvtColor(breadboard, gray, ColorConversionCodes.BGR2GRAY);
        	Mat binary = new Mat ();
        	Cv2.Threshold(gray, binary, 70, 255, ThresholdTypes.Binary);

        	sockets_cc = Cv2.ConnectedComponentsEx(binary);
	
        	foreach (var blob1 in sockets_cc.Blobs.Skip(1))
        	{
				//label.Add(blob.Label);
				blobArray[0, blob1.Label-1] = blob1.Left;
				blobArray[1, blob1.Label-1] = blob1.Top;
        	}
        }
		
		public int Nearest(int x, int y)
		{
			var diffxy = new List<int>();
			//var label = new List<int>();
			foreach (var blob2 in sockets_cc.Blobs.Skip(1))
			{
				diffxy.Add((blob2.Left - x)*(blob2.Left - x) + (blob2.Top - y)*(blob2.Top - y));
			}
		
			// 入力座標と最も近いラベル座標を返す
			// 2点間距離の2乗が格納されたリストから最小値を探す
			int min_diff = diffxy.Min();
			// 最小値がリストの何番目かを求める
			int min_label = diffxy.IndexOf(min_diff);
			//Debug.Log(min_label);
			return min_label;
		}

		public void RemoveList(int labelcount)
		{
			if(label.Count >= 2)
			{
			label.RemoveAt(label.Count - 1);
			label.RemoveAt(label.Count - 2);
			i -= 2;
			Debug.Log("redo");
			}
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

			circlemove = true;

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
				if(Input.GetMouseButtonDown(0))
				{
					++numparts;
					hascc = false;
					near = false;
					hastuuden = false;
					// HSV変換
					Mat hsvimg = new Mat();
					Cv2.CvtColor (img, hsvimg, ColorConversionCodes.BGR2HSV);
					// 差の絶対値を計算
					Mat diff = new Mat ();
					Cv2.Absdiff (grayimg, bg_img, diff);

					// 50で二値化
					Mat bndiff = new Mat ();
					Cv2.Threshold(diff, bndiff, 50, 255, ThresholdTypes.Binary);

					// HSVの黒色で二値化
					var lower = new Scalar (0, 0, 0);
					var upper = new Scalar (255, 255, 30);
					Mat rangeimg = new Mat();
					Cv2.InRange(hsvimg, lower, upper, rangeimg);

					// 2つの二値化画像の論理積を計算
					Mat andimg = new Mat();
					Cv2.BitwiseAnd(bndiff, rangeimg, andimg);

					// モルフォロジー変換
					Mat morimg = new Mat();
					var rect_kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
					Cv2.MorphologyEx(andimg, morimg, MorphTypes.Close, rect_kernel, new Point(-1,-1), 5);

					Cv2.CvtColor(bndiff, bgrimg, ColorConversionCodes.GRAY2BGRA);

					if (prebgrimg.Empty())
					{
    					prebgrimg = new Mat(bgrimg.Size(), MatType.CV_8UC4, Scalar.All(0));
					}

					Cv2.AddWeighted(bgrimg, 0.5, prebgrimg, 0.5, 0.0, bgrimg);

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
							byte a0 = 0;
							byte a1 = 10;
							Vec4b pix = prebgrimg.At<Vec4b>(y, x);
							Vec3b pixr = hsvimg.At<Vec3b>(y, x);
							bool isRed1 = (pixr[0] >= 0 && pixr[0] <= 10) || (pixr[0] >= 170 && pixr[0] <= 180);
							bool isRed2 = (100 <= pixr[1]);

							if((((pix[0] == b0) && (pix[1] == b0) && (pix[2] == b0))||(isRed1 && isRed2))||(!notdoutuu))
							{
								// 白
								pix[0] = b0;
								pix[1] = b0;
								pix[2] = b0;
								pix[3] = a0;
								prebgrimg.Set<Vec4b>(y, x, pix);
							}
							else
							{
								// 赤
								pix[0] = b0;
								pix[1] = b0;
								pix[2] = b1;
								pix[3] = a1;						
								prebgrimg.Set<Vec4b>(y, x, pix);
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

					
					if (!hascc)
					{
						// 輪郭抽出して2点探す
						OpenCvSharp.Point[][] contours;
						OpenCvSharp.HierarchyIndex[] hindex;
						Cv2.FindContours(andimg, out contours, out hindex, RetrievalModes.External, ContourApproximationModes.ApproxNone);

						IOrderedEnumerable<Point> ymax1 = contours[0].OrderByDescending(c => c.Y);
						IOrderedEnumerable<Point> ymax2 = contours[1].OrderByDescending(c => c.Y);

						point1 = new Point (ymax1.First().X, ymax1.First().Y);
						point2 = new Point (ymax2.First().X, ymax2.First().Y);
						Debug.Log(point1);
						Debug.Log(point2);
						//Debug.Log($"輪郭の個数は {contours.GetLength(0)}");
						hascc = true;
					}
					//output = Unity.MatToTexture(img, output);
				}
				if((point1.X != 0) && (point1.Y != 0))
				{
					if(point1.X < point2.X)
					{
						label1 = Nearest(point1.X, point1.Y);
						label2 = Nearest(point2.X, point2.Y);
					}
					else
					{
						label1 = Nearest(point2.X, point2.Y);
						label2 = Nearest(point1.X, point1.Y);					
					}

					if(!near)
					{
						label.Add(label1);
						label.Add(label2);
						near = true;
						//hastuuden = true;
					}

        			//Point socpoint1 = new Point(blobArray[0, label1]+2,blobArray[1, label1]+2);
        			//Point socpoint2 = new Point(blobArray[0, label1]+5,blobArray[1, label1]+5);
        			//Point socpoint3 = new Point(blobArray[0, label2],blobArray[1, label2]);
        			//Point socpoint4 = new Point(blobArray[0, label2]+5,blobArray[1, label2]+5);


					//Cv2.Rectangle(img2, socpoint1, socpoint2, Scalar.Black, 1);
					//Cv2.Rectangle(img2, socpoint3, socpoint4, Scalar.Black, 1);
					Cv2.Circle(img2, point1, 2, Scalar.Yellow, -1);
					Cv2.Circle(img2, point2, 2, Scalar.Yellow, -1);
					//Debug.Log($"{label1}, {label2}");
					int labelcount = label.Count;

					if(labelcount < 3||notdoutuu)
					{
						img2 = alphaimg + prebgrimg;
					}					

					if(dengenon)
					{
						lines.Add(new LineInfo(new Point(blobArray[0, 175],blobArray[1, 175]+5), new Point(blobArray[0, label1],blobArray[1, label1]+5), new Scalar(0, 0, 255, 255)));
						dengenon = false;
					}

					if(numparts > 3)
					{
						lines.Add(new LineInfo(new Point(blobArray[0, 150],blobArray[1, 150]-5), new Point(blobArray[0, label2],blobArray[1, label2]-5), new Scalar(255, 0, 0, 255)));
					}

					foreach (var line in lines)
					{
						Cv2.Line(img2, line.Start, line.End, line.Color, 5);
					}
				
					if(labelcount > 3)
					{
						if(!hastuuden)
						{
							i += 2;
							hastuuden = true;
						}
						else
						{
							// circlemove = true;
							if(Math.Abs(label[i] - label[i+1])% 30 == 0) 
							{								
								Point point3 = new Point(blobArray[0, label[i]],blobArray[1, label[i]]);
								Point socpoint1 = new Point(blobArray[0, label[i+1]]+2,blobArray[1, label[i+1]]+2);
								
								// 新しい円の情報を追加
        						circles.Add(new CircleInfo(new Point(point3.X, point3.Y), 8, new Point(socpoint1.X, socpoint1.Y), 0));
								// 新しい線分の情報を追加
								//lines.Add(new LineInfo(socpoint1, point3));

								img2 = alphaimg + prebgrimg;

								//Cv2.AddWeighted(img2, 1.0, circleMat, 1.0, 0.0, img2);
								//Debug.Log("tuuden");
							}
							else
							{
								notdoutuu = true;
								//Debug.Log("nottuuden");
							}

							// 全ての円を描画
							foreach (var circle in circles)
							{
								//Cv2.Circle(img2, new Point(circle.Center.X, circle.Center.Y + circle.YOffset), circle.Radius, new Scalar(0, 255, 255, 255), -1);

								if(circlemove)
								{
									//circlemove = false;
									// 円の中心座標を変化
									if(circle.Center.Y < circle.Endpoint.Y)
									{
										Cv2.Circle(img2, new Point(circle.Center.X, circle.Center.Y + circle.YOffset), circle.Radius, new Scalar(0, 255, 255, 255), -1);
										++circle.YOffset;
										if(circle.Center.Y + circle.YOffset > circle.Endpoint.Y)
										{
											circle.YOffset = 0;
										}
										//Debug.Log($"{circle.YOffset}");
									}
									else if(circle.Center.Y > circle.Endpoint.Y)
									{
										Cv2.Circle(img2, new Point(circle.Center.X, circle.Center.Y + circle.YOffset), circle.Radius, new Scalar(0, 255, 255, 255), -1);
										--circle.YOffset;
										if(circle.Center.Y + circle.YOffset < circle.Endpoint.Y)
										{
											circle.YOffset = 0;
										}
									}
									//circlemove = false;
									
								}
								
								//Cv2.Circle(img2, new Point(circle.Center.X, circle.Center.Y + circle.YOffset), circle.Radius, new Scalar(0, 255, 255, 255), -1);
							}											
						}
					}
				}

			// 画像(動画)の表示
			output = Unity.MatToTexture(img2, output);
			}
			


			// result, passing output texture as parameter allows to re-use it's buffer
			// should output texture be null a new texture will be created
			//output = Unity.MatToTexture(mask, output);
			//output = Unity.MatToTexture(bg_img, output);   // 表示したい mat を第一引数に渡す
			// この bg_img は，背景画像のテスト表示用．開始の 10 frameは画像が格納されないのでエラーが出る
			return true;
		}
	}
}