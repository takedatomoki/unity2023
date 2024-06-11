using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System.Linq;
using System;
using TMPro;

public class labeling : MonoBehaviour
{
    public Texture2D texture;
    private Vector3 position;  
    public RectTransform canvasGameRect;
    private Vector3 ScreenToViewportPoint; 
    private Mat binary = new Mat();

    public TMP_InputField inputField_x;
    public TMP_InputField inputField_y;
    public ConnectedComponents cc;

    // Start is called before the first frame update
    void Start()
    {
        // 画像の読み込み
        Mat mat = OpenCvSharp.Unity.TextureToMat (this.texture);

        // ラベリング
        Mat gray = new Mat ();
        Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
        //Mat binary = new Mat ();
        Cv2.Threshold(gray, binary, 50, 255, ThresholdTypes.Binary);

        cc = Cv2.ConnectedComponentsEx(binary);

        foreach (var blob in cc.Blobs.Skip(1))
        {
            string Lab = blob.Label.ToString();
            Point point = new Point(blob.Left, blob.Top);
            //Cv2.PutText(mat, Lab, point, HersheyFonts.HersheyComplex, 0.3, Scalar.Red);
            Cv2.Rectangle(mat, blob.Rect, Scalar.Red, 1);
            //Debug.Log(blob.Rect);
        }
        
        // Mat -> Texture2D 
        Texture2D dstTexture = OpenCvSharp.Unity.MatToTexture(mat);
        
        // 表示
        GetComponent<RawImage>().texture = dstTexture;

        canvasGameRect = GameObject.Find("Canvas").GetComponent<RectTransform>();


    }

    public void nearestinput()
    {
        string x = inputField_x.GetComponent<TMP_InputField>().text;
        string y = inputField_y.GetComponent<TMP_InputField>().text; 

        //ConnectedComponents cc = Cv2.ConnectedComponentsEx(binary);
        //var blobxlist = new List<int>();
        //var blobylist = new List<int>();
        var diffxy = new List<int>();
        var label = new List<int>();
        int [,] blobArray = new int[2, 200];
        foreach (var blob in cc.Blobs.Skip(1))
        {
            //blobxlist.Add(blob.Left);
            //blobylist.Add(blob.Top);
            /*int[,] blobArray = new int[2, 200]{
                {blob.Left},
                {blob.Top}
            };*/
            label.Add(blob.Label);
            blobArray[0, blob.Label-1] = blob.Left;
            blobArray[1, blob.Label-1] = blob.Top;

            diffxy.Add((blob.Left - Convert.ToInt32(x))*(blob.Left - Convert.ToInt32(x)) +
                         (blob.Top - Convert.ToInt32(y))*(blob.Top - Convert.ToInt32(y)));
            //Debug.Log($"x = {blob.Left}, y = {blob.Top}");
        }

        // 入力座標と最も近いラベル座標を返す

        // 2点間距離の2乗が格納されたリストから最小値を探す
        int min_diff = diffxy.Min();
        // 最小値がリストの何番目かを求める
        int min_label = diffxy.IndexOf(min_diff);


        Mat mat = OpenCvSharp.Unity.TextureToMat (this.texture);
        Point point1 = new Point(blobArray[0, min_label],blobArray[1, min_label]);
        Point point2 = new Point(blobArray[0, min_label]+5,blobArray[1, min_label]+5);
        Cv2.Rectangle(mat, point1, point2, Scalar.Red, 1);
        //Cv2.Circle(mat, targetValue_x, targetValue_y, 2, Scalar.Yellow, -1);
        Cv2.Circle(mat, Convert.ToInt32(x), Convert.ToInt32(y), 2, Scalar.Yellow, -1);

        // ラベル番号が30の倍数のとき（同じ縦列のとき
        if(Math.Abs(6 - min_label - 1) % 30 == 0)
        {
            Debug.Log("energized");
            Point point3 = new Point(blobArray[0, 6-1],blobArray[1, 6-1]);
            Cv2.Line(mat, point3, point1, Scalar.Yellow, 5);
        }

        Texture2D dstTexture = OpenCvSharp.Unity.MatToTexture(mat);

        GetComponent<RawImage>().texture = dstTexture;          

        //Debug.Log($"{targetValue_x}, {targetValue_y}");
        //Debug.Log($"{nearestValue_x}, {nearestValue_y}");
        Debug.Log($"Label = {min_label + 1} absdiff = {Math.Abs(6 - min_label - 1)}");
        //Debug.Log($"Array = {blobArray[0,0]},{blobArray[1,0]} Label = {label[0]} List = {blobxlist[0]},{blobylist[0]}");
        //Debug.Log($"Array = {blobArray[0,1]},{blobArray[1,1]} Label = {label[1]} List = {blobxlist[1]},{blobylist[1]}");
        Debug.Log($"size = {binary.Cols},{binary.Rows}");
    }

