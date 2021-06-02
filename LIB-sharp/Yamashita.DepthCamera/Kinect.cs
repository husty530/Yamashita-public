using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;

namespace Yamashita.DepthCamera
{
    public class Kinect : IDepthCamera
    {

        // ------- Fields ------- //

        private readonly Device _device;
        private readonly KinectConverter _converter;
        private readonly Transformation _transformation;
        private readonly CaliblationType _type;
        private readonly float _pitchRad;
        private readonly float _yawRad;
        private readonly float _rollRad;


        // ------- Properties ------- //

        public DeviceConfiguration Config { private set; get; }

        public Size ColorFrameSize { private set; get; }

        public Size DepthFrameSize { private set; get; }

        public enum CaliblationType { DepthBased, Separated }


        // ------- Constructor ------- //

        /// <summary>
        /// Open Device
        /// </summary>
        /// <param name="config">User Settings</param>
        public Kinect(DeviceConfiguration config, CaliblationType type = CaliblationType.DepthBased, float pitchDeg = -5.8f, float yawDeg = -1.3f, float rollDeg = 0f)
        {
            _type = type;
            _pitchRad = (float)(pitchDeg * Math.PI / 180);
            _yawRad = (float)(yawDeg * Math.PI / 180);
            _rollRad = (float)(rollDeg * Math.PI / 180);
            _device = Device.Open();
            _device.StartCameras(config);
            _transformation = _device.GetCalibration().CreateTransformation();
            var dcal = _device.GetCalibration().DepthCameraCalibration;
            var ccal = _device.GetCalibration().ColorCameraCalibration;
            DepthFrameSize = new Size(dcal.ResolutionWidth, dcal.ResolutionHeight);
            ColorFrameSize = new Size(ccal.ResolutionWidth, ccal.ResolutionHeight);
            if (_type == CaliblationType.DepthBased) ColorFrameSize = DepthFrameSize;
            Config = config;
            _converter = new KinectConverter(ColorFrameSize, DepthFrameSize);
        }

        /// <summary>
        /// Open Device (default)
        /// </summary>
        public Kinect(CaliblationType type = CaliblationType.DepthBased, float pitchDeg = -5.8f, float yawDeg = -1.3f, float rollDeg = 0f)
            : this(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R720p,
                DepthMode = DepthMode.NFOV_2x2Binned,
                SynchronizedImagesOnly = true,
                CameraFPS = FPS.FPS15
            },
            type, pitchDeg, yawDeg, rollDeg)
        { }


        // ------- Methods ------- //

        public IObservable<BgrXyzMat> Connect()
        {
            var colorMat = new Mat();
            var pointCloudMat = new Mat();
            var observable = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    using var capture = _device.GetCapture();
                    Image colorImg;
                    if (_type != CaliblationType.DepthBased)
                        colorImg = capture.Color;
                    else
                        colorImg = _transformation.ColorImageToDepthCamera(capture);
                    using var pointCloudImg = _transformation.DepthImageToPointCloud(capture.Depth);
                    _converter.ToColorMat(colorImg, ref colorMat);
                    _converter.ToPointCloudMat(pointCloudImg, ref pointCloudMat);
                    return BgrXyzMat.Create(colorMat, pointCloudMat).Rotate(_pitchRad, _yawRad, _rollRad);
                })
                .Publish()
                .RefCount();
            return observable;
        }

        public void Disconnect() => _device?.Dispose();

    }

}
