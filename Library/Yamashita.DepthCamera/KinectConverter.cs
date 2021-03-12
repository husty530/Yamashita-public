using System;
using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;

namespace Yamashita.DepthCamera
{
    public class KinectConverter
    {

        // フィールド

        private readonly int _width;
        private readonly int _height;
        private readonly double pitch;
        private readonly double yaw;
        private readonly double cx;
        private readonly double cy;
        private readonly double fx;
        private readonly double fy;


        // プロパティ

        private double CenterX => _width / 2;

        private double CenterY => _height / 2;


        // コンストラクタ

        public KinectConverter(Size size, double pitchDeg = 5.8, double yawDeg = 1.3, double cx = 154.418, double cy = 169.663, double fx = 251.977, double fy = 252.004)
        {
            _width = size.Width;
            _height = size.Height;
            pitch = pitchDeg * Math.PI / 180;
            yaw = yawDeg * Math.PI / 180;
            this.cx = cx;
            this.cy = cy;
            this.fx = fx;
            this.fy = fy;
        }


        // メソッド

        public void ToColorMat(Image colorImg, ref Mat colorMat)
        {
            if (colorMat.Type() != MatType.CV_8UC3) colorMat = new Mat(_height, _width, MatType.CV_8UC3);
            var colorArray = colorImg.GetPixels<BGRA>().ToArray();
            unsafe
            {
                var pixels = colorMat.DataPointer;
                int index = 0;
                for (int i = 0; i < colorArray.Length; i++)
                {
                    pixels[index++] = colorArray[i].B;
                    pixels[index++] = colorArray[i].G;
                    pixels[index++] = colorArray[i].R;
                }
            }
        }

        public void ToPointCloudMat(Image pointCloudImg, ref Mat pointCloudMat)
        {
            if (pointCloudMat.Type() != MatType.CV_16SC3) pointCloudMat = new Mat(_height, _width, MatType.CV_16UC3);
            var pointCloudArray = pointCloudImg.GetPixels<Short3>().ToArray();
            unsafe
            {
                var pixels = (ushort*)pointCloudMat.Data;
                int index = 0;
                for (int i = 0; i < pointCloudArray.Length; i++)
                {
                    var px = i % _width;
                    var py = i / _width;
                    var prex = (px - cx) * (ushort)pointCloudArray[i].Z / fx;
                    var prey = (py - cy) * (ushort)pointCloudArray[i].Z / fy;
                    var prez = (short)((ushort)pointCloudArray[i].Z * Math.Cos(pitch) - prey * Math.Sin(pitch));
                    var z3d = (short)(prez * Math.Cos(yaw) + prex * Math.Sin(yaw));
                    var x3d = (px - CenterX) * z3d / fx;
                    var y3d = (py - CenterY) * z3d / fy;
                    pixels[index++] = (ushort)x3d;
                    pixels[index++] = (ushort)y3d;
                    pixels[index++] = (ushort)z3d;
                }
            }
        }
    }
    }
