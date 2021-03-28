﻿using OpenCvSharp;

namespace Yamashita.Control
{
    /// <summary>
    /// 
    /// カルマンフィルタの実装
    /// 
    /// 状態数を"k", 観測数を"m", 制御入力数を"n"とする
    /// 
    /// 以下、引数でMatrixなのに配列型になっているところがある
    /// 行列で M = A B C
    ///            D E F
    ///            G H I
    /// と書きたい場合は、
    /// m = { a, b, c, d, e, f, g, h, i }
    /// のように一列の配列としてMatrixのデータを用意する
    /// 
    /// 対角行列ですべて一定値でよい場合のために簡易版のコンストラクタもある
    /// 
    /// </summary>
    public class KalmanFilter : IFilter
    {

        // フィールド

        private readonly MatType type = MatType.CV_64F;
        private readonly OpenCvSharp.KalmanFilter _kalman;
        private readonly int k;
        private readonly int m;
        private readonly int n;


        // コンストラクタ

        /// <summary>
        /// 一番シンプルなもの
        /// 出力と内部状態、観測パラメータがすべて同じ場合に使用可能
        /// 制御入力はできない
        /// 与える誤差分散の値はデフォルト引数にしている
        /// </summary>
        /// <param name="initialStateVec">X : 初期状態ベクトル (k * 1)</param>
        /// <param name="measurementNoise">R : 観測ノイズの共分散行列 (デフォルト値)</param>
        /// <param name="processNoise">Q : プロセスノイズの共分散行列。被観測側の不正確さ (デフォルト値)</param>
        /// <param name="preError">P : 誤差分散行列の初期値 (デフォルト値)</param>
        public KalmanFilter(double[] initialStateVec, double measurementNoise = 0.01, double processNoise = 0.01, double preError = 1.0)
        {

            k = initialStateVec.Length;
            m = initialStateVec.Length;
            _kalman = new OpenCvSharp.KalmanFilter(k, m, 0, type)
            {
                StatePre = new Mat(k, 1, type, initialStateVec),
                StatePost = new Mat(k, 1, type, initialStateVec),
                TransitionMatrix = Mat.Zeros(new Size(k, k), type),
                MeasurementMatrix = Mat.Zeros(new Size(m, k), type),
                MeasurementNoiseCov = Mat.Zeros(new Size(m, m), type),
                ProcessNoiseCov = Mat.Zeros(new Size(k, k), type),
                ErrorCovPre = Mat.Zeros(new Size(k, k), type)
            };
            for (int i = 0; i < k; i++)
            {
                _kalman.TransitionMatrix.At<double>(i, i) = 1;
                _kalman.MeasurementMatrix.At<double>(i, i) = 1;
                _kalman.MeasurementNoiseCov.At<double>(i, i) = measurementNoise;
                _kalman.ProcessNoiseCov.At<double>(i, i) = processNoise;
                _kalman.ErrorCovPre.At<double>(i, i) = preError;
            }

        }


        /// <summary>
        /// 出力と内部状態、観測のパラメータが異なる場合
        /// 制御入力はできない
        /// </summary>
        /// <param name="initialStateVec">X : 初期状態ベクトル (k * 1)</param>
        /// <param name="transitionMatrix">A : 状態の遷移行列 (k * k)</param>
        /// <param name="measurementMatrix">C : 観測値の遷移行列 (m * k)</param>
        /// <param name="measurementNoise">R : 観測ノイズの共分散行列。観測側の不正確さ (デフォルト値)</param>
        /// <param name="processNoise">Q : プロセスノイズの共分散行列。被観測側の不正確さ (デフォルト値)</param>
        /// <param name="preError">P : 誤差分散行列の初期値 (デフォルト値)</param>
        public KalmanFilter(double[] initialStateVec, double[] transitionMatrix, double[] measurementMatrix, double measurementNoise = 0.01, double processNoise = 0.01, double preError = 1.0)
        {

            k = initialStateVec.Length;
            m = measurementMatrix.Length / k;
            _kalman = new OpenCvSharp.KalmanFilter(k, m, 0, type)
            {
                StatePre = new Mat(k, 1, type, initialStateVec),
                StatePost = new Mat(k, 1, type, initialStateVec),
                TransitionMatrix = new Mat(k, k, type, transitionMatrix),
                MeasurementMatrix = new Mat(m, k, type, measurementMatrix),
                MeasurementNoiseCov = Mat.Zeros(new Size(m, m), type),
                ProcessNoiseCov = Mat.Zeros(new Size(k, k), type),
                ErrorCovPre = Mat.Zeros(new Size(k, k), type)
            };
            for (int i = 0; i < m; i++) _kalman.MeasurementNoiseCov.At<double>(i, i) = measurementNoise;
            for (int i = 0; i < k; i++)
            {
                _kalman.ProcessNoiseCov.At<double>(i, i) = processNoise;
                _kalman.ErrorCovPre.At<double>(i, i) = preError;
            }

        }

