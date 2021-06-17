using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;

namespace Yamashita.DepthCamera
{
    /// <summary>
    /// Native API type -> OpenCvSharp format
    /// </summary>
    public class KinectConverter
    {

        // ------- Fields ------- //

        private readonly int _colorWidth;
        private readonly int _colorHeight;
        private readonly int _depthWidth;
        private readonly int _depthHeight;


        // ------- Constructor ------- //

        public KinectConverter(Size colorSize, Size depthSize)
        {
            _colorWidth = colorSize.Width;
            _colorHeight = colorSize.Height;
            _depthWidth = depthSize.Width;
            _depthHeight = depthSize.Height;
        }


        // ------- Methods ------- //

        public unsafe void ToColorMat(Image colorImg, ref Mat colorMat)
        {
            if (colorMat.Type() != MatType.CV_8UC3) colorMat = new Mat(_colorHeight, _colorWidth, MatType.CV_8UC3);
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
            if (pointCloudMat.Type() != MatType.CV_16UC3) pointCloudMat = new Mat(_depthHeight, _depthWidth, MatType.CV_16UC3);
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
