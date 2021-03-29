using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Linq;

namespace Yamashita.DepthCamera
{
    /// <summary>
    /// BGRXYZの映像をバイナリファイルから再生
    /// 
    /// データ構造
    /// 
    ///   byte        content
    ///  
    ///    1        Format Code
    ///    8       Stream Length
    ///    
    ///    8        Time Stamp
    ///    4         BGR Size
    /// BGR Size     BGR Frame
    ///    4         XYZ Size
    /// XYZ Size     XYZ Frame
    /// 
    ///    .
    ///    .
    ///    .
    ///    
    ///    8      Frame 1 Position
    ///    8      Frame 2 Position
    ///    8      Frame 3 Position
    ///   
    ///    .
    ///    .
    ///    .
    ///    
    /// </summary>
    public class VideoPlayer : IDisposable
    {

        // ------- Fields ------- //

        private readonly BinaryReader _binReader;
        private readonly long[] _indexes;
        private int _positionIndex;


        // ------- Properties ------- //

        public int FrameCount => _indexes.Length;

        public int FPS { set; get; }

        private int Interval => 800 / FPS;


        // ------- Constructor ------- //

        /// <summary>
        /// Player for Movies captured by Depth Camera
        /// </summary>
        /// <param name="filePath"></param>
        public VideoPlayer(string filePath, int fps = 15)
        {
            if (!File.Exists(filePath)) throw new Exception("File doesn't Exist!");
            _binReader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read), Encoding.ASCII);
            var fileFormatCode = Encoding.ASCII.GetString(_binReader.ReadBytes(8));
            if (fileFormatCode != "HUSTY000") throw new Exception();
            var indexesPos = _binReader.ReadInt64();
            if (indexesPos <= 0) throw new Exception();
            _binReader.BaseStream.Position = indexesPos;
            var indexes = new List<long>();
            while (_binReader.BaseStream.Position < _binReader.BaseStream.Length) indexes.Add(_binReader.ReadInt64());
            _indexes = indexes.ToArray();
            _binReader.BaseStream.Position = 0;
            FPS = fps;
        }


        // ------- Methods ------- //

        /// <summary>
        /// Start Streaming (RefCount)
        /// </summary>
        /// <param name="position">Starting Frame Index</param>
        /// <returns></returns>
        public IObservable<(BgrXyzMat Frames, int Position)> Start(int position)
        {
            if (position > -1 && position < FrameCount) _positionIndex = position;
            var observable = Observable.Range(0, FrameCount - position, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    Thread.Sleep(Interval);
                    return (ReadFrames().Frames, position++);
                })
                .Publish()
                .RefCount();
            return observable;
        }

        /// <summary>
        /// </summary>
        /// <param name="position">Frame Index</param>
        /// <returns></returns>
        public BgrXyzMat GetOneFrameSet(int position)
        {
            if (position > -1 && position < FrameCount) _positionIndex = position;
            return ReadFrames().Frames;
        }

        public void Dispose()
        {
            _binReader.Close();
            _binReader.Dispose();
        }

        private (BgrXyzMat Frames, long Time) ReadFrames()
        {
            _binReader.BaseStream.Seek(_indexes[_positionIndex++], SeekOrigin.Begin);
            var time = _binReader.ReadInt64();
            var bgrDataSize = _binReader.ReadInt32();
            var bgrBytes = _binReader.ReadBytes(bgrDataSize);
            var xyzDataSize = _binReader.ReadInt32();
            var xyzBytes = _binReader.ReadBytes(xyzDataSize);
            return (new BgrXyzMat(bgrBytes, xyzBytes), time);
        }

    }
}
