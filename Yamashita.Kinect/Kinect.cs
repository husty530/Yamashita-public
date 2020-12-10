using System;
using System.Drawing;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Azure.Kinect.Sensor;
using Image = Microsoft.Azure.Kinect.Sensor.Image;

namespace Yamashita.Kinect
{
    public class Kinect : IDisposable
    {

        private IDisposable _cameraDisposer;
        private readonly Device _device;
        private readonly Transformation _transformation;

        public Kinect()
            : this(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R720p,
                DepthMode = DepthMode.NFOV_2x2Binned,
                SynchronizedImagesOnly = true,
                CameraFPS = FPS.FPS15
            })
        {

        }

        public Kinect(DeviceConfiguration config)
        {
            _device = Device.Open();
            _device.StartCameras(config);
            _transformation = _device.GetCalibration().CreateTransformation();
            var c = _device.GetCalibration().DepthCameraCalibration;
            FrameSize = new Size(c.ResolutionWidth, c.ResolutionHeight);
            Config = config;
        }

        public DeviceConfiguration Config { private set; get; }
        public Size FrameSize { private set; get; }

        public IObservable<(Image colorImg, Image depthImg, Image pointCloudImg)> StartStream()
        {
            var observable = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    var capture = _device.GetCapture();
                    var colorImg = _transformation.ColorImageToDepthCamera(capture);
                    var depthImg = capture.Depth;
                    var pointCloudImg = _transformation.DepthImageToPointCloud(depthImg);
                    var imgs = (colorImg, depthImg, pointCloudImg);
                    return imgs;
                })
                .Publish();
            _cameraDisposer = observable.Connect();
            return observable;
        }

        public void Pause()
        {
            if (_cameraDisposer != null) _cameraDisposer.Dispose();
        }

        public void Dispose()
        {
            _cameraDisposer.Dispose();
            if (_device != null) _device.Dispose();
        }

    }
}
