using OpenCvSharp;
using System.IO;

namespace Tools.NncDataArranger
{
    static class Vid2Img
    {
        public static void Run(string inputFile, string outputDir, Size outputSize, int skip)
        {
            using var cap = new VideoCapture(inputFile);
            cap.Set(VideoCaptureProperties.Fps, 1000);
            skip = skip < 1 ? 1 : skip + 1;
            var count = 0;
            var imnum = 0;
            var img = new Mat();
            while (cap.Read(img))
            {
                if (count++ % skip == 0)
                {
                    Cv2.Resize(img, img, outputSize);
                    while (File.Exists($"{outputDir}\\{imnum:d3}.png")) imnum++;
                    Cv2.ImWrite($"{outputDir}\\{imnum:d3}.png", img);
                    Cv2.ImShow(" ", img);
                    Cv2.WaitKey(1);
                }
            }
        }
    }
}
