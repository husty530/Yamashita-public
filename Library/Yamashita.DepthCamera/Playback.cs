using System;
using System.Threading;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Linq;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{
    public class Playback
    {

        private readonly ImageStreamReader? _reader;
        private readonly int _minDistance;
        private readonly int _maxDistance;

        public int PositionMax => _reader.FrameCount / 3;

        public Playback(string fileName, int minDistance = 300, int maxDistance = 5000)
        {
            _reader = new ImageStreamReader(fileName);
            _minDistance = minDistance;
            _maxDistance = maxDistance;
        }

        unsafe public IObservable<(Mat Color, Mat Depth8, Mat Depth16, Mat PointCloud, int Position)> Connect(int position)
        {
            if (_reader == null) throw new Exception("!!!");
            long pretime = 0;
            _reader.Seek(position * 3);
            var observable = Observable.Range(0, _reader.FrameCount / 3 - position, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    var (color, time, _) = _reader.ReadFrame();
                    var (depth16, _, _) = _reader.ReadFrame();
                    var (pointCloud, _, _) = _reader.ReadFrame();
                    var depth8 = new Mat(depth16.Height, depth16.Width, MatType.CV_8U);
                    var d8 = depth8.DataPointer;
                    var d16 = (ushort*)depth16.Data;
                    for (int j = 0; j < depth16.Width * depth16.Height; j++)
                    {
                        if (d16[j] < 300) d8[j] = 0;
                        else if (d16[j] < _maxDistance) d8[j] = (byte)((d16[j] - _minDistance) * 255 / (_maxDistance - _minDistance));
                        else d8[j] = 255;
                    }
                    time /= 10000;
                    var dt = (int)(time - pretime - 15);
                    Thread.Sleep(dt);
                    pretime = time;
                    return (color, depth8, depth16, pointCloud, position++);
                })
                .Publish()
                .RefCount();
            return observable;
        }

        public (Mat ColorMat, Mat DepthMat, Mat PointCloudMat) GetOneFrameSet(int position)
        {
            if (_reader == null) throw new Exception("!!!");
            _reader.Seek(position * 3);
            var (colorMat, _, _) = _reader.ReadFrame();
            var (depthMat, _, _) = _reader.ReadFrame();
            var (pointCloudMat, _, _) = _reader.ReadFrame();
            return (colorMat, depthMat, pointCloudMat);
        }

        public void Disconnect()
        {
            _reader?.Close();
        }
    }
}
