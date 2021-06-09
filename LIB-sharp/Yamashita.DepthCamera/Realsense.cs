using System;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using OpenCvSharp;
using Intel.RealSense;

namespace Yamashita.DepthCamera
{
    public class Realsense : IDepthCamera
    {

        // ------- Fields ------- //

        private readonly Pipeline _pipeline;
        private readonly Align _align;
        private readonly DecimationFilter _dfill;
        private readonly DisparityTransform _depthto;
        private readonly DisparityTransform _todepth;
        private readonly SpatialFilter _sfill;
        private readonly TemporalFilter _tfill;
        private readonly RealsenseConverter _converter;
        private readonly bool _alignOn;


        // ------- Properties ------- //

        public (int Width, int Height) DepthFrameSize { private set; get; }
        public (int Width, int Height) ColorFrameSize { private set; get; }


        // ------- Constructor ------- //

        /// <summary>
        /// Open Device
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Realsense(int width, int height)
        {
            DepthFrameSize = (width, height);
            ColorFrameSize = DepthFrameSize;
            _pipeline = new Pipeline();
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, width, height);
            cfg.EnableStream(Stream.Color, Format.Rgb8);
            _pipeline.Start(cfg);
            _align = new Align(Stream.Depth);
            _dfill = new DecimationFilter();
            _depthto = new DisparityTransform(true);
            _todepth = new DisparityTransform(false);
            _sfill = new SpatialFilter();
            _tfill = new TemporalFilter();
            _converter = new RealsenseConverter(width, height, width, height);
            _alignOn = true;
        }

        /// <summary>
        /// Open Device
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Realsense(int cWidth, int cHeight, int dWidth, int dHeight)
        {
            ColorFrameSize = (cWidth, cHeight);
            DepthFrameSize = (dWidth, dHeight);
            _pipeline = new Pipeline();
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, dWidth, dHeight);
            cfg.EnableStream(Stream.Color, cWidth, cHeight, Format.Rgb8);
            _pipeline.Start(cfg);
            _align = new Align(Stream.Color);
            _dfill = new DecimationFilter();
            _depthto = new DisparityTransform(true);
            _todepth = new DisparityTransform(false);
            _sfill = new SpatialFilter();
            _tfill = new TemporalFilter();
            _converter = new RealsenseConverter(cWidth, cHeight, dWidth, dHeight);
            _alignOn = false;
        }


        // ------- Methods ------- //

        public IObservable<BgrXyzMat> Connect()
        {
            var colorMat = new Mat();
            var pointCloudMat = new Mat();
            var observable = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    var frames = _pipeline.WaitForFrames();
                    if (_alignOn) frames = _align.Process(frames).AsFrameSet();
                    using var color = frames.ColorFrame.DisposeWith(frames);
                    using var depth = frames.DepthFrame.DisposeWith(frames);
                    var filterd = _dfill.Process(depth);
                    filterd = _depthto.Process(filterd);
                    filterd = _sfill.Process(filterd);
                    filterd = _tfill.Process(filterd);
                    filterd = _todepth.Process(filterd);
                    _converter.ToColorMat(color, ref colorMat);
                    _converter.ToPointCloudMat(filterd, ref pointCloudMat);
                    return new BgrXyzMat(colorMat, pointCloudMat);
                })
                .Publish()
                .RefCount();
            return observable;
        }

        public void Disconnect() => _pipeline?.Dispose();

    }

}
