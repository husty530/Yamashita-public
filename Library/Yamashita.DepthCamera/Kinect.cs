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
        private readonly float pitch;
        private readonly float yaw;
        private readonly float roll;


        // プロパティ

        public DeviceConfiguration Config { private set; get; }

        public Size FrameSize { private set; get; }


        // コンストラクタ

        /// <summary>
        /// Kinectのコンストラクタ
        /// </summary>
        /// <param name="config">デバイスのユーザー設定(任意)</param>
        public Kinect(DeviceConfiguration config, float pitch = -5.8f, float yaw = -1.3f, float roll = 0f)
        {
            this.pitch = (float)(pitch * Math.PI / 180);
            this.yaw = (float)(yaw * Math.PI / 180);
            this.roll = (float)(roll * Math.PI / 180);
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
        public Kinect(float pitch = 5.8f, float yaw = 1.3f, float roll = 0f)
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

        public IObservable<BgrXyzMat> Connect()
        {
            var colorMat = new Mat();
            var pointCloudMat = new Mat();
            var observable = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    using var capture = _device.GetCapture();
                    using var colorImg = _transformation.ColorImageToDepthCamera(capture);
                    using var pointCloudImg = _transformation.DepthImageToPointCloud(capture.Depth);
                    _converter.ToColorMat(colorImg, ref colorMat);
                    _converter.ToPointCloudMat(pointCloudImg, ref pointCloudMat);
                    var pc = new BgrXyzMat(colorMat, pointCloudMat);
                    pc.Rotate(pitch, yaw, roll);
                    return pc;
                })
                .Publish()
                .RefCount();
            return observable;
        }

        public void Disconnect() => _device?.Dispose();

    }

}
