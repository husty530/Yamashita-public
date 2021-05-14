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
            for (int i = 0; i < strs[0].Length; i++)
            {
                sw_train.Write($"{strs[0][i]}");
                sw_test.Write($"{strs[0][i]}");
                if (i != strs[0].Length - 1)
                {
                    sw_train.Write(",");
                    sw_test.Write(",");
                }
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
                    for (int j = 0; j < strs[i].Length; j++)
                    {
                        sw_test.Write($"{strs[i][j]}");
                        if (j != strs[i].Length - 1)
                        {
                            sw_test.Write(",");
                        }
                    }
                    sw_test.WriteLine();
                }
                else
                {
                    for (int j = 0; j < strs[i].Length; j++)
                    {
                        sw_train.Write($"{strs[i][j]}");
                        if (j != strs[i].Length - 1)
                        {
                            sw_train.Write(",");
                        }
                    }
                    sw_train.WriteLine();
                }
            }
        }
    }
}
