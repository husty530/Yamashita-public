using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;

namespace Yamashita.ML
{

    /// <summary>
    /// Whether you want this class to draw detection results.
    /// </summary>
    public enum DrawingMode { Off, Rectangle, Point }

    public interface IYolo
    {
        /// <summary>
        /// Inference one frame
        /// </summary>
        /// <param name="frame">Input & output image</param>
        /// <param name="results">Holding 'YoloResult' class</param>
        public void Run(ref Mat frame, out YoloResults results);

    }

    /// <summary>
    /// Accumulating detection results
    /// </summary>
    public class YoloResults : IEnumerable<(string Label, float Confidence, Point Center, Size Size, Rect Box)>, IEnumerator<(string Label, float Confidence, Point Center, Size Size, Rect Box)>
    {

        public YoloResults(IEnumerable<(string Label, float Confidence, Point Center, Size Size, Rect Box)> results)
        {
            Labels = new List<string>();
            Confidences = new List<float>();
            Centers = new List<Point>();
            Sizes = new List<Size>();
            Boxes = new List<Rect>();
            foreach (var result in results)
            {
                Labels.Add(result.Label);
                Confidences.Add(result.Confidence);
                Centers.Add(result.Center);
                Sizes.Add(result.Size);
                Boxes.Add(result.Box);
                Count++;
            }
        }


        private int position = -1;

        /// <summary>
        /// Class labels from (.names) file, such as 'person'.
        /// </summary>
        public List<string> Labels { private set; get; }

        /// <summary>
        /// Range of value is 0.0 ~ 1.0
        /// </summary>
        public List<float> Confidences { private set; get; }

        /// <summary>
        /// Detected center points
        /// </summary>
        public List<Point> Centers { private set; get; }

        /// <summary>
        /// Sizes of detected rectangles
        /// </summary>
        public List<Size> Sizes { private set; get; }

        /// <summary>
        /// Detected Rectangles
        /// </summary>
        public List<Rect> Boxes { private set; get; }

        /// <summary>
        /// Detected count
        /// </summary>
        public int Count { private set; get; }

        public (string Label, float Confidence, Point Center, Size Size, Rect Rectangle) this[int i]
            => (Labels[i], Confidences[i], Centers[i], Sizes[i], Boxes[i]);

        public IEnumerator<(string Label, float Confidence, Point Center, Size Size, Rect Box)> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        public bool MoveNext() => ++position < Count;

        public (string Label, float Confidence, Point Center, Size Size, Rect Box) Current
            => (Labels[position], Confidences[position], Centers[position], Sizes[position], Boxes[position]);

        object IEnumerator.Current => Current;

        public void Dispose() { position = -1; }

        public void Reset() { position = -1; }

    }
}
