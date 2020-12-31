using System;
using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;
using Image = Microsoft.Azure.Kinect.Sensor.Image;

namespace Yamashita.Kinect
{
    public class Converter
    {
        private readonly int _width;
        private readonly int _height;

        public Converter(System.Drawing.Size size)
        {
            _width = size.Width;
            _height = size.Height;
        }

        public void ImageToMat(Image srcImg, ref Mat dstMat)
        {
            switch (srcImg.Format)
            {
                case ImageFormat.ColorBGRA32:
                    ToColorMat(srcImg, ref dstMat);
                    break;
                case ImageFormat.Depth16:
                    ToDepthMat(srcImg, ref dstMat);
                    break;
                case ImageFormat.Custom:
                    ToPointCloudMat(srcImg, ref dstMat);
                    break;
                default:
                    throw new ApplicationException("Image format error!.");
            }
        }

        public void To8bitDepthMat(Image depthImg, ref Mat depthMat)
        {
            if (depthMat.Type() != MatType.CV_8U || depthMat.Empty()) depthMat = new Mat(_height, _width, MatType.CV_8U);
            ushort[] depthArray = depthImg.GetPixels<ushort>().ToArray();
            unsafe
            {
                var pixels = depthMat.DataPointer;
                for (int i = 0; i < depthMat.Width * depthMat.Height; i++)
                {
                    if (depthArray[i] > 5000) depthArray[i] = 0;
                    pixels[i] = (byte)(depthArray[i] * 255 / 5000);
                }
            }
        }

        private void ToColorMat(Image colorImg, ref Mat colorMat)
        {
            if (colorMat.Type() != MatType.CV_8UC3) colorMat = new Mat(_height, _width, MatType.CV_8UC3);
            BGRA[] colorArray = colorImg.GetPixels<BGRA>().ToArray();
            unsafe
            {
                byte* pixels = colorMat.DataPointer;
                int index = 0;
                for (int i = 0; i < colorArray.Length; i++)
                {
                    pixels[index++] = colorArray[i].B;
                    pixels[index++] = colorArray[i].G;
                    pixels[index++] = colorArray[i].R;
                }
            }
        }

        private void ToDepthMat(Image depthImg, ref Mat depthMat)
        {
            if (depthMat.Type() != MatType.CV_16U) depthMat = new Mat(_height, _width, MatType.CV_16U);
            ushort[] depthArray = depthImg.GetPixels<ushort>().ToArray();
            unsafe
            {
                ushort* pixels = (ushort*)depthMat.DataPointer;
                for (int i = 0; i < depthArray.Length; i++)
                {
                    pixels[i] = depthArray[i];
                }
            }
        }

        private void ToPointCloudMat(Image pointCloudImg, ref Mat pointCloudMat)
        {
            if (pointCloudMat.Type() != MatType.CV_16SC3) pointCloudMat = new Mat(_height, _width, MatType.CV_16UC3);
            Short3[] pointCloudArray = pointCloudImg.GetPixels<Short3>().ToArray();
            unsafe
            {
                ushort* pixels = (ushort*)pointCloudMat.Data;
                int index = 0;
                for (int i = 0; i < pointCloudArray.Length; i++)
                {
                    pixels[index++] = (ushort)pointCloudArray[i].X;
                    pixels[index++] = (ushort)pointCloudArray[i].Y;
                    pixels[index++] = (ushort)pointCloudArray[i].Z;
                }
            }
        }

    }
}
