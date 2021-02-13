using System;
using OpenCvSharp;

namespace Yamashita.Features
{
    public class Hog
    {

        private readonly HOGDescriptor _hog;
        private readonly Size _imageSize;

        /// <summary>
        /// 簡易版HOG。分類には使えるが検出には使えない。
        /// </summary>
        /// <param name="imageSize">入力画像のサイズ</param>
        /// <param name="blockSize">正規化する単位ブロックのサイズ</param>
        /// <param name="blockStride">ブロックのスライド幅</param>
        /// <param name="cellSize">HOGを計算する1単位のサイズ</param>
        public Hog(Size? imageSize = null, Size? blockSize = null, Size? blockStride = null, Size? cellSize = null)
        {
            var s1 = (imageSize == null) ? new Size(64, 64) : imageSize;
            var s2 = (blockSize == null) ? new Size(16, 16) : blockSize;
            var s3 = (blockStride == null) ? new Size(8, 8) : blockStride;
            var s4 = (cellSize == null) ? new Size(8, 8) : cellSize;
            _hog = new HOGDescriptor(s1, s2, s3, s4);
            _imageSize = (Size)s1;
        }

        /// <summary>
        /// 1フレームに対する処理
        /// </summary>
        /// <param name="input">8bitグレースケール画像。勝手にリサイズされる</param>
        /// <param name="result">float配列の出力</param>
        public void Compute(Mat input, out float[] result)
        {
            if (input.Type() != MatType.CV_8U) new Exception("MatType should be 'CV_8U'.");
            Cv2.Resize(input, input, _imageSize);
            result = _hog.Compute(input);
        }
    }
}
