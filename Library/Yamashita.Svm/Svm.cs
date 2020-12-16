using System;
using System.IO;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.ML;

namespace Yamashita.Svm
{
    public class Svm
    {

        public enum Mode { Train, Inference }
        private Mode _mode;
        private string _modelPath;
        private string _dataPath;
        private List<float[]> _features;
        private List<int> _labels;
        private SVM _classifier;

        /// <summary>
        /// Yamashita専用サポートベクターマシン
        /// </summary>
        /// <param name="mode">学習か推論か</param>
        /// <param name="modelPath">学習モデル(.xml)</param>
        /// <param name="dataPath">学習データ格納テキスト(.csv)</param>
        public Svm(Mode mode, string modelPath = "SvmModel.xml", string dataPath = "TrainData.csv")
        {
            _mode = mode;
            _modelPath = modelPath;
            _dataPath = dataPath;
            if(_mode == Mode.Train)
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

        // 以下、学習フェーズ

        private void LoadDataset()
        {
            if (File.Exists(_dataPath))
            {
                using(var sr = new StreamReader(_dataPath))
                {
                    var strs = sr.ReadLine().Split(",");
                    var feature = new List<float>();
                    for (int i = 0; i < strs.Length - 1; i++)
                    {
                        feature.Add(float.Parse(strs[i]));
                    }
                    _features.Add(feature.ToArray());
                    _labels.Add(int.Parse(strs[strs.Length - 1]));
                }
            }
        }

        /// <summary>
        /// データを1つ追加
        /// </summary>
        /// <param name="feature">float配列にした特徴量ベクトル</param>
        /// <param name="label">0 or 1</param>
        public void AddData(float[] feature, int label)
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            if (label != 0 || label != 1) new Exception("Label value should be 0 or 1.");
            _features.Add(feature);
            _labels.Add(label);
        }

        /// <summary>
        /// データの末尾を削除
        /// </summary>
        public void RemoveLastData()
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            if(_features.Count != 0)
            {
                _features.RemoveAt(_features.Count - 1);
                _labels.RemoveAt(_labels.Count - 1);
            }
        }

        /// <summary>
        /// データセットをクリア
        /// </summary>
        public void ClearDataset()
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            _features.Clear();
            _labels.Clear();
        }

        /// <summary>
        /// 明示的にデータセットをセーブ
        /// </summary>
        public void SaveDataset()
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            using (var sw = new StreamWriter(_dataPath, true))
            {
                int count = 0;
                foreach (var f in _features)
                {
                    for (int i = 0; i < f.Length; i++)
                    {
                        sw.Write($"{f[i]},");
                    }
                    sw.Write($"{_labels[count++]}\n");
                }
            }
        }

        /// <summary>
        /// 現在のデータセットで学習
        /// </summary>
        /// <param name="gamma">RBFのパラメータ</param>
        public void Train(double gamma = 0.1)
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            SaveDataset();
            using (var svm = SVM.Create())
            {
                svm.KernelType = SVM.KernelTypes.Rbf;
                svm.Gamma = gamma;
                var list = new List<float>();
                foreach (var feature in _features)
                {
                    list.AddRange(feature);
                }
                var featureMat = new Mat(_features.Count, list.Count / _features.Count, MatType.CV_32F, list.ToArray());
                var labelMat = new Mat(_labels.Count, 1, MatType.CV_32S, _labels.ToArray());
                svm.Train(featureMat, SampleTypes.RowSample, labelMat);
                svm.Save(_modelPath);
            }
        }


        // 以下、推論フェーズ

        private void LoadModel()
        {
            _classifier = SVM.Load(_modelPath);
        }

        /// <summary>
        /// 推論処理
        /// </summary>
        /// <param name="input">float配列にした入力特徴量ベクトル</param>
        /// <param name="result">出力ラベル (0 or 1)</param>
        public void Predict(float[] input, out float result)
        {
            if (_mode != Mode.Inference) new Exception("Mode should be 'Inference'.");
            var inputMat = new Mat(1, input.Length, MatType.CV_32F, input);
            result = _classifier.Predict(inputMat);
        }
    }
}
