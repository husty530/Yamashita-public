using System;
using System.IO;
using System.IO.Compression;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;

namespace Yamashita.DepthCameras
{
    public class Kinect : IDepthCamera
    {
        private readonly Device _device;
        private readonly Transformation _transformation;
        private readonly KinectConverter _converter;
        private IDisposable _cameraDisposer;

        public Kinect(DeviceConfiguration config)
        {
            _device = Device.Open();
            _device.StartCameras(config);
            _transformation = _device.GetCalibration().CreateTransformation();
            CameraCalibration c = _device.GetCalibration().DepthCameraCalibration;
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
                .Publish();
            _cameraDisposer = observable.Connect();
            return observable;
        }

        public void PauseStream()
        {
            _cameraDisposer?.Dispose();
        }

        public void Disconnect()
        {
            _cameraDisposer?.Dispose();
            _device?.Dispose();
        }

        public void SaveAsZip(string saveDirectory, string baseName, Mat colorMat, Mat depthMat, Mat pointCloudMat)
        {
            var zipFileNumber = 0;
            while (File.Exists($"{saveDirectory}\\Image_{baseName}{zipFileNumber:D4}.zip")) zipFileNumber++;
            string filePath = $"{saveDirectory}\\Image_{baseName}{zipFileNumber:D4}.zip";
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
}
