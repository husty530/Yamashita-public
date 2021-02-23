using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Linq;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{
    public class VideoPlayer
    {

        private readonly BinaryReader _binReader;
        private readonly long[] _indexes;
        private readonly int _minDistance;
        private readonly int _maxDistance;
        private long _pretime;
        private int _positionIndex;

        public int FrameCount => _indexes.Length;
        public int PositionMax => FrameCount / 3;

        /// <summary>
        /// DepthCamera用のプレーヤー
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="minDistance"></param>
        /// <param name="maxDistance"></param>
        public VideoPlayer(string filePath, int minDistance = 300, int maxDistance = 5000)
        {
            _binReader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read), Encoding.ASCII);
            var fileFormatCode = Encoding.ASCII.GetString(_binReader.ReadBytes(8));
            if (fileFormatCode != "HQIMST00" && fileFormatCode != "HUSTY000") throw new Exception();
            _binReader.BaseStream.Seek(8, SeekOrigin.Current);
            var indexesPos = _binReader.ReadInt64();
            if (indexesPos <= 0) throw new Exception();

            _binReader.BaseStream.Position = indexesPos;
            var indexes = new List<long>();
            while (_binReader.BaseStream.Position < _binReader.BaseStream.Length)
            {
                var pos = _binReader.ReadInt64();
                indexes.Add(pos);
            }
            _indexes = indexes.ToArray();
            _binReader.BaseStream.Position = 0;
            _minDistance = minDistance;
            _maxDistance = maxDistance;
            _pretime = 0;
        }

        /// <summary>
        /// ストリーム配信開始(RefCount方式)
        /// </summary>
        /// <param name="position">開始するフレーム番号</param>
        /// <returns></returns>
        unsafe public IObservable<(Mat Color, Mat Depth8, Mat Depth16, Mat PointCloud, int Position)> Start(int position)
        {
            Seek(position * 3);
            var observable = Observable.Range(0, FrameCount / 3 - position, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    var (color, time, _) = ReadFrame();
                    var (depth16, _, _) = ReadFrame();
                    var (pointCloud, _, _) = ReadFrame();
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
                    var dt = time - _pretime > 15 ? (int)(time - _pretime - 15) : 0;
                    Thread.Sleep(dt);
                    _pretime = time;
                    return (color, depth8, depth16, pointCloud, position++);
                })
                .Publish()
                .RefCount();
            return observable;
        }

        /// <summary>
        /// ストリームではなく1フレームだけ
        /// </summary>
        /// <param name="position">取得するフレーム番号</param>
        /// <returns></returns>
        unsafe public (Mat Color, Mat Depth8, Mat Depth16, Mat PointCloud) GetOneFrameSet(int position)
        {
            Seek(position * 3);
            var (color, _, _) = ReadFrame();
            var (depth16, _, _) = ReadFrame();
            var (pointCloud, _, _) = ReadFrame();
            var depth8 = new Mat(depth16.Height, depth16.Width, MatType.CV_8U);
            var d8 = depth8.DataPointer;
            var d16 = (ushort*)depth16.Data;
            for (int j = 0; j < depth16.Width * depth16.Height; j++)
            {
                if (d16[j] < 300) d8[j] = 0;
                else if (d16[j] < _maxDistance) d8[j] = (byte)((d16[j] - _minDistance) * 255 / (_maxDistance - _minDistance));
                else d8[j] = 255;
            }
            return (color, depth8, depth16, pointCloud);
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        public void Close()
        {
            _binReader.Close();
            _binReader.Dispose();
        }

        private (Mat Frame, long Time, byte[]? NULL) ReadFrame()
        {
            var pos = _indexes[_positionIndex++];
            _binReader.BaseStream.Seek(pos, SeekOrigin.Begin);
            var time = _binReader.ReadInt64();
            _binReader.BaseStream.Seek(2, SeekOrigin.Current);
            var imageDataSize = _binReader.ReadInt32();
            var image = Cv2.ImDecode(_binReader.ReadBytes(imageDataSize), ImreadModes.Unchanged);
            return (image, time, null);
        }

        private void Seek(int index)
        {
            if (index > -1 && index < FrameCount) _positionIndex = index;
        }
    }
}
