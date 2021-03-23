using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace Yamashita.ML
{
    public class Yolo : IYolo
    {

        // フィールド

        private readonly DrawingMode _draw;
        private readonly Net _net;
        private readonly float _threshold;
        private readonly float _nmsThreshold;
        private readonly string[] _labels;
        private readonly Size _blobSize;
        private readonly Scalar[] _colors = Enumerable.Repeat(false, 80).Select(x => Scalar.RandomColor()).ToArray();


        // コンストラクタ

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
            _blobSize = blobSize;
            _draw = draw;
            _threshold = confThresh;
            _nmsThreshold = nmsThresh;
            _labels = File.ReadAllLines(names).ToArray();
            _net = CvDnn.ReadNetFromDarknet(cfg, weights);
        }


        // メソッド

        public void Run(ref Mat frame, out YoloResults results)
        {
            var blob = CvDnn.BlobFromImage(frame, 1.0 / 255, _blobSize, new Scalar(), true, false);
            _net.SetInput(blob);
            var outNames = _net.GetUnconnectedOutLayersNames();
            var outs = outNames.Select(_ => new Mat()).ToArray();
            _net.Forward(outs, outNames);
            results = new YoloResults(GetResults(outs, frame));
        }

        private List<(string Label, float Confidence, Point Center, Size Size)> GetResults(Mat[] output, Mat image)
        {
            var classIds = new List<int>();
            var confidences = new List<float>();
            var probabilities = new List<float>();
            var centers = new List<Point>();
            var boxes = new List<Rect2d>();
            var w = image.Width;
            var h = image.Height;
            unsafe 
            {
                foreach (var pred in output)
                {
                    var p = (float*)pred.Data;
                    for (var i = 0; i < pred.Rows; i++, p += pred.Cols)
                    {
                        var confidence = p[4];
                        if (confidence > _threshold)
                        {
                            Cv2.MinMaxLoc(pred.Row(i).ColRange(5, pred.Cols), out _, out Point classIdPoint);
                            var probability = p[classIdPoint.X + 5];
                            if (probability > _threshold)
                            {
                                var centerX = p[0] * w;
                                var centerY = p[1] * h;
                                var width = p[2] * w;
                                var height = p[3] * h;
                                classIds.Add(classIdPoint.X);
                                confidences.Add(confidence);
                                probabilities.Add(probability);
                                centers.Add(new Point(centerX, centerY));
                                boxes.Add(new Rect2d(centerX - width / 2, centerY - height / 2, width, height));
                            }
                        }
                    }
                }
            }
            var results = new List<(string Label, float Confidence, Point Center, Size Size)>();
            CvDnn.NMSBoxes(boxes, confidences, _threshold, _nmsThreshold, out int[] indices);
            foreach (var i in indices)
            {
                results.Add((_labels[classIds[i]], confidences[i], centers[i], new Size(boxes[i].Width, boxes[i].Height)));
                switch (_draw)
                {
                    case (DrawingMode.Off):
                        break;
                    case (DrawingMode.Point):
                        DrawPoint(image, classIds[i], centers[i]);
                        break;
                    case (DrawingMode.Rectangle):
                        DrawRect(image, classIds[i], confidences[i], centers[i], new Size(boxes[i].Width, boxes[i].Height));
                        break;
                }
            }
            return results;
        }

        private void DrawPoint(Mat image, int classes, Point center)
        {
            image.Circle(center.X, center.Y, 3, _colors[classes], 5);
        }

        private void DrawRect(Mat image, int classes, float confidence, Point center, Size size)
        {
            var label = $"{_labels[classes]}{confidence * 100:0}%";
            var x1 = Math.Max(center.X - size.Width / 2, 0);
            var y1 = Math.Max(center.Y - size.Height / 2, 0);
            var x2 = Math.Min(center.X + size.Width / 2, image.Width);
            var y2 = Math.Min(center.Y + size.Height / 2, image.Height);
            var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheyTriplex, 0.3, 0, out var baseline);
            Cv2.Rectangle(image, new Rect(new Point(x1, y1 - textSize.Height - baseline),
                new Size(textSize.Width, textSize.Height + baseline)), _colors[classes], Cv2.FILLED);
            var textColor = Cv2.Mean(_colors[classes]).Val0 < 70 ? Scalar.White : Scalar.Black;
            Cv2.PutText(image, label, new Point(x1, y1 - baseline), HersheyFonts.HersheyTriplex, 0.3, textColor);
        }

    }
}