        /// <summary>
        /// 誤差の共分散行列を指定したい場合
        /// 制御入力はできない
        /// </summary>
        /// <param name="initialStateVec">X : 初期状態ベクトル (k * 1)</param>
        /// <param name="transitionMatrix">A : 状態の遷移行列 (k * k)</param>
        /// <param name="measurementMatrix">C : 観測値の遷移行列 (m * k)</param>
        /// <param name="measurementNoiseMatrix">R : 観測ノイズの共分散行列。観測側の不正確さ (m * m)</param>
        /// <param name="processNoiseMatrix">Q : プロセスノイズの共分散行列。被観測側の不正確さ (k * k)</param>
        /// <param name="preErrorMatrix">P : 誤差分散行列の初期値。誤差が独立なら対角行列になる (k * k)</param>
        public KalmanFilter(double[] initialStateVec, double[] transitionMatrix, double[] measurementMatrix, double[] measurementNoiseMatrix, double[] processNoiseMatrix, double[] preErrorMatrix)
        {

            k = initialStateVec.Length;
            m = measurementMatrix.Length / k;
            _kalman = new OpenCvSharp.KalmanFilter(k, m, 0, type)
            {
                StatePre = new Mat(k, 1, type, initialStateVec),
                StatePost = new Mat(k, 1, type, initialStateVec),
                TransitionMatrix = new Mat(k, k, type, transitionMatrix),
                MeasurementMatrix = new Mat(m, k, type, measurementMatrix),
                MeasurementNoiseCov = new Mat(m, m, type, measurementNoiseMatrix),
                ProcessNoiseCov = new Mat(k, k, type, processNoiseMatrix),
                ErrorCovPre = new Mat(k, k, type, preErrorMatrix)
            };

        }

        /// <summary>
        /// 一番シンプルなもの
        /// 出力と内部状態、観測パラメータがすべて同じ場合に使用可能
        /// 制御入力できる
        /// 与える誤差分散の値はデフォルト引数にしている
        /// </summary>        
        /// <param name="initialStateVec">X : 初期状態ベクトル (k * 1)</param>
        /// <param name="controlMatrix">B : 制御入力の遷移行列 (k * n)</param>
        /// <param name="measurementNoise">R : 観測ノイズの共分散行列。観測側の不正確さ (デフォルト値)</param>
        /// <param name="processNoise">Q : プロセスノイズの共分散行列。被観測側の不正確さ (デフォルト値)</param>
        /// <param name="preError">P : 誤差分散行列の初期値 (デフォルト値)</param>
        public KalmanFilter(double[] initialStateVec, double[] controlMatrix, double measurementNoise = 0.01, double processNoise = 0.01, double preError = 1.0)
        {

            k = initialStateVec.Length;
            m = initialStateVec.Length;
            n = controlMatrix.Length / k;
            _kalman = new OpenCvSharp.KalmanFilter(k, m, n, type)
            {
                StatePre = new Mat(k, 1, type, initialStateVec),
                StatePost = new Mat(k, 1, type, initialStateVec),
                ControlMatrix = new Mat(k, n, type, controlMatrix),
                TransitionMatrix = Mat.Zeros(new Size(k, k), type),
                MeasurementMatrix = Mat.Zeros(new Size(m, k), type),
                MeasurementNoiseCov = Mat.Zeros(new Size(m, m), type),
                ProcessNoiseCov = Mat.Zeros(new Size(k, k), type),
                ErrorCovPre = Mat.Zeros(new Size(k, k), type)
            };
            for (int i = 0; i < k; i++)
            {
                _kalman.TransitionMatrix.At<double>(i, i) = 1;
                _kalman.MeasurementMatrix.At<double>(i, i) = 1;
                _kalman.MeasurementNoiseCov.At<double>(i, i) = measurementNoise;
                _kalman.ProcessNoiseCov.At<double>(i, i) = processNoise;
                _kalman.ErrorCovPre.At<double>(i, i) = preError;
            }

        }

