using System.IO;
using System.IO.Compression;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{

    public static class ImageIO
    {

        // ------- Methods ------- //

        /// <summary>
        /// Save RGB, Depth, and PointCloud Image in Zip File.
        /// </summary>
        /// <param name="saveDirectory">Target Directory</param>
        /// <param name="baseName">To Identify</param>
        public static void SaveAsZip(string saveDirectory, string baseName, BgrXyzMat input)
        {
            var zipFileNumber = 0;
            if (!Directory.Exists(saveDirectory)) throw new System.Exception("Directory doesn't Exist!");
            while (File.Exists($"{saveDirectory}\\Image_{baseName}{zipFileNumber:D4}.zip")) zipFileNumber++;
            var filePath = $"{saveDirectory}\\Image_{baseName}{zipFileNumber:D4}.zip";
            Cv2.ImWrite($"{filePath}_C.png", input.BGR);
            Cv2.ImWrite($"{filePath}_D.png", input.Depth16);
            Cv2.ImWrite($"{filePath}_P.png", input.XYZ);
            using var z = ZipFile.Open($"{filePath}", ZipArchiveMode.Update);
            z.CreateEntryFromFile($"{filePath}_C.png", $"C.png", CompressionLevel.Optimal);
            z.CreateEntryFromFile($"{filePath}_D.png", $"D.png", CompressionLevel.Optimal);
            z.CreateEntryFromFile($"{filePath}_P.png", $"P.png", CompressionLevel.Optimal);
            File.Delete($"{filePath}_C.png");
            File.Delete($"{filePath}_D.png");
            File.Delete($"{filePath}_P.png");
        }

        /// <summary>
        /// Extract Images from Zip File.
        /// </summary>
        /// <param name="filePath">Zip File Path</param>
        /// <returns></returns>
        public static BgrXyzMat OpenZip(string filePath)
        {
            if (!File.Exists(filePath)) throw new System.Exception("File doesn't Exist!");
            using var archive = ZipFile.OpenRead(filePath);
            archive.GetEntry("C.png").ExtractToFile(@"C.png", true);
            archive.GetEntry("P.png").ExtractToFile(@"P.png", true);
            return new BgrXyzMat(new Mat(@"C.png"), new Mat(@"P.png", ImreadModes.Unchanged));
        }

    }
}
