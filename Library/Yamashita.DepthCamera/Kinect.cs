using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;

namespace Yamashita.DepthCamera
{
    public class Kinect : IDepthCamera
    {

        // フィールド

        private readonly Device _device;
        private readonly Transformation _transformation;
        private readonly KinectConverter _converter;


        // プロパティ

        public DeviceConfiguration Config { private set; get; }

        public Size FrameSize { private set; get; }


        // コンストラクタ

        /// <summary>
        /// Kinectのコンストラクタ
        /// </summary>
        /// <param name="config">デバイスのユーザー設定(任意)</param>
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

        /// <summary>
        /// Kinectのデフォルトコンストラクタ
        /// </summary>
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


        // メソッド

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

    }

}
