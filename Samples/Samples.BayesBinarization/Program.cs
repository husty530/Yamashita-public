using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenCvSharp;
using Yamashita.ML;

namespace Samples.BayesBinarization
{
    class Program
    {

        private static int _width;
        private static int _height;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\nTrain is 0. Inference is 1.");
                Console.Write("Input : ");
                var mode = int.Parse(Console.ReadLine());
                Console.WriteLine("Target image's path. The image must have 3 channels");
                Console.Write("Input : ");
                var path = Console.ReadLine();
                using var image = Cv2.ImRead(path);
                Cv2.Resize(image, image, new Size(640, 480));
                _width = image.Width;
                _height = image.Height;
                if (mode == 0)
                {
                    Console.WriteLine("Select 'Positive' Areas. 'Enter' or 'Space' finishes current rectangle, 'Esc' determinates your selections.");
                    var zeroRects = Cv2.SelectROIs("Select 'Positive' Areas", image, false);
                    Console.WriteLine($"{zeroRects.Length} Rects is selected.");
                    Console.WriteLine("Select 'Negative' Areas. 'Enter' or 'Space' finishes current rectangle, 'Esc' determinates your selections.");
                    var oneRects = Cv2.SelectROIs("Select 'Negative' Areas", image, false);
                    Console.WriteLine($"{oneRects.Length} Rects is selected.");
                    var bay = new BayesClassifier(Mode.Train);
                    foreach (var r in zeroRects)
                        foreach (var bgr in ImageToNormalizedList(image[r].Clone())) bay.AddData(bgr, 0);
                    foreach (var r in oneRects)
                        foreach (var bgr in ImageToNormalizedList(image[r].Clone())) bay.AddData(bgr, 1);
                    bay.Train(false);
                }
                else if (mode == 1)
                {
                    var bgrs = ImageToNormalizedList(image);
                    var bay = new BayesClassifier(Mode.Inference);
                    bay.PredictProb(bgrs, out var output, out var probability);
                    Console.WriteLine("'Binary' is 0. 'Probability' is 1.");
                    Console.Write("Input : ");
                    var choice = int.Parse(Console.ReadLine());
                    var watch = new Stopwatch();
                    watch.Start();
                    if (choice == 0)
                    {
                        using var result = new Mat();
                        Cv2.BitwiseNot(ListToImage(output), result);
                        Cv2.ImShow(" ", result);
                    }
                    else if (choice == 1) Cv2.ImShow(" ", ListToImage(probability));
                    watch.Stop();
                    Console.WriteLine($"{watch.ElapsedMilliseconds} ms");
                    Cv2.WaitKey();
                }
                Console.WriteLine("'Finish' is 0. 'Continue' is 1.");
                Console.Write("Input : ");
                if (int.Parse(Console.ReadLine()) != 1) break;
                Cv2.DestroyAllWindows();
            }
        }

        unsafe private static List<float[]> ImageToNormalizedList(Mat image)
        {
            var d = image.DataPointer;
            var output = new List<float[]>();
            for (int i = 0; i < image.Width * image.Height; i++)
                output.Add(new float[] { d[i * 3 + 0] / 255f, d[i * 3 + 1] / 255f, d[i * 3 + 2] / 255f });
            return output;
        }

        unsafe private static Mat ListToImage(List<float> list)
        {
            var max = list.Max();
            var gray = new Mat(_height, _width, MatType.CV_8U);
            var d = gray.DataPointer;
            for (int i = 0; i < _width * _height; i++) d[i] = (byte)(list[i] * 255 / max);
            return gray;
        }
    }
}
