using System.Collections.Generic;
using System.IO;

namespace Tools.NncDataArranger
{
    static class TTSplit
    {
        public static void Run(string inputFile, string outputDir, double testRate)
        {

            using var sw_train = new StreamWriter($"{outputDir}\\Train.csv");
            using var sw_test = new StreamWriter($"{outputDir}\\Test.csv");
            using var sr = new StreamReader(inputFile);
            var strs = new List<string[]>();
            while (sr.Peek() != -1)
                strs.Add(sr.ReadLine().Split(","));
            foreach (var s in strs[0])
            {
                sw_train.Write($"{s},");
                sw_test.Write($"{s},");
            }
            sw_train.WriteLine();
            sw_test.WriteLine();

            var count = 0;
            if (testRate < 0) testRate = 0.0;
            else if (testRate > 1) testRate = 1.0;
            for (int i = 1; i < strs.Count; i++)
            {
                if (count++ % (1.0 / testRate) == 0)
                {
                    foreach (var s in strs[i])
                        sw_test.Write($"{s},");
                    sw_test.WriteLine();
                }
                else
                {
                    foreach (var s in strs[i])
                        sw_train.Write($"{s},");
                    sw_train.WriteLine();
                }
            }
        }
    }
}
