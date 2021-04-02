using System.Collections.Generic;

namespace Yamashita.ML
{
    public enum Mode { Train, Inference }

    public interface IStats
    {

        // ----- Train -----

        /// <summary>
        /// </summary>
        /// <param name="feature">Feature Vector</param>
        /// <param name="label">0 or 1</param>
        public void AddData(float[] feature, int label);

        /// <summary>
        /// </summary>
        public void RemoveLastData();

        /// <summary>
        /// </summary>
        public void ClearDataset();

        /// <summary>
        /// Save apparently
        /// </summary>
        public void SaveDataset(bool append);

        /// <summary>
        /// Train with current Dataset.
        /// </summary>
        /// <param name="append"></param>
        /// <param name="param"></param>
        public void Train(bool append, double? param);


        // ----- Inference -----

        /// <summary>
        /// </summary>
        /// <param name="input">Feature Vecto</param>
        /// <param name="output">0 or 1</param>
        public void Predict(List<float[]> input, out List<float> output);
    }
}
