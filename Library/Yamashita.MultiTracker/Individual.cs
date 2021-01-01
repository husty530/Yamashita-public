using OpenCvSharp;
using Yamashita.Kalman;

namespace Yamashita.MultiTracker
{
    class Individual
    {
        private readonly Filter _kalman;
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
            Iou = 1f;
            DetectCount = 1;
            Mark = mark;
            var state = new double[] { Center.X, Center.Y, 0.0, 0.0, Size.Width, Size.Height };
            _kalman = new Filter(state, transitionMatrix, measurementMatrix, 0.5);
        }

        public void Predict(Point center, Size size)
        {
            var output = _kalman.Update(new double[] { center.X, center.Y, size.Width, size.Height });
            Center = new Point(output[0], output[1]);
            Size = new Size(output[4], output[5]);
        }

    }
}
