using System;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using OpenCvSharp;
using Intel.RealSense;

namespace Yamashita.DepthCamera
{
    public class Realsense : IDepthCamera
    {

        // フィールド

        private readonly Pipeline _pipeline;
        private readonly Align _align;
        private readonly DecimationFilter _dfill;
        private readonly DisparityTransform _depthto;
        private readonly DisparityTransform _todepth;
        private readonly SpatialFilter _sfill;
        private readonly TemporalFilter _tfill;
        private readonly HoleFillingFilter _hfill;
        private readonly RealsenseConverter _converter;


        // プロパティ

        public (int Width, int Height) FrameSize { private set; get; }


        // コンストラクタ

        /// <summary>
        /// Realsenseのコンストラクタ
        /// </summary>
        /// <param name="width">所望の画像幅</param>
        /// <param name="height">所望の画像高さ</param>
        public Realsense(int width, int height)
        {
            FrameSize = (width, height);
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
            _hfill = new HoleFillingFilter();
            _converter = new RealsenseConverter(width, height);
        }

        public Realsense() : this(640, 480) { }

        
        // メソッド

        public IObservable<BgrXyzMat> Connect()
        {
            var colorMat = new Mat();
            var pointCloudMat = new Mat();
            var observable = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    var frames = _pipeline.WaitForFrames();
                    frames = _align.Process(frames).AsFrameSet();
                    using var color = frames.ColorFrame.DisposeWith(frames);
                    using var depth = frames.DepthFrame.DisposeWith(frames);
                    var filterd = _dfill.Process(depth);
                    filterd = _depthto.Process(filterd);
                    filterd = _sfill.Process(filterd);
                    filterd = _tfill.Process(filterd);
                    filterd = _todepth.Process(filterd);
                    using var depth2 = _hfill.Process<DepthFrame>(filterd);
                    _converter.ToColorMat(color, ref colorMat);
                    _converter.ToPointCloudMat(depth2, ref pointCloudMat);
                    return new BgrXyzMat(colorMat, pointCloudMat);
                })
                .Publish()
                .RefCount();
            return observable;
        }

        public void Disconnect() => _pipeline?.Dispose();

    }

}
