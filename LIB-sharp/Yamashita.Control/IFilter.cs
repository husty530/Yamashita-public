﻿namespace Yamashita.Control
{
    public interface IFilter
    {

        // k ... State Vector Length
        // m ... Measurement Vector Length
        // n ... Control Vector Length
        // 
        // If you describe Matrix like,
        // 
        //     M = A B C
        //         D E F
        //         G H I
        //         
        // you should write code like this,
        // 
        // m = { a, b, c, d, e, f, g, h, i }
        // 
        // Simple version constructor is available if you need.

        /// <summary>
        /// Apply filter.
        /// Predict next state from observed input.
        /// </summary>
        /// <param name="measurementVec">Y (m * 1)</param>
        /// <param name="controlVec">U (n * 1)</param>
        /// <returns>Results as same type of input</returns>
        public (double[] Correct, double[] Predict) Update(double[] measurementVec, double[] controlVec = null);

    }
}
