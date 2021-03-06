﻿using System;
using System.Linq;
using System.Collections.Generic;
using OpenCvSharp;

namespace Yamashita.Control
{
    /// <summary>
    /// Tracking objects in movie frames.
    /// It is initialized & updated by detection result.
    /// </summary>
    public class MultiTracker : IMultiTracker
    {

        // ------- Fields ------- //

        private int _id;
        private readonly double _dt;
        private readonly double _filterStrength;
        private readonly OutputType _type;
        private readonly float _iouThresh;
        private readonly int _minDetectCount;
        private readonly int _maxMissCount;
        private readonly List<Individual> _trackers;
        private readonly Scalar[] _colors = Enumerable.Repeat(false, 80).Select(x => Scalar.RandomColor()).ToArray();


        // ------- Constructor ------- //

        /// <summary>
        /// Generate tracker.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="iouThresh">Threshold for regarding as same object.</param>
        /// <param name="maxMissCount">For regarding as missed object.</param>
        /// <param name="minDetectCount">For regarding as detected object.</param>
        public MultiTracker(OutputType type, float iouThresh = 0.2f, int maxMissCount = 1, int minDetectCount = 1, double dt = 0.1, double filterStrength = 1.0)
        {
            if (iouThresh < 0 || iouThresh > 1 || maxMissCount < 1 || minDetectCount < 1) throw new Exception("");
            _dt = dt;
            _filterStrength = filterStrength;
            _type = type;
            _iouThresh = iouThresh;
            _minDetectCount = minDetectCount;
            _maxMissCount = maxMissCount;
            _trackers = new List<Individual>();
        }


        // ------- Methods ------- //

        public void Update(ref Mat frame, List<(string Label, Point Center, Size Size)> detections, out List<(int Id, string Label, float Iou, Point Center, Size Size, Rect Box)> results)
        {
            Assign(detections);
            results = UpdateMemory(frame).ToList();
        }

        public void Remove(int id)
        {
            var target = _trackers.Where(t => t.Id == id).FirstOrDefault();
            _trackers.Remove(target);
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
                    _trackers.Add(new Individual(detection.Center, detection.Size, _id++, _dt, _filterStrength, detection.Label));
                }
            }
        }

        private IEnumerable<(int Id, string Label, float Confidence, Point Center, Size Size, Rect Box)> UpdateMemory(Mat frame)
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
                var box = new Rect(
                    tracker.Center.X - tracker.Size.Width / 2,
                    tracker.Center.Y - tracker.Size.Height / 2,
                    tracker.Center.X + tracker.Size.Width / 2,
                    tracker.Center.Y + tracker.Size.Height / 2);
                if (tracker.DetectCount > _minDetectCount - 1)
                {
                    if(_type == OutputType.Correct)
                    {
                        DrawRect(frame, tracker.Label, tracker.Id, tracker.Iou, tracker.Center, tracker.Size);
                        yield return (tracker.Id, tracker.Label, tracker.Iou, tracker.Center, tracker.Size, box);
                    }
                    else
                    {
                        DrawRect(frame, tracker.Label, tracker.Id, tracker.Iou, tracker.NextCenter, tracker.NextSize);
                        yield return (tracker.Id, tracker.Label, tracker.Iou, tracker.NextCenter, tracker.NextSize, box);
                    }
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
            var left = Math.Max(r1.Left, r2.Left);
            var right = Math.Min(r1.Right, r2.Right);
            var top = Math.Max(r1.Top, r2.Top);
            var bottom = Math.Min(r1.Bottom, r2.Bottom);
            var and = (right - left) * (bottom - top);
            return (double)and / (area1 + area2 - and);
        }

        private void DrawPoint(Mat image, int id, Point center)
        {
            image.Circle(center.X, center.Y, 3, _colors[id], 5);
        }

        private void DrawRect(Mat image, string labelName, int id, float iou, Point center, Size size)
        {
            var x1 = Math.Max(center.X - size.Width / 2, 0);
            var y1 = Math.Max(center.Y - size.Height / 2, 0);
            var x2 = Math.Min(center.X + size.Width / 2, image.Width);
            var y2 = Math.Min(center.Y + size.Height / 2, image.Height);
            var label = $"{labelName}{iou * 100: 0}%";
            Cv2.Rectangle(image, new Point(x1, y1), new Point(x2, y2), _colors[id % 80], 2);
            var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheyTriplex, 0.3, 0, out var baseline);
            Cv2.Rectangle(image, new Rect(new Point(x1, y1 - textSize.Height - baseline),
                new Size(textSize.Width, textSize.Height + baseline)), _colors[id % 80], Cv2.FILLED);
            var textColor = Cv2.Mean(_colors[id % 80]).Val0 < 70 ? Scalar.White : Scalar.Black;
            Cv2.PutText(image, label, new Point(x1, y1 - baseline), HersheyFonts.HersheyTriplex, 0.3, textColor);
        }

    }
}
