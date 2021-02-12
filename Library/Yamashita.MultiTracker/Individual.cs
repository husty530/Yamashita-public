using OpenCvSharp;
using KalmanFilter = Yamashita.Filter.KalmanFilter;

namespace Yamashita.MultiTracker
{
    class Individual
    {
        private readonly KalmanFilter _kalman;
        private readonly double[] transitionMatrix = new double[]
                        {   1, 0, 1, 0, 0, 0,
                            0, 1, 0, 1, 0, 0,
                            0, 0, 1, 0, 0, 0,
                            0, 0, 0, 1, 0, 0,
                            0, 0, 0, 0, 1, 0,
                            0, 0, 0, 0, 0, 1  };
        private readonly double[] measurementMatrix = new double[]
                        {   1, 0, 0, 0, 0, 0,
                            0, 1, 0, 0, 0, 0,
                            0, 0, 0, 0, 1, 0,
                            0, 0, 0, 0, 0, 1  };


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


        public Individual(Point center, Size size, int id = 0, string label = "", string mark = "")
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
            var state = new double[] { Center.X, Center.Y, 0.0, 0.0, Size.Width, Size.Height };
            _kalman = new KalmanFilter(state, transitionMatrix, measurementMatrix, 0.5);
        }

        public void Predict(Point center, Size size)
        {
            var (correct, predict) = _kalman.Update(new double[] { center.X, center.Y, size.Width, size.Height });
            Center = new Point(correct[0], correct[1]);
            Size = new Size(correct[4], correct[5]);
            NextCenter = new Point(predict[0], predict[1]);
            NextSize = new Size(predict[4], predict[5]);
        }

    }
}
