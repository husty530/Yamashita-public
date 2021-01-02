using System.Collections;
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
        public void Run(ref Mat frame, out YoloResults results);

    }

    public class YoloResults : IEnumerable<(string Label, float Confidence, Point Center, Size Size)>, IEnumerator<(string Label, float Confidence, Point Center, Size Size)>
    {

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

        private int position = -1;
        public List<string> Labels { private set; get; }
        public List<float> Confidences { private set; get; }
        public List<Point> Centers { private set; get; }
        public List<Size> Sizes { private set; get; }
        public int Count { private set; get; }
        public (string Label, float Confidence, Point Center, Size Size) this[int i]
            => (Labels[i], Confidences[i], Centers[i], Sizes[i]);
        public IEnumerator<(string Label, float Confidence, Point Center, Size Size)> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        public bool MoveNext() => ++position < Count;
        public (string Label, float Confidence, Point Center, Size Size) Current
            => (Labels[position], Confidences[position], Centers[position], Sizes[position]);
        object IEnumerator.Current => Current;
        public void Dispose() { }
        public void Reset() { position = -1; }

    }
}
