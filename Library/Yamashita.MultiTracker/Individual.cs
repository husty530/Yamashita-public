using OpenCvSharp;
using Yamashita.Kalman;

namespace Yamashita.MultiTracker
{
    class Individual
    {
        private readonly Filter _kalman;
        private double[] transitionMatrix = new double[]
                        {   1, 0, 1, 0, 0, 0,
                            0, 1, 0, 1, 0, 0,
                            0, 0, 1, 0, 0, 0,
                            0, 0, 0, 1, 0, 0,
                            0, 0, 0, 0, 1, 0,
                            0, 0, 0, 0, 0, 1  };
        private double[] measurementMatrix = new double[]
                        {   1, 0, 0, 0, 0, 0,
                            0, 1, 0, 0, 0, 0,
                            0, 0, 0, 0, 1, 0,
                            0, 0, 0, 0, 0, 1  };


        public string Label { set; get; }
        public float Iou { set; get; }
        public Rect2d Box { set; get; }
        public int DetectCount { set; get; }
        public int MissCount { set; get; }
        public int Id { private set; get; }
        public string Mark { private set; get; }


        public Individual(Rect2d box, float iou = 1f, int id = 0, string label = "", string mark = "")
        {
            Id = id;
            Label = label;
            Box = box;
            Iou = iou;
            DetectCount = 1;
            Mark = mark;
            var state = new double[] { Box.X, Box.Y, 0.0, 0.0, Box.Width, Box.Height };
            _kalman = new Filter(state, transitionMatrix, measurementMatrix, 0.5);
        }

        public void Predict(Rect2d box)
        {
            var output = _kalman.Update(new double[] { box.X, box.Y, box.Width, box.Height });
            Box = new Rect2d(output[0], output[1], output[4], output[5]);
        }
    }
}
