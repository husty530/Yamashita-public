using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace Yamashita.Yolo
{
    public class Yolo : IYolo
    {
        
        private DrawingMode _draw;
        private Net _net;
        private float _threshold;
        private float _nmsThreshold;
        private static string[] _labels;

        private static readonly Scalar[] Colors = Enumerable.Repeat(false, 80).Select(x => Scalar.RandomColor()).ToArray();
        private static Size BlobSize { set; get; }

        /// <summary>
        /// 検出器の初期化
        /// </summary>
        /// <param name="cfg">.cfgファイル</param>
        /// <param name="names">.namesファイル</param>
        /// <param name="weights">.weightsファイル</param>
        /// <param name="blobSize">入力サイズ</param>
        /// <param name="draw">描画の設定</param>
        /// <param name="confThresh">確信度の閾値</param>
        /// <param name="nmsThresh">重なりの閾値</param>
        public Yolo(string cfg, string names, string weights, Size blobSize, DrawingMode draw = DrawingMode.Rectangle, float confThresh = 0.5f, float nmsThresh = 0.3f)
        {
            BlobSize = blobSize;
            _draw = draw;
            _threshold = confThresh;
            _nmsThreshold = nmsThresh;
            _labels = File.ReadAllLines(names).ToArray();
            _net = CvDnn.ReadNetFromDarknet(cfg, weights);
            _net.SetPreferableBackend(Net.Backend.OPENCV);
            _net.SetPreferableTarget(Net.Target.CPU);
        }

        public void Run(ref Mat frame, out List<(string Label, float Confidence, Point Centers, Rect2d Box)> results)
        {
            var blob = CvDnn.BlobFromImage(frame, 1.0 / 255, BlobSize, new Scalar(), true, false);
            _net.SetInput(blob);
            var outNames = _net.GetUnconnectedOutLayersNames();
            var outs = outNames.Select(_ => new Mat()).ToArray();
            _net.Forward(outs, outNames);
            results = GetResult(outs, frame, _threshold, _nmsThreshold).ToList();
        }

        private IEnumerable<(string Label, float Confidence, Point Centers, Rect2d Box)> GetResult(IEnumerable<Mat> output, Mat image, float threshold, float nmsThreshold)
        {
            var classIds = new List<int>();
            var confidences = new List<float>();
            var probabilities = new List<float>();
            var boxes = new List<Rect2d>();
            var w = image.Width;
            var h = image.Height;
            foreach (var pred in output)
            {
                for (var i = 0; i < pred.Rows; i++)
                {
                    var confidence = pred.At<float>(i, 4);
                    if (confidence > threshold)
                    {
                        Cv2.MinMaxLoc(pred.Row(i).ColRange(5, pred.Cols), out _, out Point classIdPoint);
                        var probability = pred.At<float>(i, classIdPoint.X + 5);
                        if (probability > threshold)
                        {
                            var centerX = pred.At<float>(i, 0) * w;
                            var centerY = pred.At<float>(i, 1) * h;
                            var width = pred.At<float>(i, 2) * w;
                            var height = pred.At<float>(i, 3) * h;
                            classIds.Add(classIdPoint.X);
                            confidences.Add(confidence);
                            probabilities.Add(probability);
                            boxes.Add(new Rect2d(centerX, centerY, width, height));
                        }
                    }
                }
            }

            CvDnn.NMSBoxes(boxes, confidences, threshold, nmsThreshold, out int[] indices);
            foreach (var i in indices)
            {
                switch (_draw)
                {
                    case (DrawingMode.Off):
                        break;
                    case (DrawingMode.Point):
                        DrawPoint(image, classIds[i], boxes[i].X, boxes[i].Y);
                        break;
                    case (DrawingMode.Rectangle):
                        DrawRect(image, classIds[i], confidences[i], boxes[i]);
                        break;
                }
                yield return (_labels[classIds[i]], confidences[i], new Point((int)boxes[i].X, (int)boxes[i].Y), boxes[i]);
            }
        }

        private static void DrawPoint(Mat image, int classes, double centerX, double centerY)
        {
            image.Circle((int)centerX, (int)centerY, 3, Colors[classes], 5);
        }

        private static void DrawRect(Mat image, int classes, float confidence, Rect2d rect)
        {
            var label = $"{_labels[classes]}{confidence * 100 : 0}%";
            Console.WriteLine($"confidence {confidence * 100 : 0}% , {label}");
            var x1 = (rect.X - rect.Width / 2) < 0 ? 0 : rect.X - rect.Width / 2;
            image.Rectangle(new Point(x1, rect.Y - rect.Height / 2), new Point((int)(rect.X + rect.Width / 2), rect.Y + rect.Height / 2), Colors[classes], 2);
            var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheyTriplex, 0.3, 0, out var baseline);
            Cv2.Rectangle(image, new Rect(new Point((int)x1, rect.Y - rect.Height / 2 - textSize.Height - baseline),
                new Size(textSize.Width, textSize.Height + baseline)), Colors[classes], Cv2.FILLED);
            var textColor = Cv2.Mean(Colors[classes]).Val0 < 70 ? Scalar.White : Scalar.Black;
            Cv2.PutText(image, label, new Point((int)x1, rect.Y - rect.Height / 2 - baseline), HersheyFonts.HersheyTriplex, 0.3, textColor);
        }
    }
}
