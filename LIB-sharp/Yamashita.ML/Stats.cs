using System;
using System.Collections.Generic;
using System.IO;

namespace Yamashita.ML
{
    public abstract class Stats : IStats
    {

        // ------- Fields ------- //

        protected Mode _mode;
        protected string _modelPath;
        protected List<float[]> _features;
        protected List<int> _labels;
        private readonly string _dataPath;


        // ------- Constructor ------- //

        /// <summary>
        /// For Yamashita's private use
        /// </summary>
        /// <param name="mode">Train or Inference</param>
        /// <param name="modelPath">(.xml)</param>
        /// <param name="dataPath">(.csv)</param>
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


        // ------- Methods ------- //

        /// <summary>
        /// Push back one vector data on dataset
        /// </summary>
        /// <param name="feature">Feature vector, such as HOG</param>
        /// <param name="label">0 or 1 value for classification</param>
        public void AddData(float[] feature, int label)
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            if (label != 0 || label != 1) new Exception("Label value should be 0 or 1.");
            _features.Add(feature);
            _labels.Add(label);
        }

        /// <summary>
        /// Remove n-1 index of data from dataset
        /// </summary>
        public void RemoveLastData()
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            if (_features.Count != 0)
            {
                _features.RemoveAt(_features.Count - 1);
                _labels.RemoveAt(_labels.Count - 1);
            }
        }

        /// <summary>
        /// Remove all data from dataset
        /// </summary>
        public void ClearDataset()
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            _features.Clear();
            _labels.Clear();
        }

        /// <summary>
        /// Save model at the same directory of dataset file.
        /// </summary>
        /// <param name="append"></param>
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
