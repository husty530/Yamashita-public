using System;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{

    /// <summary>
    /// Point Cloud with Color
    /// </summary>
    public class BgrXyzMat
    {

        // ------- Properties ------- //

        /// <summary>
        /// Color Image
        /// </summary>
        public Mat BGR { private set; get; }

        /// <summary>
        /// Point Cloud Image
        /// </summary>
        public Mat XYZ { private set; get; }

        /// <summary>
        /// Depth Image (0～65535)(mm)
        /// </summary>
        public Mat Depth16 => XYZ.Split()[2];


        // ------- Constructor ------- //

        /// <summary>
        /// Hold Point Cloud with Color.
        /// </summary>
        /// <param name="bgr">Color Image</param>
        /// <param name="xyz">Point Cloud Image (must be same size of Color Image)</param>
        public BgrXyzMat(Mat bgr, Mat xyz)
        {
            BGR = bgr;
            XYZ = xyz;
        }

        /// <summary>
        /// Decode from Byte Array.
        /// </summary>
        /// <param name="BGRBytes"></param>
        /// <param name="XYZBytes"></param>
        public BgrXyzMat(byte[] BGRBytes, byte[] XYZBytes)
        {
            BGR = Cv2.ImDecode(BGRBytes, ImreadModes.Unchanged);
            XYZ = Cv2.ImDecode(XYZBytes, ImreadModes.Unchanged);
        }


        // ------- Methods ------- //

        /// <summary>
        /// Same function as Constructor.
        /// </summary>
        /// <param name="bgr">Color Image</param>
        /// <param name="xyz">Point Cloud Image (must be same size of Color Image)</param>
        /// <returns></returns>
        public static BgrXyzMat Create(Mat bgr, Mat xyz)
            => new BgrXyzMat(bgr, xyz);

        /// <summary>
        /// Decode from Byte Array.
        /// </summary>
        /// <param name="BGRBytes"></param>
        /// <param name="XYZBytes"></param>
        /// <returns></returns>
        public static BgrXyzMat YmsDecode(byte[] BGRBytes, byte[] XYZBytes)
            => new BgrXyzMat(Cv2.ImDecode(BGRBytes, ImreadModes.Unchanged), Cv2.ImDecode(XYZBytes, ImreadModes.Unchanged));

        /// <summary>
        /// Output Encoded Byte Array.
        /// </summary>
        /// <returns></returns>
        public (byte[] BGRBytes, byte[] XYZBytes) YmsEncode()
            => (BGR.ImEncode(), XYZ.ImEncode());

        /// <summary>
        /// Check if it's Empty.
        /// </summary>
        /// <returns></returns>
        public bool Empty() => BGR.Empty();

        /// <summary>
        /// Get Depth Image (Normalize value in 0-255)
        /// </summary>
        /// <param name="minDistance">(mm)</param>
        /// <param name="maxDistance">(mm)</param>
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
        /// Get Information of where you input.
        /// </summary>
        /// <param name="point">Pixel Coordinate</param>
        /// <returns>Real 3D Coordinate with Color</returns>
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
        /// Move All Point Cloud.
        /// </summary>
        /// <param name="delta">3D Vector of Transform (mm)</param>
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
        /// Change Point Cloud Scale.
        /// </summary>
        /// <param name="delta">Scale of XYZ Direction</param>
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
        /// Rotate 3D of Right Hand System.
        /// </summary>
        /// <param name="pitch">Pitch Angle (rad, Clockwise of X axis)</param>
        /// <param name="yaw">Yaw Angle (rad, Clockwise of Y axis)</param>
        /// <param name="roll">Roll Angle (rad, Clockwise of Z axis)</param>
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
    /// Struct of Point and Color
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
