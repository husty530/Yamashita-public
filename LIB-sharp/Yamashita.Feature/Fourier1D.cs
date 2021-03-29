using System.Collections.Generic;
using OpenCvSharp;

namespace Yamashita.Feature
{
    public class Fourier1D
    {

        // ------- Fields ------- //

        private readonly double _samplingRate;
        private readonly int _sampleCount;
        private readonly Mat _complex;


        // ------- Constructor ------- //

        /// <summary>
        /// 1D Fourier transformation for Time Series Data.
        /// </summary>
        /// <param name="inputList">List of Value</param>
        /// <param name="dt">Sampling Time</param>
        public Fourier1D(List<double> inputList, double dt)
        {
            _samplingRate = 1.0 / dt;
            _sampleCount = inputList.Count;
            using var real = new Mat(_sampleCount, 1, MatType.CV_32F, inputList.ToArray());
            _complex = new Mat();
            Cv2.Merge(new Mat[] { real, new Mat(_sampleCount, 1, MatType.CV_32F, 0.0) }, _complex);
        }


        // ------- Methods ------- //

        /// <summary>
        /// Discrete Fourier Transformation
        /// </summary>
        /// <param name="result">Histogram of Frequency and Value</param>
        unsafe public void Dft(out List<(double Frequency, float Value)> result)
        {
            Cv2.Dft(_complex, _complex);
            Cv2.Split(_complex, out var planes);
            Cv2.Magnitude(planes[0], planes[1], planes[0]);
            var data = (float*)planes[0].Data;
            result = new List<(double Frequency, float Value)>();
            for (int i = 0; i < _sampleCount / 2; i++)
                result.Add(((double)i / _sampleCount * _samplingRate, data[i]));
        }

        /// <summary>
        /// Discrete Fourier Transformation
        /// </summary>
        /// <param name="feature">Feature of Values</param>
        public void Dft(out float[] feature)
        {
            Dft(out List<(double Frequency, float Value)> list);
            feature = new float[list.Count];
            for (int i = 0; i < feature.Length; i++)
                feature[i] = list[i].Value;
        }

        /// <summary>
        /// Inverse Discrete Fourier Transformation
        /// </summary>
        /// <param name="result">Time and Value</param>
        unsafe public void Idft(out List<(double Time, float Value)> result)
        {
            Cv2.Idft(_complex, _complex, DftFlags.Scale);
            Cv2.Split(_complex, out var planes);
            var data = (float*)planes[0].Data;
            result = new List<(double Time, float Value)>();
            for (int i = 0; i < _sampleCount; i++)
                result.Add((i / _samplingRate, data[i]));
        }

        /// <summary>
        /// Inverse Discrete Fourier Transformation
        /// </summary>
        /// <param name="feature">Feature of Values</param>
        public void Idft(out float[] feature)
        {
            Idft(out List<(double Time, float Value)> list);
            feature = new float[list.Count];
            for (int i = 0; i < feature.Length; i++)
                feature[i] = list[i].Value;
        }

        /// <summary>
        /// Band-pass Filter
        /// </summary>
        /// <param name="minFrequency"></param>
        /// <param name="maxFrequency"></param>
        unsafe public void Filter(double minFrequency, double maxFrequency)
        {
            for (int i = 0; i < _sampleCount; i++)
            {
                var data = (float*)_complex.Data;
                var freq = (double)i / _sampleCount * _samplingRate;
                if (freq < minFrequency || freq > maxFrequency)
                {
                    data[i * 2 + 0] = 0;
                    data[i * 2 + 1] = 0;
                }
            }
        }

    }
}
