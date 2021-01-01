using System.Collections.Generic;
using OpenCvSharp;

namespace Yamashita.Yolo
{

    public enum DrawingMode { Off, Rectangle, Point }

    public class YoloResults
    {
        public List<string> Labels { private set; get; }
        public List<float> Confidences { private set; get; }
        public List<Point> Centers { private set; get; }
        public List<Size> Sizes { private set; get; }
        public int Count { private set; get; }
        public (string Label, float Confidence, Point Center, Size Size) this[int i]
            => (Labels[i], Confidences[i], Centers[i], Sizes[i]); 

        public YoloResults(IEnumerable<(string Label, float Confidence, Point Center, Size Size)> results)
        {
            Labels = new List<string>();
            Confidences = new List<float>();
            Centers = new List<Point>();
            Sizes = new List<Size>();
            foreach (var result in results)
            {
                Labels.Add(result.Label);
                Confidences.Add(result.Confidence);
                Centers.Add(result.Center);
                Sizes.Add(result.Size);
                Count++;
            }
        }
    }

    public interface IYolo
    {
        /// <summary>
        /// 一枚の画像に対する処理
        /// </summary>
        /// <param name="frame">入出力画像</param>
        /// <param name="results">検出結果</param>
        public void Run(ref Mat frame, out YoloResults results);

    }
}