        /// <summary>
        /// 出力と内部状態、観測のパラメータが異なる場合
        /// 制御入力できる
        /// </summary>
        /// <param name="initialStateVec">X : 初期状態ベクトル (k * 1)</param>
        /// <param name="controlMatrix">B : 制御入力の遷移行列 (k * n)</param>
        /// <param name="transitionMatrix">A : 状態の遷移行列 (k * k)</param>
        /// <param name="measurementMatrix">C : 観測値の遷移行列 (m * k)</param>
        /// <param name="measurementNoise">R : 観測ノイズの共分散行列。観測側の不正確さ (デフォルト値)</param>
        /// <param name="processNoise">Q : プロセスノイズの共分散行列。被観測側の不正確さ (デフォルト値)</param>
        /// <param name="preError">P : 誤差分散行列の初期値 (デフォルト値)</param>
        public KalmanFilter(double[] initialStateVec, double[] controlMatrix, double[] transitionMatrix, double[] measurementMatrix, double measurementNoise = 0.01, double processNoise = 0.01, double preError = 1.0)
        {

            k = initialStateVec.Length;
            m = measurementMatrix.Length / k;
            n = controlMatrix.Length / k;
            _kalman = new OpenCvSharp.KalmanFilter(k, m, n, type)
            {
                StatePre = new Mat(k, 1, type, initialStateVec),
                StatePost = new Mat(k, 1, type, initialStateVec),
                ControlMatrix = new Mat(k, n, type, controlMatrix),
                TransitionMatrix = new Mat(k, k, type, transitionMatrix),
                MeasurementMatrix = new Mat(m, k, type, measurementMatrix),
                MeasurementNoiseCov = Mat.Zeros(new Size(m, m), type)
            };
            for (int i = 0; i < m; i++) _kalman.MeasurementNoiseCov.At<double>(i, i) = measurementNoise;
            _kalman.ProcessNoiseCov = Mat.Zeros(new Size(k, k), type);
            _kalman.ErrorCovPre = Mat.Zeros(new Size(k, k), type);
            for (int i = 0; i < k; i++)
            {
                _kalman.ProcessNoiseCov.At<double>(i, i) = processNoise;
                _kalman.ErrorCovPre.At<double>(i, i) = preError;
            }

        }

        /// <summary>
        /// 誤差の共分散行列を指定したい場合
        /// 制御入力できる
        /// </summary>
        /// <param name="initialStateVec">X : 初期状態ベクトル (k * 1)</param>
        /// <param name="controlMatrix">B : 制御入力の遷移行列 (k * n)</param>
        /// <param name="transitionMatrix">A : 状態の遷移行列 (k * k)</param>
        /// <param name="measurementMatrix">C : 観測値の遷移行列 (m * k)</param>
        /// <param name="measurementNoiseMatrix">R : 観測ノイズの共分散行列。観測側の不正確さ (m * m)</param>
        /// <param name="processNoiseMatrix">Q : プロセスノイズの共分散行列。被観測側の不正確さ (k * k)</param>
        /// <param name="preErrorMatrix">P : 誤差分散行列の初期値。誤差が独立なら対角行列になる (k * k)</param>
        public KalmanFilter(double[] initialStateVec, double[] controlMatrix, double[] transitionMatrix, double[] measurementMatrix, double[] measurementNoiseMatrix, double[] processNoiseMatrix, double[] preErrorMatrix)
        {

            k = initialStateVec.Length;
            m = measurementMatrix.Length / k;
            n = controlMatrix.Length / k;
            _kalman = new OpenCvSharp.KalmanFilter(k, m, n, type)
            {
                StatePre = new Mat(k, 1, type, initialStateVec),
                StatePost = new Mat(k, 1, type, initialStateVec),
                ControlMatrix = new Mat(k, n, type, controlMatrix),
                TransitionMatrix = new Mat(k, k, type, transitionMatrix),
                MeasurementMatrix = new Mat(m, k, type, measurementMatrix),
                MeasurementNoiseCov = new Mat(m, m, type, measurementNoiseMatrix),
                ProcessNoiseCov = new Mat(k, k, type, processNoiseMatrix),
                ErrorCovPre = new Mat(k, k, type, preErrorMatrix)
            };

        }


        // メソッド

        public (double[] Correct, double[] Predict) Update(double[] measurementVec, double[] controlVec = null)
        {
            using var correctMat = _kalman.Correct(new Mat(m, 1, type, measurementVec));
            using var controlMat = controlVec != null ? new Mat(n, 1, type, controlVec) : null;
            using var predictMat = _kalman.Predict(controlMat);
            var correctArray = new double[k];
            var predictArray = new double[k];
            for (int i = 0; i < k; i++)
            {
                correctArray[i] = correctMat.At<double>(i);
                predictArray[i] = predictMat.At<double>(i);
            }
            return (correctArray, predictArray);
        }

    }
}