using System;
using System.Linq;
using System.Collections.Generic;
using OpenCvSharp;

namespace Yamashita.MultiTracker
{
    public class MultiTracker : IMultiTracker
    {
        private int _id;
        private readonly float _iouThresh;
        private readonly int _minDetectCount;
        private readonly int _maxMissCount;
        private readonly List<Individual> _trackers;
        private static readonly Scalar[] Colors = Enumerable.Repeat(false, 80).Select(x => Scalar.RandomColor()).ToArray();

        /// <summary>
        /// トラッカーを生成
        /// </summary>
        /// <param name="iouThresh">同一物体とみなす重なり度合いの閾値</param>
        /// <param name="maxMissCount">消えたとみなす連続見落とし数</param>
        /// <param name="minDetectCount">発見とみなす最小検出数</param>
        public MultiTracker(float iouThresh = 0.2f, int maxMissCount = 1, int minDetectCount = 1)
        {
            if (iouThresh < 0 || iouThresh > 1 || maxMissCount < 1 || minDetectCount < 1) throw new Exception("");
            _iouThresh = iouThresh;
            _minDetectCount = minDetectCount;
            _maxMissCount = maxMissCount;
            _trackers = new List<Individual>();
        }

        public void Update(ref Mat frame, List<(string Label, Point Center, Size Size)> detections, out List<(int Id, string Label, float Iou, Point Center, Size Size)> results)
        {
            Assign(detections);
            results = UpdateMemory(frame).ToList();
        }

        private void Assign(List<(string Label, Point Center, Size Size)> detections)
        {
            foreach (var detection in detections)
            {
                var first = true;
                foreach (var tracker in _trackers)
                {
                    var iou = (float)CalcIou(detection.Center, detection.Size, tracker.Center, tracker.Size);
                    if (iou > tracker.Iou)
                    {
                        tracker.Iou = iou;
                        tracker.Center = detection.Center;
                        tracker.Size = detection.Size;
                        tracker.MissCount = 0;
                        tracker.DetectCount++;
                        first = false;
                    }
                }
                if (first)
                {
                    _trackers.Add(new Individual(detection.Center, detection.Size, _id++, detection.Label));
                }
            }
        }

        private IEnumerable<(int Id, string Label, float Confidence, Point Center, Size Size)> UpdateMemory(Mat frame)
        {
            var removeList = new List<Individual>();
            foreach (var tracker in _trackers)
            {
                if (tracker.Iou <= _iouThresh)
                {
                    tracker.MissCount++;
                    if (tracker.MissCount > _maxMissCount ||
                        tracker.Center.X - tracker.Size.Width / 2 < 10 ||
                        tracker.Center.X + tracker.Size.Width / 2 > frame.Width - 10 ||
                        tracker.Center.Y - tracker.Size.Height / 2 < 10 ||
                        tracker.Center.Y + tracker.Size.Height / 2 > frame.Height - 10)
                    {
                        removeList.Add(tracker);
                        continue;
                    }
                }
                tracker.Predict(tracker.Center, tracker.Size);
                if (tracker.DetectCount > _minDetectCount - 1)
                {
                    DrawRect(frame, tracker.Label, tracker.Id, tracker.Iou, tracker.Center, tracker.Size);
                    yield return (tracker.Id, tracker.Label, tracker.Iou, tracker.Center, tracker.Size);
                }
                tracker.Iou = _iouThresh;
            }
            foreach (var tracker in removeList)
            {
                _trackers.Remove(tracker);
            }
        }

        private double CalcIou(Point p1, Size s1, Point p2, Size s2)
        {
            var area1 = s1.Width * s1.Height;
            var area2 = s2.Width * s2.Height;
            var r1 = new Rect(p1, s1);
            var r2 = new Rect(p2, s2);
            if (r1.Left > r2.Right || r2.Left > r1.Right || r1.Top > r2.Bottom || r2.Top > r1.Bottom) return 0.0;
            var left = (r1.Left > r2.Left) ? r1.Left : r2.Left;
            var right = (r1.Right < r2.Right) ? r1.Right : r2.Right;
            var top = (r1.Top > r2.Top) ? r1.Top : r2.Top;
            var bottom = (r1.Bottom < r2.Bottom) ? r1.Bottom : r2.Bottom;
            var and = (right - left) * (bottom - top);
            return (double)and / (area1 + area2 - and);
        }

        private void DrawPoint(Mat image, int id, Point center)
        {
            image.Circle(center.X, center.Y, 3, Colors[id], 5);
        }

        private void DrawRect(Mat image, string labelName, int id, float iou, Point center, Size size)
        {
            var x1 = (center.X - size.Width / 2) < 0 ? 0 : center.X - size.Width / 2;
            var y1 = (center.Y - size.Height / 2) < 0 ? 0 : center.Y - size.Height / 2;
            var x2 = (center.X + size.Width / 2) > image.Width ? image.Width : center.X + size.Width / 2;
            var y2 = (center.Y + size.Height / 2) > image.Height ? image.Height : center.Y + size.Height / 2;
            var label = $"{labelName}{iou * 100: 0}%";
            Console.WriteLine($"Iou {iou * 100: 0}% , {label}");
            Cv2.Rectangle(image, new Point(x1, y1), new Point(x2, y2), Colors[id % 80], 2);
            var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheyTriplex, 0.3, 0, out var baseline);
            Cv2.Rectangle(image, new Rect(new Point(x1, y1 - textSize.Height - baseline),
                new Size(textSize.Width, textSize.Height + baseline)), Colors[id % 80], Cv2.FILLED);
            var textColor = Cv2.Mean(Colors[id % 80]).Val0 < 70 ? Scalar.White : Scalar.Black;
            Cv2.PutText(image, label, new Point(x1, y1 - baseline), HersheyFonts.HersheyTriplex, 0.3, textColor);
        }
    }
}
