using System;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Intel.RealSense;

namespace Yamashita.Realsense
{
    public class Realsense : IDisposable
    {
        private readonly Pipeline _pipeline;
        private readonly Align _align;
        private readonly DecimationFilter _dfill;
        private readonly DisparityTransform _depthto;
        private readonly DisparityTransform _todepth;
        private readonly SpatialFilter _sfill;
        private readonly TemporalFilter _tfill;
        private readonly HoleFillingFilter _hfill;
        private IDisposable _cameraDisposer;

        public Realsense() : this(640, 480) { }

        public Realsense(int width, int height)
        {
            FrameSize = (width, height);
            _pipeline = new Pipeline();
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, width, height);
            cfg.EnableStream(Stream.Color, Format.Rgb8);
            var p = _pipeline.Start(cfg);
            _align = new Align(Stream.Depth);
            _dfill = new DecimationFilter();
            _depthto = new DisparityTransform(true);
            _todepth = new DisparityTransform(false);
            _sfill = new SpatialFilter();
            _tfill = new TemporalFilter();
            _hfill = new HoleFillingFilter();
        }

        public (int Width, int Height) FrameSize { private set; get; }

        public IObservable<(VideoFrame ColorFrame, DepthFrame DepthFrame)> CaptureStream()
        {
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
                    return (color, depth);
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
            if (_cameraDisposer != null) _cameraDisposer.Dispose();
            if (_pipeline != null) _pipeline.Dispose();
        }
    }
}
