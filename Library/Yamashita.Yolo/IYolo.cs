using System.Collections.Generic;
using OpenCvSharp;

namespace Yamashita.Yolo
{

    public enum DrawingMode { Off, Rectangle, Point }

    public interface IYolo
    {
        /// <summary>
        /// 一枚の画像に対する処理
        /// </summary>
        /// <param name="frame">入出力画像</param>
        /// <param name="results">検出結果</param>
        public void Run(ref Mat frame, out List<(string Label, float Confidence, Point Center, Size Size)> results);

    }
}
