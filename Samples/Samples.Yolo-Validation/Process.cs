using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenCvSharp;
using Yamashita.Yolo;

namespace Samples.Yolo_Validation
{
    static class Process
    {

        private static IYolo _detector;
        private static List<(string Name, Mat Image, List<(int Label, Point Center, Size Size)> Labels)> _dataSet;
        private static string _className;
        private static int _width;
        private static int _height;
        private static double _iouThresh;

        public static string Init(string modelFolder, int width, int height, int classNum, double iouThresh, string imgDir, string labDir)
        {
            _width = width;
            _height = height;
            _iouThresh = iouThresh;
            var response = Load(modelFolder, classNum, imgDir, labDir);
            if (response != "Success") return response;
            return response;
        }

        public static (string ClassName, int TotalTime, int AvgTime, int Tp, int Fp, int Fn, double P, double R, double Ap) Go(bool? save = false)
        {
            var totalTp = 0;
            var totalFp = 0;
            var totalFn = 0;
            var totalTime = 0;
            var strs = new List<string>();
            var allList = new List<(float Confidence, bool Correct)>();

            foreach (var ds in _dataSet)
            {
                var img = ds.Image;

                var watch = new Stopwatch();
                watch.Start();
                _detector.Run(ref img, out var results);
                watch.Stop();
                var time = watch.ElapsedMilliseconds;

                var tp = 0;
                foreach (var r in results)
                {
                    if (r.Label != _className) continue;
                    var correct = false;
                    var iou = 0.0;
                    foreach (var l in ds.Labels)
                    {
                        iou = CalcIou(r.Center, r.Size, l.Center, l.Size);
                        if (iou >= _iouThresh)
                        {
                            tp++;
                            correct = true;
                            break;
                        }
                    }
                    allList.Add(((float)(r.Confidence * iou), correct));
                }
                var fp = results.Count - tp;
                var fn = ds.Labels.Count - tp;

                strs.Add($"{ds.Name},{time},{tp},{fp},{fn}");
                totalTp += tp;
                totalFp += fp;
                totalFn += fn;
                totalTime += (int)time;
            }
            var truthCount = 0;
            foreach (var ds in _dataSet) truthCount += ds.Labels.Count;
            var ap = CalcAp(allList, truthCount);
            var avgTime = totalTime / _dataSet.Count;
            var precision = (double)totalTp / (totalTp + totalFp);
            var recall = (double)totalTp / (totalTp + totalFn);
            if ((bool)save)
            {
                using (var sr = new StreamWriter($"Validation.csv"))
                {
                    sr.WriteLine($"Width,{_width},Height,{_height}");
                    sr.WriteLine("");
                    sr.WriteLine("TotalTime,AverageTime,TotalTp,TotalFp,TotalFn");
                    sr.WriteLine($"{totalTime},{avgTime},{totalTp},{totalFp},{totalFn}");
                    sr.WriteLine("");
                    sr.WriteLine("Precision,Recall,Ap");
                    sr.WriteLine($"{precision},{recall},{ap}");
                    sr.WriteLine("");
                    sr.WriteLine("FileName,Time,Tp,Fp,Fn");
                    foreach (var s in strs) sr.WriteLine(s);
                }
            }
            return (_className, totalTime, avgTime, totalTp, totalFp, totalFn, precision, recall, ap);
        }

        private static string Load(string modelFolder, int classNum, string imgDir, string labDir)
        {
            var cfg = "";
            var names = "";
            var weights = "";
            foreach (var file in Directory.GetFiles(modelFolder))
            {
                if (Path.GetExtension(file) == ".cfg") cfg = file;
                else if (Path.GetExtension(file) == ".names") names = file;
                else if (Path.GetExtension(file) == ".weights") weights = file;
            }
            if (cfg == "" || names == "" || weights == "") return "Model Not Found Error";
            var count = 0;
            using (var sr = new StreamReader(names))
            {
                while (sr.Peek() != -1)
                {
                    var className = sr.ReadLine();
                    if (count++ == classNum) _className = className;
                }
            }
            if (classNum < 0 || classNum > count - 1) return "Invalid Class Number Error";
            if (_iouThresh < 0.0 || _iouThresh > 1.0) return "Invalid IoU Threshold Range Error";
            try
            {
                _detector = new Yolo(cfg, names, weights, new Size(_width, _height), DrawingMode.Off, 0.25f, 0.01f);
            }
            catch
            {
                return "Model Loading Error";
            }
            var imgPaths = new List<string>();
            var labPaths = new List<string>();
            try
            {

                imgPaths.AddRange(Directory.GetFiles(imgDir, $"*.png"));
                imgPaths.AddRange(Directory.GetFiles(imgDir, $"*.jpg"));
            }
            catch
            {
                return "Image Folder Error";
            }
            try
            {
                labPaths = new List<string>(Directory.GetFiles(labDir, $"*.txt"));
            }
            catch
            {
                return "Label Folder Error";
            }
            try
            {
                _dataSet = new List<(string Name, Mat Image, List<(int Label, Point Center, Size Size)> Labels)>();
                for (int i = 0; i < imgPaths.Count; i++)
                {
                    var img = new Mat(imgPaths[i]);
                    var name = Path.GetFileNameWithoutExtension(imgPaths[i]);
                    foreach (var lp in labPaths)
                    {
                        if (name == Path.GetFileNameWithoutExtension(lp))
                        {
                            using (var sw = new StreamReader(lp))
                            {
                                var labList = new List<(int, Point, Size)>();
                                while (sw.Peek() != -1)
                                {
                                    var s = sw.ReadLine().Split(" ");
                                    var label = int.Parse(s[0]);
                                    var x = (int)(double.Parse(s[1]) * img.Width);
                                    var y = (int)(double.Parse(s[2]) * img.Height);
                                    var w = (int)(double.Parse(s[3]) * img.Width);
                                    var h = (int)(double.Parse(s[4]) * img.Height);
                                    labList.Add((label, new Point(x, y), new Size(w, h)));
                                }
                                _dataSet.Add((name, img, labList));
                            }
                            break;
                        }
                    }
                }
            }
            catch
            {
                return "Images and Labels Matching Error";
            }
            return "Success";
        }

        private static double CalcIou(Point p1, Size s1, Point p2, Size s2)
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

        private static double CalcAp(List<(float Confidence, bool Correct)> list, int truthCount)
        {
            list.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
            var totalArea = 0.0;
            var tpCount = 0;
            var fpCount = 0;
            var tmp = new List<(double R, double P)>();

            foreach (var item in list)
            {
                if (item.Correct == false)
                {
                    fpCount++;
                }
                else
                {
                    tpCount++;
                    var p = (double)tpCount / (tpCount + fpCount);
                    var r = (double)tpCount / truthCount;
                    tmp.Add((r, p));
                }
            }

            var points = new List<(double R, double P)>();
            foreach (var t1 in tmp)
            {
                var t = t1;
                foreach (var t2 in tmp)
                {
                    if (t2.R > t1.R && t2.P > t1.P) t = t2;
                }
                points.Add(t);
            }

            var preR = 0.0;
            foreach (var pt in points)
            {
                totalArea += (pt.R - preR) * pt.P;
                preR = pt.R;
            }

            return totalArea;
        }
    }
}
