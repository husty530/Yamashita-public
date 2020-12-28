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
        /// <param name="iouThresh">重なり度合いの閾値</param>
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

        public void Update(ref Mat frame, List<(string Label, float Confidence, Rect2d Box)> detections, out List<(int Id, string Label, float Iou, Rect2d Box)> results)
        {
            results = new List<(int Id, string Label, float Iou, Rect2d Box)>();
            Assign(detections);
            UpdateMemory(ref frame, ref results);
        }

        private void Assign(List<(string Label, float Confidence, Rect2d Box)> detections)
        {
            foreach (var detection in detections)
            {
                var first = true;
                foreach (var tracker in _trackers)
                {
                    var iou = (float)CalcIou(detection.Box, tracker.Box);
                    if (iou > tracker.Iou)
                    {
                        tracker.Iou = iou;
                        tracker.Box = detection.Box;
                        tracker.MissCount = 0;
                        tracker.DetectCount++;
                        first = false;
                    }
                }
                if (first)
                {
                    _trackers.Add(new Individual(detection.Box, 1f, _id++, detection.Label));
                }
            }
        }

        private void UpdateMemory(ref Mat frame, ref List<(int Id, string Label, float Confidence, Rect2d Box)> results)
        {
            var removeList = new List<Individual>();
            foreach (var tracker in _trackers)
            {
                if (tracker.Iou <= _iouThresh)
                {
                    tracker.MissCount++;
                    if (tracker.MissCount > _maxMissCount ||
                        tracker.Box.X - tracker.Box.Width / 2 < 10 ||
                        tracker.Box.X + tracker.Box.Width / 2 > frame.Width - 10 ||
                        tracker.Box.Y - tracker.Box.Height / 2 < 10 ||
                        tracker.Box.Y + tracker.Box.Height / 2 > frame.Height - 10)
                    {
                        removeList.Add(tracker);
                        continue;
                    }
                }
                tracker.Predict(tracker.Box);
                if (tracker.DetectCount > _minDetectCount - 1)
                {
                    DrawRect(ref frame, tracker.Label, tracker.Id, tracker.Iou, tracker.Box);
                    results.Add((tracker.Id, tracker.Label, tracker.Iou, tracker.Box));
                }
                tracker.Iou = _iouThresh;
            }
            foreach (var tracker in removeList)
            {
                _trackers.Remove(tracker);
            }
        }

        private double CalcIou(Rect2d predict, Rect2d observe)
        {
            var area1 = predict.Width * predict.Height;
            var area2 = observe.Width * observe.Height;
            var pl = predict.X - predict.Width / 2;
            var pr = predict.X + predict.Width / 2;
            var pt = predict.Y - predict.Height / 2;
            var pb = predict.Y + predict.Height / 2;
            var ol = observe.X - observe.Width / 2;
            var or = observe.X + observe.Width / 2;
            var ot = observe.Y - observe.Height / 2;
            var ob = observe.Y + observe.Height / 2;
            if (pl > or || ol > pr || pt > ob || ot > pb) return 0.0;
            var left = (pl > ol) ? pl : ol;
            var right = (pr < or) ? pr : or;
            var top = (pt > ot) ? pt : ot;
            var bottom = (pb < ob) ? pb : ob;
            var and = (right - left) * (bottom - top);
            return (double)and / (area1 + area2 - and);
        }

        private static void DrawPoint(ref Mat image, int id, double centerX, double centerY)
        {
            image.Circle((int)centerX, (int)centerY, 3, Colors[id], 5);
        }

        private static void DrawRect(ref Mat image, string labelName, int id, float iou, Rect2d rect)
        {
            var label = $"{labelName}{iou * 100: 0}%";
            Console.WriteLine($"Iou {iou * 100: 0}% , {label}");
            var x1 = (rect.X - rect.Width / 2) < 0 ? 0 : rect.X - rect.Width / 2;
            image.Rectangle(new Point(x1, rect.Y - rect.Height / 2), new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2), Colors[id % 80], 2);
            var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheyTriplex, 0.3, 0, out var baseline);
            Cv2.Rectangle(image, new Rect(new Point(x1, rect.Y - rect.Height / 2 - textSize.Height - baseline),
                new Size(textSize.Width, textSize.Height + baseline)), Colors[id % 80], Cv2.FILLED);
            var textColor = Cv2.Mean(Colors[id % 80]).Val0 < 70 ? Scalar.White : Scalar.Black;
            Cv2.PutText(image, label, new Point(x1, rect.Y - rect.Height / 2 - baseline), HersheyFonts.HersheyTriplex, 0.3, textColor);
        }
    }
}
