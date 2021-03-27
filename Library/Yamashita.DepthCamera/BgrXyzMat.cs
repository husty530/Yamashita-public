using System;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{

    /// <summary>
    /// 色付きの点群
    /// </summary>
    public class BgrXyzMat
    {

        // プロパティ

        /// <summary>
        /// カラー画像
        /// </summary>
        public Mat BGR { private set; get; }

        /// <summary>
        /// 点群画像
        /// </summary>
        public Mat XYZ { private set; get; }

        /// <summary>
        /// 距離画像(0～65535)(mm)
        /// </summary>
        public Mat Depth16 => XYZ.Split()[2];


        // コンストラクタ

        /// <summary>
        /// 色付きの点群を保持
        /// </summary>
        /// <param name="bgr">カラー画像</param>
        /// <param name="xyz">点群画像(カラーと同じサイズ)</param>
        public BgrXyzMat(Mat bgr, Mat xyz)
        {
            BGR = bgr;
            XYZ = xyz;
        }

        /// <summary>
        /// byte配列からデコードして復元
        /// </summary>
        /// <param name="BGRBytes"></param>
        /// <param name="XYZBytes"></param>
        public BgrXyzMat(byte[] BGRBytes, byte[] XYZBytes)
        {
            BGR = Cv2.ImDecode(BGRBytes, ImreadModes.Unchanged);
            XYZ = Cv2.ImDecode(XYZBytes, ImreadModes.Unchanged);
        }


        // メソッド

        /// <summary>
        /// コンストラクタと同義
        /// </summary>
        /// <param name="bgr">カラー画像</param>
        /// <param name="xyz">点群画像(カラーと同じサイズ)</param>
        /// <returns></returns>
        public static BgrXyzMat Create(Mat bgr, Mat xyz)
            => new BgrXyzMat(bgr, xyz);

        /// <summary>
        /// デコードしてbyteから復元
        /// </summary>
        /// <param name="BGRBytes"></param>
        /// <param name="XYZBytes"></param>
        /// <returns></returns>
        public static BgrXyzMat YmsDecode(byte[] BGRBytes, byte[] XYZBytes)
            => new BgrXyzMat(Cv2.ImDecode(BGRBytes, ImreadModes.Unchanged), Cv2.ImDecode(XYZBytes, ImreadModes.Unchanged));

        /// <summary>
        /// エンコードしてbyte出力
        /// </summary>
        /// <returns></returns>
        public (byte[] BGRBytes, byte[] XYZBytes) YmsEncode()
            => (BGR.ImEncode(), XYZ.ImEncode());

        /// <summary>
        /// 空確認
        /// </summary>
        /// <returns></returns>
        public bool Empty() => BGR.Empty();

        /// <summary>
        /// 距離画像(0～255に丸めたもの)を取得
        /// </summary>
        /// <param name="minDistance">最小値(mm)</param>
        /// <param name="maxDistance">最大値(mm)</param>
        /// <returns></returns>
        public unsafe Mat Depth8(int minDistance, int maxDistance)
        {
            var depth8 = new Mat(XYZ.Height, XYZ.Width, MatType.CV_8U);
            var d8 = depth8.DataPointer;
            var d16 = (ushort*)Depth16.Data;
            for (int j = 0; j < XYZ.Width * XYZ.Height; j++)
            {
                if (d16[j] < 300) d8[j] = 0;
                else if (d16[j] < maxDistance) d8[j] = (byte)((d16[j] - minDistance) * 255 / (maxDistance - minDistance));
                else d8[j] = 255;
            }
            return depth8;
        }

        /// <summary>
        /// 指定した点の情報を取得
        /// </summary>
        /// <param name="point">座標</param>
        /// <returns>色と実座標</returns>
        public unsafe BGRXYZ GetPointInfo(Point point)
        {
            var index = (point.Y * BGR.Cols + point.X) * 3;
            var bgr = BGR.DataPointer;
            var xyz = (short*)XYZ.Data;
            var b = bgr[index + 0];
            var g = bgr[index + 1];
            var r = bgr[index + 2];
            var x = xyz[index + 0];
            var y = xyz[index + 1];
            var z = xyz[index + 2];
            return new BGRXYZ(b, g, r, x, y, z);
        }

        /// <summary>
        /// 点群を3次元的に移動
        /// </summary>
        /// <param name="delta">移動量のベクトル</param>
        public unsafe BgrXyzMat Move(Vector3 delta)
        {
            var s = (short*)XYZ.Data;
            var index = 0;
            for (int i = 0; i < XYZ.Rows * XYZ.Cols; i++)
            {
                s[index++] += delta.X;
                s[index++] += delta.Y;
                s[index++] += delta.Z;
            }
            return this;
        }

        /// <summary>
        /// 点群の大きさを変換
        /// </summary>
        /// <param name="delta">XYZ方向のスケール</param>
        public unsafe BgrXyzMat Scale(Vector3 delta)
        {
            var s = (short*)XYZ.Data;
            var index = 0;
            for (int i = 0; i < XYZ.Rows * XYZ.Cols; i++)
            {
                s[index++] *= delta.X;
                s[index++] *= delta.Y;
                s[index++] *= delta.Z;
            }
            return this;
        }

        /// <summary>
        /// 右手系の3次元回転
        /// </summary>
        /// <param name="pitch">ピッチ角(X座標中心時計回り)</param>
        /// <param name="yaw">ヨー角(Y座標中心時計回り)</param>
        /// <param name="roll">ロール角(Z座標中心時計回り)</param>
        public unsafe BgrXyzMat Rotate(float pitch, float yaw, float roll)
        {
            Mat rot = ZRot(roll) * YRot(yaw) * XRot(pitch);
            var d = (float*)rot.Data;
            var s = (short*)XYZ.Data;
            for (int i = 0; i < XYZ.Rows * XYZ.Cols * 3; i += 3)
            {
                var x = s[i + 0];
                var y = s[i + 1];
                var z = s[i + 2];
                s[i + 0] = (short)(d[0] * x + d[1] * y + d[2] * z);
                s[i + 1] = (short)(d[3] * x + d[4] * y + d[5] * z);
                s[i + 2] = (short)(d[6] * x + d[7] * y + d[8] * z);
            }
            return this;
        }


        private Mat XRot(float rad)
            => new Mat(3, 3, MatType.CV_32F, new float[] { 1, 0, 0, 0, (float)Math.Cos(rad), -(float)Math.Sin(rad), 0, (float)Math.Sin(rad), (float)Math.Cos(rad) });

        private Mat YRot(float rad)
            => new Mat(3, 3, MatType.CV_32F, new float[] { (float)Math.Cos(rad), 0, (float)Math.Sin(rad), 0, 1, 0, -(float)Math.Sin(rad), 0, (float)Math.Cos(rad) });

        private Mat ZRot(float rad)
            => new Mat(3, 3, MatType.CV_32F, new float[] { (float)Math.Cos(rad), -(float)Math.Sin(rad), 0, (float)Math.Sin(rad), (float)Math.Cos(rad), 0, 0, 0, 1 });

    }


    /// <summary>
    /// 色付きの点
    /// </summary>
    public struct BGRXYZ
    {

        public BGRXYZ(byte b, byte g, byte r, short x, short y, short z)
        {
            B = b;
            G = g;
            R = r;
            X = x;
            Y = y;
            Z = z;
        }

        public byte B { private set; get; }

        public byte G { private set; get; }

        public byte R { private set; get; }

        public short X { private set; get; }

        public short Y { private set; get; }

        public short Z { private set; get; }

    }

}
