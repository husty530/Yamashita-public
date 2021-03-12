using OpenCvSharp;
using Intel.RealSense;

namespace Yamashita.DepthCamera
{
    public class RealsenseConverter
    {

        // フィールド

        private readonly int _width;
        private readonly int _height;


        // コンストラクタ

        public RealsenseConverter(int width, int height)
        {
            _width = width;
            _height = height;
        }


        // メソッド

        public void ToColorMat(VideoFrame frame, ref Mat colorMat)
        {
            colorMat = new Mat(_height, _width, MatType.CV_8UC3);
            unsafe
            {
                var rgbData = (byte*)frame.Data;
                var pixels = colorMat.DataPointer;
                for (int i = 0; i < colorMat.Width * colorMat.Height; i++)
                {
                    pixels[i * 3 + 0] = rgbData[i * 3 + 2];
                    pixels[i * 3 + 1] = rgbData[i * 3 + 1];
                    pixels[i * 3 + 2] = rgbData[i * 3 + 0];
                }
            }
            Cv2.Resize(colorMat, colorMat, new Size(_width / 2, _height / 2));
        }

        public void ToPointCloudMat(DepthFrame frame, ref Mat pointCloudMat)
        {
            if (pointCloudMat.Type() != MatType.CV_16UC3) pointCloudMat = new Mat(_height / 2, _width / 2, MatType.CV_16UC3);
            unsafe
            {
                var pData = (float*)(new PointCloud().Process(frame).Data);
                var pixels = (ushort*)pointCloudMat.Data;
                int index = 0;
                for (int i = 0; i < pointCloudMat.Width * pointCloudMat.Height; i++)
                {
                    pixels[index] = (ushort)(pData[index++] * 1000);
                    pixels[index] = (ushort)(pData[index++] * 1000);
                    pixels[index] = (ushort)(pData[index++] * 1000);
                }
            }
        }
    }
}
