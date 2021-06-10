using System.IO;
using System.IO.Compression;
using OpenCvSharp;

namespace Tools.NncDataArranger
{
    public static class Zip2Img
    {

        public static void Run(string inputDir, string outputDir, string which)
        {
            var zips = Directory.GetFiles(inputDir);
            var count = 0;
            foreach (var z in zips)
            {
                using var archive = ZipFile.OpenRead(z);
                switch (which)
                {
                    case "C":
                        archive.GetEntry("C.png").ExtractToFile("C.png", true);
                        var color = new Mat("C.png");
                        while (File.Exists($"{outputDir}\\C_{count:D4}.png")) count++;
                        Cv2.ImWrite($"{outputDir}\\C_{count:D4}.png", color);
                        File.Delete("C.png");
                        break;
                    case "D":
                        archive.GetEntry("D.png").ExtractToFile("D.png", true);
                        var depth = new Mat("D.png", ImreadModes.Unchanged);
                        while (File.Exists($"{outputDir}\\D_{count:D4}.png")) count++;
                        Cv2.ImWrite($"{outputDir}\\D_{count:D4}.png", depth);
                        File.Delete("D.png");
                        break;
                    case "P":
                        archive.GetEntry("P.png").ExtractToFile("P.png", true);
                        var pc = new Mat("P.png", ImreadModes.Unchanged);
                        while (File.Exists($"{outputDir}\\P_{count:D4}.png")) count++;
                        Cv2.ImWrite($"{outputDir}\\P_{count:D4}.png", pc);
                        File.Delete("P.png");
                        break;
                }
                
                
            }
        }

    }

}
