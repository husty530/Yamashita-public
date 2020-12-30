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

        /// <summary>
        /// デバイスの接続を開始
        /// </summary>
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

        public Realsense() : this(640, 480) { }

        public (int Width, int Height) FrameSize { private set; get; }

        /// <summary>
        /// 映像配信を開始
        /// </summary>
        /// <returns>KinectネイティブのImage型, ConverterクラスでMatに直す</returns>
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
                .Publish()
                .RefCount();
            return observable;
        }

        /// <summary>
        /// デバイスを閉じる
        /// </summary>
        public void Dispose()
        {
            _pipeline?.Dispose();
        }
    }
}
