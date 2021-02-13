namespace Yamashita.Svm
{

    public enum Mode { Train, Inference }

    public interface ISvm
    {
        // 学習フェーズ

        /// <summary>
        /// データを1つ追加
        /// </summary>
        /// <param name="feature">float配列にした特徴量ベクトル</param>
        /// <param name="label">0 or 1</param>
        public void AddData(float[] feature, int label);

        /// <summary>
        /// データの末尾を削除
        /// </summary>
        public void RemoveLastData();

        /// <summary>
        /// データセットをクリア
        /// </summary>
        public void ClearDataset();

        /// <summary>
        /// 明示的にデータセットをセーブ
        /// </summary>
        public void SaveDataset(bool append);

        /// <summary>
        /// 現在のデータセットで学習
        /// </summary>
        /// <param name="gamma">RBFのパラメータ</param>
        public void Train(double gamma = 0.1, bool append = true);


        // 推論フェーズ

        /// <summary>
        /// 推論処理
        /// </summary>
        /// <param name="input">float配列にした入力特徴量ベクトル</param>
        /// <param name="result">出力ラベル (0 or 1)</param>
        public void Predict(float[] input, out float result);
    }
}
