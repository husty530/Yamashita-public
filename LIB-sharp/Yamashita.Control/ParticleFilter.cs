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

        // ------- Fields ------- //

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


        // ------- Properties ------- //

        public List<Vector<double>> Particles { private set; get; }


        // ------- Constructor ------- //

        /// <summary>
        /// The most simple.
        /// Use same Status and Observe parameter.
        /// You can't input Control.
        /// Noise covariance is default value.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="measurementNoise">R (default)</param>
        /// <param name="processNoise">Q (default)</param>
        /// <param name="N">Particles Count (default)</param>
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
        /// In the case of that Observe differ from Status.
        /// You can't input Control.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="transitionMatrix">A (k * k)</param>
        /// <param name="measurementMatrix">C (m * k)</param>
        /// <param name="measurementNoise">R (default)</param>
        /// <param name="processNoise">Q (default)</param>
        /// <param name="N">Particles Count (default)</param>
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
        /// You can design Noise Covariance Matrix.
        /// But can't input Control.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="transitionMatrix">A (k * k)</param>
        /// <param name="measurementMatrix">C (m * k)</param>
        /// <param name="measurementNoiseMatrix">R (m * m)</param>
        /// <param name="processNoiseMatrix">Q (k * k)</param>
        /// <param name="N">Particles Count (default)</param>
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
        /// The most simple.
        /// Use same Status and Observe parameter.
        /// You can input Control.
        /// Noise covariance is default value.
        /// </summary>        
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="controlMatrix">B (k * n)</param>
        /// <param name="measurementNoise">R (default)</param>
        /// <param name="processNoise">Q (default)</param>
        /// <param name="N">Particles Count (default)</param>
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
        /// In the case of that Observe differ from Status.
        /// You can input Control.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="controlMatrix">B (k * n)</param>
        /// <param name="transitionMatrix">A (k * k)</param>
        /// <param name="measurementMatrix">C (m * k)</param>
        /// <param name="measurementNoise">R (default)</param>
        /// <param name="processNoise">Q (default)</param>
        /// <param name="N">Particles Count(default)</param>
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
        /// You can design Noise Covariance Matrix.
        /// But can input Control.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="controlMatrix">B (k * n)</param>
        /// <param name="transitionMatrix">A (k * k)</param>
        /// <param name="measurementMatrix">C (m * k)</param>
        /// <param name="measurementNoiseMatrix">R (m * m)</param>
        /// <param name="processNoiseMatrix">Q (k * k)</param>
        /// <param name="N">Particles Count (default)</param>
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


        // ------- Methods ------- //

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

        private DenseVector MakeVectorRandom(Vector<double> vec)
        {
            var value = new double[k];
            for (int i = 0; i < k; i++)
                value[i] = Normal.Samples(vec[i], _processNoise[i, i]).Take(1).ToArray()[0];
            return new DenseVector(value);
        }

        private double CalcLikelihood(Vector<double> x, Vector<double> y)
        {
            var err = y - _measureMatrix * x;
            var errT = err.ToRowMatrix();
            var index = (-errT * _measureNoiseInv * err / 2)[0];
            return Math.Exp(index) / _denominator;
        }

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
