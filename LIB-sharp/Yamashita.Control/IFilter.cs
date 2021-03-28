namespace Yamashita.Control
{
    public interface IFilter
    {

        /// <summary>
        /// フィルタ適用.
        /// 観測値で更新→次期の予測を行う
        /// </summary>
        /// <param name="measurementVec">観測値のベクトル (m * 1)</param>
        /// <param name="controlVec">制御入力 (n * 1)</param>
        /// <returns>観測値の補正値および次期の推定値</returns>
        public (double[] Correct, double[] Predict) Update(double[] measurementVec, double[] controlVec = null);

    }
}
