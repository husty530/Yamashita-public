using System;
using System.IO;
using System.IO.Compression;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;
using Image = Microsoft.Azure.Kinect.Sensor.Image;

namespace Yamashita.DepthCamera
{
    public class Kinect : IDepthCamera
    {
        private readonly Device _device;
        private readonly Transformation _transformation;
        private readonly KinectConverter _converter;

        public Kinect(DeviceConfiguration config)
        {
            _device = Device.Open();
            _device.StartCameras(config);
            _transformation = _device.GetCalibration().CreateTransformation();
            var c = _device.GetCalibration().DepthCameraCalibration;
            FrameSize = new Size(c.ResolutionWidth, c.ResolutionHeight);
            Config = config;
            _converter = new KinectConverter(FrameSize);
        }

        public Kinect()
            : this(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R720p,
                DepthMode = DepthMode.NFOV_2x2Binned,
                SynchronizedImagesOnly = true,
                CameraFPS = FPS.FPS15
            })
        { }

        public DeviceConfiguration Config { private set; get; }
        public Size FrameSize { private set; get; }


        public IObservable<(Mat ColorMat, Mat DepthMat, Mat PointCloudMat)> Connect()
        {
            var colorMat = new Mat();
            var depthMat = new Mat();
            var pointCloudMat = new Mat();
            var observable = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    var capture = _device.GetCapture();
                    var colorImg = _transformation.ColorImageToDepthCamera(capture);
                    var pointCloudImg = _transformation.DepthImageToPointCloud(capture.Depth);
                    _converter.ToColorMat(colorImg, ref colorMat);
                    _converter.ToPointCloudMat(pointCloudImg, ref pointCloudMat);
                    depthMat = pointCloudMat.Split()[2].Clone();
                    return (colorMat, depthMat, pointCloudMat);
                })
                .Publish()
                .RefCount();
            return observable;
        }

        public void Disconnect() => _device?.Dispose();

        public void SaveAsZip(string saveDirectory, string baseName, Mat colorMat, Mat depthMat, Mat pointCloudMat)
        {
            var zipFileNumber = 0;
            while (File.Exists($"{saveDirectory}\\Image_{baseName}{zipFileNumber:D4}.zip")) zipFileNumber++;
            var filePath = $"{saveDirectory}\\Image_{baseName}{zipFileNumber:D4}.zip";
            Cv2.ImWrite($"{filePath}_C.png", colorMat);
            Cv2.ImWrite($"{filePath}_D.png", depthMat);
            Cv2.ImWrite($"{filePath}_P.png", pointCloudMat);
            using (var z = ZipFile.Open($"{filePath}", ZipArchiveMode.Update))
            {
                z.CreateEntryFromFile($"{filePath}_C.png", $"C.png", CompressionLevel.Optimal);
                z.CreateEntryFromFile($"{filePath}_D.png", $"D.png", CompressionLevel.Optimal);
                z.CreateEntryFromFile($"{filePath}_P.png", $"P.png", CompressionLevel.Optimal);
            }
            File.Delete($"{filePath}_C.png");
            File.Delete($"{filePath}_D.png");
            File.Delete($"{filePath}_P.png");
        }
    }

    class KinectConverter
    {
        private readonly int _width;
        private readonly int _height;
        private readonly double pitch;
        private readonly double yaw;
        private readonly double cx;
        private readonly double cy;
        private readonly double fx;
        private readonly double fy;
        private double centerX => _width / 2;
        private double centerY => _height / 2;

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
                    var x3d = (px - centerX) * z3d / fx;
                    var y3d = (py - centerY) * z3d / fy;
                    pixels[index++] = (ushort)x3d;
                    pixels[index++] = (ushort)y3d;
                    pixels[index++] = (ushort)z3d;
                }
            }
        }
    }
}
