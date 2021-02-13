namespace Yamashita.Filter
{
    public interface IFilter
    {
        /// <summary>
        /// フィルタ適用
        /// ループの中でこれを呼び出すと観測値で更新→次期の予測を行う
        /// 戻るのは参照で入ってきた観測の補正値
        /// </summary>
        /// <param name="measurementVec">観測値のベクトル (m * 1)</param>
        /// <param name="controlVec">制御入力 (n * 1)</param>
        /// <returns>観測値の補正値および次期の推定値</returns>
        public (double[] Correct, double[] Predict) Update(double[] measurementVec, double[] controlVec = null);

    }
}