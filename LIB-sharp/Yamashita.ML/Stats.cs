using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp.ML;

namespace Yamashita.ML
{
    public abstract class Stats : IStats
    {

        // フィールド

        protected Mode _mode;
        protected string _modelPath;
        protected List<float[]> _features;
        protected List<int> _labels;
        private readonly string _dataPath;


        // コンストラクタ

        /// <summary>
        /// Yamashita専用統計分類器
        /// </summary>
        /// <param name="mode">学習か推論か</param>
        /// <param name="modelPath">学習モデル(.xml)</param>
        /// <param name="dataPath">学習データ格納テキスト(.csv)</param>
        public Stats(Mode mode, string modelPath, string dataPath)
        {
            _mode = mode;
            _modelPath = modelPath;
            _dataPath = dataPath;
            if (_mode == Mode.Train)
            {
                _features = new List<float[]>();
                _labels = new List<int>();
                LoadDataset();
            }
            else
            {
                LoadModel();
            }
        }


        // メソッド

        public void AddData(float[] feature, int label)
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            if (label != 0 || label != 1) new Exception("Label value should be 0 or 1.");
            _features.Add(feature);
            _labels.Add(label);
        }

        public void RemoveLastData()
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            if (_features.Count != 0)
            {
                _features.RemoveAt(_features.Count - 1);
                _labels.RemoveAt(_labels.Count - 1);
            }
        }

        public void ClearDataset()
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            _features.Clear();
            _labels.Clear();
        }

        public void SaveDataset(bool append)
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            using var sw = new StreamWriter(_dataPath, append);
            int count = 0;
            _features.ForEach(f =>
            {
                for (int i = 0; i < f.Length; i++) sw.Write($"{f[i]},");
                sw.Write($"{_labels[count++]}\n");
            });
        }

        public abstract void Train(bool append, double? param);

        public abstract void Predict(List<float[]> input, out List<float> output);

        protected abstract void LoadModel();

        protected void LoadDataset()
        {
            if (File.Exists(_dataPath))
            {
                using var sr = new StreamReader(_dataPath);
                var strs = sr.ReadLine().Split(",");
                var feature = new List<float>();
                for (int i = 0; i < strs.Length - 1; i++)
                    feature.Add(float.Parse(strs[i]));
                _features.Add(feature.ToArray());
                _labels.Add(int.Parse(strs[strs.Length - 1]));
            }
        }

    }
}