    // Update is called once per frame
    void Update()
    {
        // マウスをクリックしたときの座標を表示する
        /*if(Input.GetMouseButtonUp(0))
        {
            position = Input.mousePosition;             // Vector3でマウス位置座標を取得する
            position.z = 0f;
            ScreenToViewportPoint = Camera.main.ScreenToViewportPoint(position);
            // 座標のずれを補正する 
            Vector2 WorldObject_ScreenPosition = new Vector2(
                ((ScreenToViewportPoint .x * canvasGameRect.sizeDelta.x) - 44 ),
                ((ScreenToViewportPoint .y * canvasGameRect.sizeDelta.y)* -1 + 605 ));

            Vector2 WorldObject_ScreenPosition_1 = new Vector2(
                ((position.x) - 44 ),
                ((position.y)* -1 + 605 ));
            Debug.Log(WorldObject_ScreenPosition.ToString());
            Debug.Log(WorldObject_ScreenPosition_1.ToString());
            //Debug.Log(Math.Round(WorldObject_ScreenPosition.x).ToString());
            //Debug.Log(Math.Round(WorldObject_ScreenPosition.y).ToString());

            ConnectedComponents cc = Cv2.ConnectedComponentsEx(binary);
            var blobxlist = new List<int>();
            var blobylist = new List<int>();
            foreach (var blob in cc.Blobs.Skip(1))
            {
                blobxlist.Add(blob.Left);
                blobylist.Add(blob.Top);
            }
            //foreach (var blob in cc.Blobs.Skip(1))
            //{
                // マウス座標に一番近いラベル座標を探す
                int targetValue_x = (int)Math.Round(WorldObject_ScreenPosition_1.x);
                //int targetValue_x = Convert.ToInt32(inputField_x);

                // ここで拡張メソッドの呼び出しをしている
                int nearestValue_x = blobxlist.Nearest(targetValue_x); 

                //Debug.Log($"{targetValue}に最も近い値は{nearestValue}です");

                int targetValue_y = (int)Math.Round(WorldObject_ScreenPosition_1.y);
                //int targetValue_y = Convert.ToInt32(inputField_y);

                int nearestValue_y = blobylist.Nearest(targetValue_y); 

                Mat mat = OpenCvSharp.Unity.TextureToMat (this.texture);
                Point point1 = new Point(nearestValue_x,nearestValue_y);
                Point point2 = new Point(nearestValue_x+5,nearestValue_y+5);
                Cv2.Rectangle(mat, point1, point2, Scalar.Red, 1);
                //Cv2.Circle(mat, targetValue_x, targetValue_y, 2, Scalar.Yellow, -1);
                Cv2.Circle(mat, targetValue_x, targetValue_y, 2, Scalar.Green, -1);
                Texture2D dstTexture = OpenCvSharp.Unity.MatToTexture(mat);

                GetComponent<RawImage>().texture = dstTexture;                
            //}
            
        }*/
    }
}

public static class LINQExtension 
{
    /// <summary>
    /// 目的の値に最も近い値を取得(int用)
    /// </summary>
    public static int Nearest(this IEnumerable<int> source, int targetValue)
    {
        if (source.Count() == 0) 
        {
            Debug.LogError($"値が入っていないので、最も近い値を取得出来ません");
            return targetValue;
        }
    
        //目的の値との差の絶対値が最小の値を計算
        var min = source.Min(value => Mathf.Abs(value - targetValue));
    
        //絶対値が最小の値だった物を最も近い値として返す
        return source.First(value => Mathf.Abs(value - targetValue) == min);
    }
  
    /// <summary>
    /// 目的の値に最も近い値を取得(float用)
    /// </summary>
    /*public static float Nearest(this IEnumerable<float> source, float targetValue)
    {
        if (source.Count() == 0) 
        {
            Debug.LogError($"値が入っていないので、最も近い値を取得出来ません");
            return targetValue;
        }
    
        //目的の値との差の絶対値が最小の値を計算
        var min = source.Min(value => Mathf.Abs(value - targetValue));
    
        //絶対値が最小の値だった物を最も近い値として返す
        return source.First(value => Mathf.Approximately(Mathf.Abs(value - targetValue), min));
    }*/
  
}
