using System.Linq;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;

namespace Tools.Yolo_Labeller
{
    static class Process
    {
        private static Size _size;
        private static List<(Rect Rect, int LabelIndex)>[] _items;
        private static Point _temp;
        private static Mat _image;
        private static string[] _imgPaths;
        private static Scalar[] _colors;
        private static string _saveDirectory;
        public static int FrameNumber { set; get; }
        public static int FileCount => _imgPaths.Length;
        public static int RectCount => _items[FrameNumber].Count;

        public static Mat Initialize(string dir, string saveDir, string[] labels, int width, int height)
        {
            FrameNumber = 0;
            _saveDirectory = saveDir;
            _size = new Size(width, height);
            var imgPaths = new List<string>();
            imgPaths.AddRange(Directory.GetFiles(dir, $"*.png"));
            imgPaths.AddRange(Directory.GetFiles(dir, $"*.jpg"));
            _imgPaths = imgPaths.ToArray();
            _items = new List<(Rect, int)>[_imgPaths.Length];
            if (_imgPaths.Length == 0) return new Mat(height, width, MatType.CV_8U, 0);
            _colors = Enumerable.Repeat(false, labels.Length).Select(i => Scalar.RandomColor()).ToArray();
            for (int i = 0; i < _imgPaths.Length; i++)
            {
                _items[i] = new List<(Rect, int)>();
                var labelPath = Path.ChangeExtension(_imgPaths[i], ".txt");
                if (File.Exists(labelPath))
                {
                    using var sr = new StreamReader(labelPath);
                    while (sr.Peek() != -1)
                    {
                        var str = sr.ReadLine().Split(" ");
                        var label = int.Parse(str[0]);
                        var w = (int)(double.Parse(str[3]) * _size.Width);
                        var h = (int)(double.Parse(str[4]) * _size.Height);
                        var l = (int)(double.Parse(str[1]) * _size.Width) - w / 2;
                        var t = (int)(double.Parse(str[2]) * _size.Height) - h / 2;
                        _items[i].Add((new Rect(l, t, w, h), label));
                    }
                }
            }
            _image = new Mat(_imgPaths[0]);
            DrawAll(ref _image);
            return _image;
        }

        public static Mat GoBack()
        {
            _image = new Mat(_imgPaths[--FrameNumber]);
            DrawAll(ref _image);
            return _image;
        }

        public static Mat GoNext()
        {
            _image = new Mat(_imgPaths[++FrameNumber]);
            DrawAll(ref _image);
            return _image;
        }

        public static Mat SelectStart(int x, int y)
        {
            _temp = new Point(x, y);
            var viewImg = _image.Clone();
            DrawAll(ref viewImg);
            return viewImg;
        }

        public static Mat Drag(int x, int y)
        {
            if (x == _temp.X || y == _temp.Y) return _image;
            var rect = new Rect();
            if (x > _temp.X && y > _temp.Y) rect = new Rect(_temp, new Size(x - _temp.X, y - _temp.Y));
            if (x > _temp.X && y < _temp.Y) rect = new Rect(new Point(_temp.X, y), new Size(x - _temp.X, _temp.Y - y));
            if (x < _temp.X && y > _temp.Y) rect = new Rect(new Point(x, _temp.Y), new Size(_temp.X - x, y - _temp.Y));
            if (x < _temp.X && y < _temp.Y) rect = new Rect(new Point(x, y), new Size(_temp.X - x, _temp.Y - y));
            var viewImg = _image.Clone();
            DrawAll(ref viewImg);
            Cv2.Rectangle(viewImg, rect, new Scalar(0, 255, 0), 3);
            return viewImg;
        }

        public static Mat SelectGoal(int x, int y, int labelIndex)
        {
            if (x == _temp.X || y == _temp.Y) return _image;
            var rect = new Rect();
            if (x > _temp.X && y > _temp.Y) rect = new Rect(_temp, new Size(x - _temp.X, y - _temp.Y));
            if (x > _temp.X && y < _temp.Y) rect = new Rect(new Point(_temp.X, y), new Size(x - _temp.X, _temp.Y - y));
            if (x < _temp.X && y > _temp.Y) rect = new Rect(new Point(x, _temp.Y), new Size(_temp.X - x, y - _temp.Y));
            if (x < _temp.X && y < _temp.Y) rect = new Rect(new Point(x, y), new Size(_temp.X - x, _temp.Y - y));
            _items[FrameNumber].Add((rect, labelIndex));
            var viewImg = _image.Clone();
            DrawAll(ref viewImg);
            return viewImg;
        }

        public static void Save()
        {
            var filename = Path.GetFileNameWithoutExtension(_imgPaths[FrameNumber]);
            using var sw = new StreamWriter($"{_saveDirectory}\\{filename}.txt", false);
            foreach (var item in _items[FrameNumber])
            {
                var centerX = (double)(item.Rect.Left + item.Rect.Width / 2) / _size.Width;
                var centerY = (double)(item.Rect.Top + item.Rect.Height / 2) / _size.Height;
                var width = (double)item.Rect.Width / _size.Width;
                var height = (double)item.Rect.Height / _size.Height;
                sw.WriteLine($"{item.LabelIndex} {centerX:f6} {centerY:f6} {width:f6} {height:f6}");
            }
        }

        public static Mat Clear()
        {
            _image = new Mat(_imgPaths[FrameNumber]);
            Cv2.Resize(_image, _image, _size);
            _items[FrameNumber].Clear();
            return _image;
        }

        public static Mat RemoveLast()
        {
            _items[FrameNumber].RemoveAt(_items[FrameNumber].Count - 1);
            _image = new Mat(_imgPaths[FrameNumber]);
            DrawAll(ref _image);
            return _image;
        }

        private static void DrawAll(ref Mat img)
        {
            Cv2.Resize(img, img, _size);
            foreach (var item in _items[FrameNumber])
                Cv2.Rectangle(img, item.Rect, _colors[item.LabelIndex], 3);
        }
    }
}
