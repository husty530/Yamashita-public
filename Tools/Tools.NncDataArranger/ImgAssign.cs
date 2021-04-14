using System.IO;

namespace Tools.NncDataArranger
{
    class ImgAssign
    {

        public static void Run(string dataDir, string maskDir)
        {

            var x = Directory.GetFiles(dataDir);
            var xDirs = dataDir.Split("\\");
            using var sw = new StreamWriter($"{dataDir}\\..\\DataSet.csv");
            sw.WriteLine("x:data,y:label");
            var y = Directory.GetFiles(maskDir);
            var yDirs = maskDir.Split("\\");
            for (int i = 0; i < y.Length; i++)
            {
                var xName = Path.GetFileName(x[i]);
                var yName = Path.GetFileName(y[i]);
                sw.WriteLine($"./{xDirs[xDirs.Length - 1]}/{xName},./{yDirs[yDirs.Length - 1]}/{yName}");
            }

        }
    }
}
