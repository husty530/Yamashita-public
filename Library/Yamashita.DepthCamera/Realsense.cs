using System;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using OpenCvSharp;
using Intel.RealSense;

namespace Yamashita.DepthCamera
{
    public class Realsense : IDepthCamera
    {

        private readonly Pipeline _pipeline;
        private readonly Align _align;
        private readonly DecimationFilter _dfill;
        private readonly DisparityTransform _depthto;
        private readonly DisparityTransform _todepth;
        private readonly SpatialFilter _sfill;
        private readonly TemporalFilter _tfill;
        private readonly HoleFillingFilter _hfill;
        private readonly RealsenseConverter _converter;

        public Realsense(int width, int height)
        {
            FrameSize = (width, height);
            _pipeline = new Pipeline();
            var cfg = new Config();
            cfg.EnableStream(Intel.RealSense.Stream.Depth, width, height);
            cfg.EnableStream(Intel.RealSense.Stream.Color, Format.Rgb8);
            var p = _pipeline.Start(cfg);
            _align = new Align(Intel.RealSense.Stream.Depth);
            _dfill = new DecimationFilter();
            _depthto = new DisparityTransform(true);
            _todepth = new DisparityTransform(false);
            _sfill = new SpatialFilter();
            _tfill = new TemporalFilter();
            _hfill = new HoleFillingFilter();
            _converter = new RealsenseConverter(width, height);
        }

        public Realsense() : this(640, 480) { }

        public (int Width, int Height) FrameSize { private set; get; }

        public IObservable<(Mat ColorMat, Mat DepthMat, Mat PointCloudMat)> Connect()
        {
            var colorMat = new Mat();
            var depthMat = new Mat();
            var pointCloudMat = new Mat();
            var observable = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    var frames = _pipeline.WaitForFrames();
                    frames = _align.Process(frames).AsFrameSet();
                    var color = frames.ColorFrame.DisposeWith(frames);
                    var depth = frames.DepthFrame.DisposeWith(frames);
                    var filterd = _dfill.Process(depth);
                    filterd = _depthto.Process(filterd);
                    filterd = _sfill.Process(filterd);
                    filterd = _tfill.Process(filterd);
                    filterd = _todepth.Process(filterd);
                    depth = _hfill.Process<DepthFrame>(filterd);
                    _converter.ToColorMat(color, ref colorMat);
                    _converter.ToPointCloudMat(depth, ref pointCloudMat);
                    depthMat = pointCloudMat.Split()[2].Clone();
                    return (colorMat, depthMat, pointCloudMat);
                })
                .Publish()
                .RefCount();
            return observable;
        }

        public void Disconnect() => _pipeline?.Dispose();

    }

}
