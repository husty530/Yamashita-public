using Intel.RealSense;
using OpenCvSharp;

namespace Yamashita.Realsense
{
    public class Converter
    {
        private readonly int _width;
        private readonly int _height;

        public Converter(int width, int height)
        {
            _width = width;
            _height = height;
        }

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

        public void ToDepthMat(DepthFrame frame, ref Mat depthMat)
        {
            if (depthMat.Type() != MatType.CV_16U) depthMat = new Mat(_height / 2, _width / 2, MatType.CV_16U);
            unsafe
            {
                var dData = (ushort*)frame.Data;
                var pixels = (ushort*)depthMat.DataPointer;
                for (int i = 0; i < depthMat.Width * depthMat.Height; i++)
                {
                    pixels[i] = dData[i];
                }
            }
        }

        public void To8bitDepthMat(DepthFrame frame, ref Mat depthMat)
        {
            if (depthMat.Type() != MatType.CV_8U || depthMat.Empty()) depthMat = new Mat(_height / 2, _width / 2, MatType.CV_8U);
            unsafe
            {
                var dData = (ushort*)frame.Data;
                var pixels = depthMat.DataPointer;
                for (int i = 0; i < depthMat.Width * depthMat.Height; i++)
                {
                    if (dData[i] > 10000) dData[i] = 0;
                    pixels[i] = (byte)(dData[i] * 255 / 10000);
                }
            }
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
