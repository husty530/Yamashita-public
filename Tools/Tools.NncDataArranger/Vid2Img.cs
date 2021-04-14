using OpenCvSharp;

namespace Tools.NncDataArranger
{
    static class Vid2Img
    {
        public static void Run(string inputFile, string outputDir, Size outputSize)
        {
            using var cap = new VideoCapture(inputFile);
            cap.Set(VideoCaptureProperties.Fps, 1000);
            var count = 0;
            var img = new Mat();
            while (cap.Read(img))
            {
                Cv2.Resize(img, img, outputSize);
                Cv2.ImWrite($"{outputDir}\\{count++:d3}.png", img);
                Cv2.ImShow(" ", img);
                Cv2.WaitKey(1);
            }
        }
    }
}
