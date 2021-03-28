using System.Collections.Generic;
using OpenCvSharp;

namespace Yamashita.Feature
{
    public class Fourier1D
    {

        // フィールド

        private readonly double _samplingRate;
        private readonly int _sampleCount;
        private readonly Mat _complex;


        // コンストラクタ

        /// <summary>
        /// 時系列データに対する1次元フーリエ解析
        /// </summary>
        /// <param name="inputList">変位のリスト</param>
        /// <param name="dt">サンプリング時間</param>
        public Fourier1D(List<double> inputList, double dt)
        {
            _samplingRate = 1.0 / dt;
            _sampleCount = inputList.Count;
            using var real = new Mat(_sampleCount, 1, MatType.CV_32F, inputList.ToArray());
            _complex = new Mat();
            Cv2.Merge(new Mat[] { real, new Mat(_sampleCount, 1, MatType.CV_32F, 0.0) }, _complex);
        }


        // メソッド

        /// <summary>
        /// 離散フーリエ変換
        /// </summary>
        /// <param name="result">周波数と強度のヒストグラム</param>
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
        /// 離散フーリエ変換
        /// </summary>
        /// <param name="feature">周波数ごとの強度の特徴量</param>
        public void Dft(out float[] feature)
        {
            Dft(out List<(double Frequency, float Value)> list);
            feature = new float[list.Count];
            for (int i = 0; i < feature.Length; i++)
                feature[i] = list[i].Value;
        }

        /// <summary>
        /// 逆変換
        /// </summary>
        /// <param name="result">時間と変位</param>
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
        /// 逆変換
        /// </summary>
        /// <param name="feature"></param>
        public void Idft(out float[] feature)
        {
            Idft(out List<(double Time, float Value)> list);
            feature = new float[list.Count];
            for (int i = 0; i < feature.Length; i++)
                feature[i] = list[i].Value;
        }

        /// <summary>
        /// バンドパスフィルタ
        /// </summary>
        /// <param name="minFrequency">最小周波数</param>
        /// <param name="maxFrequency">最大周波数</param>
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
