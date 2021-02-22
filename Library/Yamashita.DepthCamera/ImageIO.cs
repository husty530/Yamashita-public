using System.IO;
using System.IO.Compression;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{
    public static class ImageIO
    {
        public static void SaveAsZip(string saveDirectory, string baseName, Mat colorMat, Mat depthMat, Mat pointCloudMat)
        {
            var zipFileNumber = 0;
            while (File.Exists($"{saveDirectory}\\Image_{baseName}{zipFileNumber:D4}.zip")) zipFileNumber++;
            var filePath = $"{saveDirectory}\\Image_{baseName}{zipFileNumber:D4}.zip";
            Cv2.ImWrite($"{filePath}_C.png", colorMat);
            Cv2.ImWrite($"{filePath}_D.png", depthMat);
            Cv2.ImWrite($"{filePath}_P.png", pointCloudMat);
            using (var z = ZipFile.Open($"{filePath}", ZipArchiveMode.Update))
            {
                z.CreateEntryFromFile($"{filePath}_C.png", $"C.png", CompressionLevel.Optimal);
                z.CreateEntryFromFile($"{filePath}_D.png", $"D.png", CompressionLevel.Optimal);
                z.CreateEntryFromFile($"{filePath}_P.png", $"P.png", CompressionLevel.Optimal);
            }
            File.Delete($"{filePath}_C.png");
            File.Delete($"{filePath}_D.png");
            File.Delete($"{filePath}_P.png");
        }

        public static (Mat Color, Mat Depth, Mat PointCloud) OpenZip(string filePath)
        {
            using (var archive = ZipFile.OpenRead(filePath))
            {
                var e1 = archive.GetEntry("C.png");
                e1.ExtractToFile(@"C.png", true);
                var e2 = archive.GetEntry("D.png");
                e1.ExtractToFile(@"D.png", true);
                var e3 = archive.GetEntry("P.png");
                e3.ExtractToFile(@"P.png", true);
            }
            var color = new Mat(@"C.png");
            var depth = new Mat(@"D.png", ImreadModes.Unchanged);
            var points = new Mat(@"P.png", ImreadModes.Unchanged);
            return (color, depth, points);
        }

    }
}
