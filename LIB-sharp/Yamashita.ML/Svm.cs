using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.ML;

namespace Yamashita.ML
{
    /// <summary>
    /// OpenCvSharp 'SVM' class wrapper.
    /// This class has accumulate & save & load methods for machine learning.
    /// </summary>
    public class Svm : Stats
    {

        // ------- Fields ------- //

        private SVM _classifier;


        // ------- Constructor ------- //

        /// <summary>
        /// OpenCvSharp 'SVM' class wrapper.
        /// This class has accumulate & save & load methods for machine learning.
        /// </summary>
        public Svm(Mode mode, string modelPath = "SvmModel.xml", string dataPath = "SvmTrainData.csv")
            : base(mode, modelPath, dataPath) { }


        // ------- Methods ------- //

        protected override void LoadModel() => _classifier = SVM.Load(_modelPath);

        public override void Train(bool append = true, double? param = 0.1)
        {
            if (_mode != Mode.Train) new Exception("Mode should be 'Train'.");
            SaveDataset(append);
            using var svm = SVM.Create();
            svm.KernelType = SVM.KernelTypes.Rbf;
            svm.Gamma = (double)param;
            var list = new List<float>();
            _features.ForEach(f => list.AddRange(f));
            using var featureMat = new Mat(_features.Count, list.Count / _features.Count, MatType.CV_32F, list.ToArray());
            using var labelMat = new Mat(_labels.Count, 1, MatType.CV_32S, _labels.ToArray());
            svm.Train(featureMat, SampleTypes.RowSample, labelMat);
            svm.Save(_modelPath);
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

    }
}
