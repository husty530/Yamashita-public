using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Yamashita.Filter
{
    public class ParticleFilter
    {

        private readonly int k;
        private readonly int N;
        private readonly Matrix<double> _measureNoise;
        private readonly Matrix<double> _processNoise;
        private List<Vector<double>> _particles;


        public ParticleFilter(double[] initialStateVec, double measurementNoise = 10, double processNoise = 10, int N = 10)
        {
            k = initialStateVec.Length;
            this.N = N;
            _measureNoise = DenseMatrix.OfArray(new double[k, k]);
            _processNoise = DenseMatrix.OfArray(new double[k, k]);
            _particles = new List<Vector<double>>();
            for (int i = 0; i < k; i++)
            {
                _measureNoise[i, i] = measurementNoise;
                _processNoise[i, i] = processNoise;
            }
            for (int i = 0; i < N; i++)
                _particles.Add(MakeVectorRandom(new DenseVector(initialStateVec)));
        }

        public double[] Update(double[] measurementStateVec)
        {
            var y = new DenseVector(measurementStateVec);
            var wList = new List<double>();
            foreach (var x in _particles) wList.Add(CalcLikelihood(x, y));
            var wSum = wList.Sum();
            var result = new double[k];
            for (int i = 0; i < N; i++)
            {
                wList[i] /= wSum;
                for (int j = 0; j < k; j++)
                    result[j] += _particles[i][j] * wList[i];
            }
            Resample(wList);
            return result;
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
            var err = y - x;
            var errT = err.ToRowMatrix();
            var index = 0.0;
            var indexVec = -errT * _measureNoise.Inverse() * err / 2;
            for (int i = 0; i < k; i++) index += x[i] * indexVec[0];
            var denominator = 2 * Math.PI * Math.Sqrt(_measureNoise.Determinant());
            return Math.Exp(index) / denominator;
        }

        private void Resample(List<double> likelihoodList)
        {
            var updated = new List<Vector<double>>();
            for (double d = 1.0 / N / 2; d < 1.0; d += 1.0 / N)
            {
                var sum = 0.0;
                for (int i = 0; i < likelihoodList.Count; i++)
                {
                    sum += likelihoodList[i];
                    if (sum > d)
                    {
                        updated.Add(MakeVectorRandom(_particles[i]));
                        break;
                    }
                }
            }
            _particles = updated;
        }

    }
}