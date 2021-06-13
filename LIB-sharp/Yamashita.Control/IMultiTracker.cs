using System.Collections.Generic;
using OpenCvSharp;

namespace Yamashita.Control
{

    public enum OutputType { Correct, Predict }

    public interface IMultiTracker
    {

        /// <summary>
        /// Using Observe, Update Status.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="detections">List of Detection Results</param>
        /// <param name="results"></param>
        public void Update(
            ref Mat frame,
            List<(string Label, Point Center, Size Size)> detections,
            out List<(int Id, string Label, float Iou, Point Center, Size Size)> results
            );

        /// <summary>
        /// Remove designatedID object.
        /// </summary>
        /// <param name="id">Target ID</param>
        public void Remove(int id);

    }


    /// <summary>
    /// For Management by MultiTracker
    /// </summary>
    class Individual
    {

        // ------- Fields ------- //

        private readonly IFilter _filter;


        // ------- Properties ------- //

        public string Label { set; get; }

        public float Iou { set; get; }

        public Point Center { set; get; }

        public Size Size { set; get; }

        public Point NextCenter { set; get; }

        public Size NextSize { set; get; }

        public int DetectCount { set; get; }

        public int MissCount { set; get; }

        public int Id { private set; get; }

        public string Mark { private set; get; }


        // ------- Constructor ------- //

        public Individual(Point center, Size size, int id, double dt, double filterStrength = 1.0, string label = "", string mark = "")
        {
            Id = id;
            Label = label;
            Center = center;
            Size = size;
            NextCenter = Center;
            NextSize = Size;
            Iou = 1f;
            DetectCount = 1;
            Mark = mark;
            var transitionMatrix = new double[]
                        {   1, 0, dt, 0, 0, 0,
                            0, 1, 0, dt, 0, 0,
                            0, 0, 1, 0, 0, 0,
                            0, 0, 0, 1, 0, 0,
                            0, 0, 0, 0, 1, 0,
                            0, 0, 0, 0, 0, 1  };
            var measurementMatrix = new double[]
                        {   1, 0, 0, 0, 0, 0,
                            0, 1, 0, 0, 0, 0,
                            0, 0, 0, 0, 1, 0,
                            0, 0, 0, 0, 0, 1  };
            var state = new double[] { Center.X, Center.Y, 0.0, 0.0, Size.Width, Size.Height };
            _filter = new KalmanFilter(state, transitionMatrix, measurementMatrix, filterStrength);
        }


        // ------- Methods ------- //

        public void Predict(Point center, Size size)
        {
            var (correct, predict) = _filter.Update(new double[] { center.X, center.Y, size.Width, size.Height });
            Center = new Point(correct[0], correct[1]);
            Size = new Size(correct[4], correct[5]);
            NextCenter = new Point(predict[0], predict[1]);
            NextSize = new Size(predict[4], predict[5]);
        }
    }
}
