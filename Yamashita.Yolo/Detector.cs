using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace Yamashita.Yolo
{
    public class Detector
    {
        private bool _initialized = false;
        private Net _net;
        private float _threshold;
        private float _nmsThreshold;
        private static string[] _labels;

        //Property
        private static readonly Scalar[] Colors = Enumerable.Repeat(false, 80).Select(x => Scalar.RandomColor()).ToArray();
        private static Size BlobSize { set; get; }

        //Method
        /// <summary>
        /// 検出器の初期化
        /// </summary>
        /// <param name="cfg">.cfgファイル</param>
        /// <param name="names">.namesファイル</param>
        /// <param name="weights">.weightsファイル</param>
        /// <param name="blobSize">入力サイズ</param>
        /// <param name="confThresh">確信度の閾値</param>
        /// <param name="nmsThresh">重なりの閾値</param>
        public void InitializeDetector(string cfg, string names, string weights, Size blobSize, float confThresh = 0.5f, float nmsThresh = 0.3f)
        {
            BlobSize = blobSize;
            _threshold = confThresh;
            _nmsThreshold = nmsThresh;
            _labels = File.ReadAllLines(names).ToArray();
            _net = CvDnn.ReadNetFromDarknet(cfg, weights);
            _net.SetPreferableBackend(Net.Backend.OPENCV);
            _net.SetPreferableTarget(Net.Target.CPU);
            _initialized = true;
        }

        /// <summary>
        /// 一枚の画像に対する処理
        /// </summary>
        /// <param name="inputImg">入力画像</param>
        /// <returns></returns>
        public (List<string> classNames, List<Point> Centers, List<float> Confidences, List<Rect2d> Boxes) Run(ref Mat inputImg)
        {
            if (!_initialized) throw new ApplicationException("Detector is not Initialized yet!");
            var blob = CvDnn.BlobFromImage(inputImg, 1.0 / 255, BlobSize, new Scalar(), true, false);
            _net.SetInput(blob);
            var outNames = _net.GetUnconnectedOutLayersNames();
            var outs = outNames.Select(_ => new Mat()).ToArray();
            _net.Forward(outs, outNames);
            return GetResult(outs, ref inputImg, _threshold, _nmsThreshold);
        }

        private static (List<string>, List<Point>, List<float>, List<Rect2d>) GetResult(IEnumerable<Mat> output, ref Mat image, float threshold, float nmsThreshold)
        {
            var centers = new List<Point>();
            var classIds = new List<int>();
            var classNames = new List<string>();
            var confidences = new List<float>();
            var probabilities = new List<float>();
            var boxes = new List<Rect2d>();
            var selectedBoxes = new List<Rect2d>();
            var selectedConf = new List<float>();
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
                var box = boxes[i];
                selectedBoxes.Add(box);
                selectedConf.Add(confidences[i]);
                classNames.Add(_labels[classIds[i]]);
                centers.Add(new Point(box.X, box.Y));
                DrawRect(ref image, classIds[i], confidences[i], box.X, box.Y, box.Width, box.Height);
            }
            return (classNames, centers, selectedConf, selectedBoxes);
        }

        private static void DrawPoint(ref Mat image, int classes, double centerX, double centerY)
        {
            image.Circle((int)centerX, (int)centerY, 3, Colors[classes], 5);
        }

        private static void DrawRect(ref Mat image, int classes, float confidence, double centerX, double centerY, double width, double height)
        {
            var label = $" {_labels[classes]} {confidence * 100:0.00}%";
            Console.WriteLine($"confidence {confidence * 100:0.00}% , {label}");
            var x1 = (centerX - width / 2) < 0 ? 0 : centerX - width / 2;
            image.Rectangle(new Point(x1, centerY - height / 2), new Point(centerX + width / 2, centerY + height / 2), Colors[classes], 1);
            var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheyTriplex, 0.3, 0, out var baseline);
            Cv2.Rectangle(image, new Rect(new Point(x1, centerY - height / 2 - textSize.Height - baseline),
                new Size(textSize.Width, textSize.Height + baseline)), Colors[classes], Cv2.FILLED);
            var textColor = Cv2.Mean(Colors[classes]).Val0 < 70 ? Scalar.White : Scalar.Black;
            Cv2.PutText(image, label, new Point(x1, centerY - height / 2 - baseline), HersheyFonts.HersheyTriplex, 0.3, textColor);
        }
    }
}
