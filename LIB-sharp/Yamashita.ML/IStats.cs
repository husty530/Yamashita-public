using System.Collections.Generic;

namespace Yamashita.ML
{
    public enum Mode { Train, Inference }

    public interface IStats
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
        /// <param name="append">もとのモデルを追加学習するか</param>
        /// <param name="param">各モデル用のパラメータ</param>
        public void Train(bool append, double? param);


        // 推論フェーズ

        /// <summary>
        /// 推論処理
        /// </summary>
        /// <param name="input">float配列にした入力特徴量ベクトルの配列</param>
        /// <param name="output">出力ラベル (0 or 1) の配列</param>
        public void Predict(List<float[]> input, out List<float> output);
    }
}
