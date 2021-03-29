namespace Yamashita.Control
{
    /// <summary>
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
    public interface IFilter
    {

        /// <summary>
        /// Apply Filter.
        /// Using Observe, Predict Next State.
        /// </summary>
        /// <param name="measurementVec">Y (m * 1)</param>
        /// <param name="controlVec">U (n * 1)</param>
        /// <returns></returns>
        public (double[] Correct, double[] Predict) Update(double[] measurementVec, double[] controlVec = null);

    }
}
