using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.ML;

namespace Yamashita.ML
{
    public class BayesClassifier : Stats
    {

        // フィールド

        private NormalBayesClassifier _classifier;


        // コンストラクタ

        /// <summary>
        /// Yamashita専用ベイズ分類器
        /// </summary>
        /// <param name="mode">学習か推論か</param>
        /// <param name="modelPath"></param>
        /// <param name="dataPath"></param>
        public BayesClassifier(Mode mode, string modelPath = "BayesModel.xml", string dataPath = "BayesTrainData.csv")
            : base(mode, modelPath, dataPath) { }


        // メソッド

        protected override void LoadModel() => _classifier = NormalBayesClassifier.Load(_modelPath);

        public override void Train(bool append = true, double? param = null)
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            SaveDataset(append);
            using var bayes = NormalBayesClassifier.Create();
            var list = new List<float>();
            foreach (var feature in _features) list.AddRange(feature);
            using var featureMat = new Mat(_features.Count, list.Count / _features.Count, MatType.CV_32F, list.ToArray());
            using var labelMat = new Mat(_labels.Count, 1, MatType.CV_32S, _labels.ToArray());
            bayes.Train(featureMat, SampleTypes.RowSample, labelMat);
            bayes.Save(_modelPath);
        }

        public override void Predict(List<float[]> input, out List<float> output)
        {
            if (_mode != Mode.Inference) new Exception("Mode should be 'Inference'.");
            if (input.Count == 0)
            {
                output = new List<float>();
                return;
            }
            using var inputMat = new Mat(input.Count, input[0].Length, MatType.CV_32F, input.SelectMany(i => i).ToArray());
            using var outputMat = new Mat();
            _classifier.Predict(inputMat, outputMat);
            output = new List<float>();
            for (int i = 0; i < outputMat.Rows; i++) output.Add(outputMat.At<float>(i, 0));
        }

        public void PredictProb(List<float[]> input, out List<float> output, out List<float> probability)
        {
            if (_mode != Mode.Inference) new Exception("Mode should be 'Inference'.");
            if (input.Count == 0)
            {
                output = new List<float>();
                probability = new List<float>();
                return;
            }
            using var inputMat = new Mat(input.Count, input[0].Length, MatType.CV_32F, input.SelectMany(i => i).ToArray());
            using var outputMat = new Mat();
            using var probMat = new Mat();
            _classifier.PredictProb(inputMat, outputMat, probMat);
            output = new List<float>();
            probability = new List<float>();
            for (int i = 0; i < outputMat.Rows; i++)
            {
                output.Add(outputMat.At<float>(i, 0));
                probability.Add(probMat.At<float>(i, 0));
            }
        }

    }
}
