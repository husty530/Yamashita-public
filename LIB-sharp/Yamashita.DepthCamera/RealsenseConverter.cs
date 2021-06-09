using OpenCvSharp;
using Intel.RealSense;

namespace Yamashita.DepthCamera
{
    public class RealsenseConverter
    {

        // ------- Fields ------- //

        private readonly int _cWidth;
        private readonly int _cHeight;
        private readonly int _dWidth;
        private readonly int _dHeight;


        // ------- Constructor ------- //

        public RealsenseConverter(int cWidth, int cHeight, int dWidth, int dHeight)
        {
            _cWidth = cWidth;
            _cHeight = cHeight;
            _dWidth = dWidth;
            _dHeight = dHeight;
        }


        // ------- Methods ------- //

        public void ToColorMat(VideoFrame frame, ref Mat colorMat)
        {
            colorMat = new Mat(_cHeight, _cWidth, MatType.CV_8UC3);
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
            Cv2.Resize(colorMat, colorMat, new Size(_cWidth / 2, _cHeight / 2));
        }

        public void ToPointCloudMat(Frame frame, ref Mat pointCloudMat)
        {
            if (pointCloudMat.Type() != MatType.CV_16UC3) pointCloudMat = new Mat(_dHeight / 2, _dWidth / 2, MatType.CV_16UC3);
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
