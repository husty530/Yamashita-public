using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Yamashita.Control
{
    public class ParticleFilter : IFilter
    {

        // フィールド

        private readonly int k;
        private readonly int m;
        private readonly int n;
        private readonly int N;
        private readonly Matrix<double> _controlMatrix;
        private readonly Matrix<double> _transitionMatrix;
        private readonly Matrix<double> _measureMatrix;
        private readonly Matrix<double> _processNoise;
        private readonly Matrix<double> _measureNoiseInv;
        private readonly double _denominator;


        // プロパティ

        public List<Vector<double>> Particles { private set; get; }


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
        /// <param name="N">粒子数 (デフォルト値)</param>
        public ParticleFilter(double[] initialStateVec, double measurementNoise = 3, double processNoise = 3, int N = 100)
        {
            k = initialStateVec.Length;
            m = initialStateVec.Length;
            this.N = N;
            _measureMatrix = DenseMatrix.OfArray(new double[m, k]);
            _transitionMatrix = DenseMatrix.OfArray(new double[k, k]);
            var measureNoise = DenseMatrix.OfArray(new double[m, m]);
            _processNoise = DenseMatrix.OfArray(new double[k, k]);
            Particles = new List<Vector<double>>();
            for (int i = 0; i < k; i++)
            {
                _measureMatrix[i, i] = 1;
                _transitionMatrix[i, i] = 1;
                measureNoise[i, i] = measurementNoise;
                _processNoise[i, i] = processNoise;
            }
            for (int i = 0; i < N; i++)
                Particles.Add(MakeVectorRandom(new DenseVector(initialStateVec)));
            _denominator = Math.Sqrt(Math.Pow(2 * Math.PI, k) * measureNoise.Determinant());
            _measureNoiseInv = measureNoise.Inverse();
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
        /// <param name="N">粒子数 (デフォルト値)</param>
        public ParticleFilter(double[] initialStateVec, double[] transitionMatrix, double[] measurementMatrix, double measurementNoise = 3, double processNoise = 3, int N = 100)
        {
            k = initialStateVec.Length;
            m = measurementMatrix.Length / k;
            this.N = N;
            _measureMatrix = new DenseMatrix(k, m, measurementMatrix).Transpose();
            _transitionMatrix = new DenseMatrix(k, k, transitionMatrix).Transpose();
            var measureNoise = DenseMatrix.OfArray(new double[m, m]);
            _processNoise = DenseMatrix.OfArray(new double[k, k]);
            Particles = new List<Vector<double>>();
            for (int i = 0; i < k; i++)
                _processNoise[i, i] = processNoise;
            for (int i = 0; i < m; i++)
                measureNoise[i, i] = measurementNoise;
            for (int i = 0; i < N; i++)
                Particles.Add(MakeVectorRandom(new DenseVector(initialStateVec)));
            _denominator = Math.Sqrt(Math.Pow(2 * Math.PI, k) * measureNoise.Determinant());
            _measureNoiseInv = measureNoise.Inverse();
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
        /// <param name="N">粒子数 (デフォルト値)</param>
        public ParticleFilter(double[] initialStateVec, double[] transitionMatrix, double[] measurementMatrix, double[] measurementNoiseMatrix, double[] processNoiseMatrix, int N = 100)
        {
            k = initialStateVec.Length;
            m = measurementMatrix.Length / k;
            this.N = N;
            _measureMatrix = new DenseMatrix(k, m, measurementMatrix).Transpose();
            _transitionMatrix = new DenseMatrix(k, k, transitionMatrix).Transpose();
            var measureNoise = new DenseMatrix(m, m, measurementNoiseMatrix).Transpose();
            _processNoise = new DenseMatrix(k, k, processNoiseMatrix).Transpose();
            Particles = new List<Vector<double>>();
            for (int i = 0; i < N; i++)
                Particles.Add(MakeVectorRandom(new DenseVector(initialStateVec)));
            _denominator = Math.Sqrt(Math.Pow(2 * Math.PI, k) * measureNoise.Determinant());
            _measureNoiseInv = measureNoise.Inverse();
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
        /// <param name="N">粒子数 (デフォルト値)</param>
        public ParticleFilter(double[] initialStateVec, double[] controlMatrix, double measurementNoise = 0.01, double processNoise = 0.01, int N = 100)
        {

            k = initialStateVec.Length;
            m = initialStateVec.Length;
            n = controlMatrix.Length / k;
            this.N = N;
            _controlMatrix = new DenseMatrix(n, k, controlMatrix).Transpose();
            _measureMatrix = DenseMatrix.OfArray(new double[m, k]);
            _transitionMatrix = DenseMatrix.OfArray(new double[k, k]);
            var measureNoise = DenseMatrix.OfArray(new double[m, m]);
            _processNoise = DenseMatrix.OfArray(new double[k, k]);
            Particles = new List<Vector<double>>();
            for (int i = 0; i < k; i++)
            {
                _measureMatrix[i, i] = 1;
                _transitionMatrix[i, i] = 1;
                measureNoise[i, i] = measurementNoise;
                _processNoise[i, i] = processNoise;
            }
            for (int i = 0; i < N; i++)
                Particles.Add(MakeVectorRandom(new DenseVector(initialStateVec)));
            _denominator = Math.Sqrt(Math.Pow(2 * Math.PI, k) * measureNoise.Determinant());
            _measureNoiseInv = measureNoise.Inverse();
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
        /// <param name="N">粒子数 (デフォルト値)</param>
        public ParticleFilter(double[] initialStateVec, double[] controlMatrix, double[] transitionMatrix, double[] measurementMatrix, double measurementNoise = 0.01, double processNoise = 0.01, int N = 100)
        {
            k = initialStateVec.Length;
            m = measurementMatrix.Length / k;
            n = controlMatrix.Length / k;
            this.N = N;
            _controlMatrix = new DenseMatrix(n, k, controlMatrix).Transpose();
            _measureMatrix = new DenseMatrix(k, m, measurementMatrix).Transpose();
            _transitionMatrix = new DenseMatrix(k, k, transitionMatrix).Transpose();
            var measureNoise = DenseMatrix.OfArray(new double[m, m]);
            _processNoise = DenseMatrix.OfArray(new double[k, k]);
            Particles = new List<Vector<double>>();
            for (int i = 0; i < k; i++)
                _processNoise[i, i] = processNoise;
            for (int i = 0; i < m; i++)
                measureNoise[i, i] = measurementNoise;
            for (int i = 0; i < N; i++)
                Particles.Add(MakeVectorRandom(new DenseVector(initialStateVec)));
            _denominator = Math.Sqrt(Math.Pow(2 * Math.PI, k) * measureNoise.Determinant());
            _measureNoiseInv = measureNoise.Inverse();

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
        /// <param name="N">粒子数 (デフォルト値)</param>
        public ParticleFilter(double[] initialStateVec, double[] controlMatrix, double[] transitionMatrix, double[] measurementMatrix, double[] measurementNoiseMatrix, double[] processNoiseMatrix, int N = 100)
        {
            k = initialStateVec.Length;
            m = measurementMatrix.Length / k;
            n = controlMatrix.Length / k;
            this.N = N;
            _controlMatrix = new DenseMatrix(n, k, controlMatrix).Transpose();
            _measureMatrix = new DenseMatrix(k, m, measurementMatrix).Transpose();
            _transitionMatrix = new DenseMatrix(k, k, transitionMatrix).Transpose();
            var measureNoise = new DenseMatrix(m, m, measurementNoiseMatrix).Transpose();
            _processNoise = new DenseMatrix(k, k, processNoiseMatrix).Transpose();
            Particles = new List<Vector<double>>();
            for (int i = 0; i < N; i++)
                Particles.Add(MakeVectorRandom(new DenseVector(initialStateVec)));
            _denominator = Math.Sqrt(Math.Pow(2 * Math.PI, k) * measureNoise.Determinant());
            _measureNoiseInv = measureNoise.Inverse();

        }


        // メソッド

        public (double[] Correct, double[] Predict) Update(double[] measurementVec, double[] controlVec = null)
        {
            var y = new DenseVector(measurementVec);
            var wList = new List<double>();
            foreach (var x in Particles)
                wList.Add(CalcLikelihood(x, y));
            var wSum = wList.Sum();
            var correct = new double[k];
            for (int i = 0; i < N; i++)
            {
                wList[i] /= wSum;
                for (int j = 0; j < k; j++)
                    correct[j] += Particles[i][j] * wList[i];
            }
            Particles = Resample(wList).ToList();
            Particles = PredictNextState(controlVec).ToList();
            var predict = new double[k];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < k; j++)
                    predict[j] += Particles[i][j] * wList[i];
            return (correct, predict);
        }

        // ベクトルにプロセスノイズを加えて返す
        private DenseVector MakeVectorRandom(Vector<double> vec)
        {
            var value = new double[k];
            for (int i = 0; i < k; i++)
                value[i] = Normal.Samples(vec[i], _processNoise[i, i]).Take(1).ToArray()[0];
            return new DenseVector(value);
        }

        // 尤度の計算
        private double CalcLikelihood(Vector<double> x, Vector<double> y)
        {
            var err = y - _measureMatrix * x;
            var errT = err.ToRowMatrix();
            var index = (-errT * _measureNoiseInv * err / 2)[0];
            return Math.Exp(index) / _denominator;
        }

        // 系統サンプリング
        private IEnumerable<Vector<double>> Resample(List<double> likelihoodList)
        {
            for (double d = 1.0 / N / 2; d < 1.0; d += 1.0 / N)
            {
                var sum = 0.0;
                for (int i = 0; i < N; i++)
                {
                    sum += likelihoodList[i];
                    if (sum > d)
                    {
                        yield return Particles[i];
                        break;
                    }
                }
            }
        }

        // 次期の予測
        private IEnumerable<Vector<double>> PredictNextState(double[] controlVec)
        {
            if (_controlMatrix == null || controlVec == null)
                foreach (var p in Particles)
                    yield return MakeVectorRandom(_transitionMatrix * p);
            else
                foreach (var p in Particles)
                    yield return MakeVectorRandom(_transitionMatrix * p + _controlMatrix * new DenseVector(controlVec));
        }

    }
}
