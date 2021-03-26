using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;

namespace Yamashita.DepthCamera
{
    public class KinectConverter
    {

        // フィールド

        private readonly int width;
        private readonly int height;


        // コンストラクタ

        public KinectConverter(Size size)
        {
            this.width = size.Width;
            this.height = size.Height;
        }


        // メソッド

        public unsafe void ToColorMat(Image colorImg, ref Mat colorMat)
        {
            if (colorMat.Type() != MatType.CV_8UC3) colorMat = new Mat(height, width, MatType.CV_8UC3);
            var cAry = colorImg.GetPixels<BGRA>().ToArray();
            var p = colorMat.DataPointer;
            int index = 0;
            for (int i = 0; i < cAry.Length; i++)
            {
                p[index++] = cAry[i].B;
                p[index++] = cAry[i].G;
                p[index++] = cAry[i].R;
            }
        }

        public unsafe void ToPointCloudMat(Image pointCloudImg, ref Mat pointCloudMat)
        {
            if (pointCloudMat.Type() != MatType.CV_16UC3) pointCloudMat = new Mat(height, width, MatType.CV_16UC3);
            var pdAry = pointCloudImg.GetPixels<Short3>().ToArray();
            var p = (ushort*)pointCloudMat.Data;
            int index = 0;
            for (int i = 0; i < pdAry.Length; i++)
            {
                p[index++] = (ushort)pdAry[i].X;
                p[index++] = (ushort)pdAry[i].Y;
                p[index++] = (ushort)pdAry[i].Z;
            }
        }

    }
}
