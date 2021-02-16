using System.Collections.Generic;
using OpenCvSharp;

namespace Yamashita.Control
{

    public enum OutputType { Correct, Predict }

    public interface IMultiTracker
    {

        /// <summary>
        /// 観測を使って追跡状態を更新する
        /// </summary>
        /// <param name="frame">入出力画像</param>
        /// <param name="detections">検出結果のリスト</param>
        /// <param name="results">更新結果</param>
        public void Update(
            ref Mat frame,
            List<(string Label, Point Center, Size Size)> detections,
            out List<(int Id, string Label, float Iou, Point Center, Size Size)> results
            );

        /// <summary>
        /// 指定したIDの物体を削除する
        /// </summary>
        /// <param name="id">削除対象のID</param>
        public void Remove(int id);

    }

    class Individual
    {
        private readonly IFilter _filter;
        private double[] transitionMatrix = new double[]
                        {   1, 0, 0.1, 0, 0, 0,
                            0, 1, 0, 0.1, 0, 0,
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
            _filter = new KalmanFilter(state, transitionMatrix, measurementMatrix, 0.5);
            //_filter = new ParticleFilter(state, transitionMatrix, measurementMatrix);
        }

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
