using System.Collections.Generic;
using System.IO;
using OpenCvSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tools.NncDataArranger
{
    static class J2Mask
    {
        public static void Run(string jsonDir, string outputDir, Size size)
        {
            var files = Directory.GetFiles(jsonDir);
            foreach (var file in files)
            {
                if (Path.GetExtension(file) != ".json") continue;
                var json = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(file));
                var w = (int)json["imageWidth"];
                var h = (int)json["imageHeight"];
                var img = new Mat(h, w, MatType.CV_8U, 0);
                foreach (var j in json["shapes"])
                {
                    var poly = new List<Point>();
                    foreach (var p in j["points"])
                        poly.Add(new Point((int)p[0], (int)p[1]));
                    Cv2.FillPoly(img, new List<Point>[] { poly }, 255);
                }
                Cv2.Resize(img, img, size);
                Cv2.ImWrite($"{outputDir}\\{Path.GetFileNameWithoutExtension(file)}.png", img);
            }
        }
    }
}
