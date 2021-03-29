using OpenCvSharp;

namespace Yamashita.Control
{
    public class KalmanFilter : IFilter
    {

        // ------- Fields ------- //

        private readonly MatType type = MatType.CV_64F;
        private readonly OpenCvSharp.KalmanFilter _kalman;
        private readonly int k;
        private readonly int m;
        private readonly int n;


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
        /// <param name="preError">P (default)</param>
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
        /// In the case of that Observe differ from Status.
        /// You can't input Control.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="transitionMatrix">A (k * k)</param>
        /// <param name="measurementMatrix">C (m * k)</param>
        /// <param name="measurementNoise">R (default)</param>
        /// <param name="processNoise">Q (default)</param>
        /// <param name="preError">P (default)</param>
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
        /// You can design Noise Covariance Matrix.
        /// But can't input Control.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="transitionMatrix">A (k * k)</param>
        /// <param name="measurementMatrix">C (m * k)</param>
        /// <param name="measurementNoiseMatrix">R (m * m)</param>
        /// <param name="processNoiseMatrix">Q (k * k)</param>
        /// <param name="preErrorMatrix">P (k * k)</param>
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
        /// The most simple.
        /// Use same Status and Observe parameter.
        /// You can't input Control.
        /// Noise covariance is default value.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="controlMatrix">B (k * n)</param>
        /// <param name="measurementNoise">R (default)</param>
        /// <param name="processNoise">Q (default)</param>
        /// <param name="preError">P (default)</param>
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
        /// In the case of that Observe differ from Status.
        /// You can't input Control.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="controlMatrix">B (k * n)</param>
        /// <param name="transitionMatrix">A (k * k)</param>
        /// <param name="measurementMatrix">C (m * k)</param>
        /// <param name="measurementNoise">R (default)</param>
        /// <param name="processNoise">Q (default)</param>
        /// <param name="preError">P (default)</param>
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
        /// You can design Noise Covariance Matrix.
        /// But can't input Control.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="controlMatrix">B (k * n)</param>
        /// <param name="transitionMatrix">A (k * k)</param>
        /// <param name="measurementMatrix">C (m * k)</param>
        /// <param name="measurementNoiseMatrix">R (m * m)</param>
        /// <param name="processNoiseMatrix">Q (k * k)</param>
        /// <param name="preErrorMatrix">P (k * k)</param>
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


        // ------- Methods ------- //

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
