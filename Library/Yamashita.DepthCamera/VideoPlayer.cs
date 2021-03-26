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

        // フィールド

        private readonly BinaryReader _binReader;
        private readonly long[] _indexes;
        private long _pretime;
        private int _positionIndex;


        // プロパティ

        public int FrameCount => _indexes.Length;

        public int PositionMax => FrameCount / 2;


        // コンストラクタ

        /// <summary>
        /// DepthCamera用のプレーヤー
        /// </summary>
        /// <param name="filePath"></param>
        public VideoPlayer(string filePath)
        {
            if (!File.Exists(filePath)) throw new Exception("File doesn't Exist!");
            _binReader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read), Encoding.ASCII);
            var fileFormatCode = Encoding.ASCII.GetString(_binReader.ReadBytes(8));
            //if (fileFormatCode != "HQIMST00" && fileFormatCode != "HUSTY000") throw new Exception();
            if (fileFormatCode != "HUSTY000") throw new Exception();
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
            _pretime = 0;
        }


        // メソッド

        /// <summary>
        /// ストリーム配信開始(RefCount方式)
        /// </summary>
        /// <param name="position">開始するフレーム番号</param>
        /// <returns></returns>
        unsafe public IObservable<(BgrXyzMat Bgrxyz, int Position)> Start(int position)
        {
            Seek(position * 2);
            var observable = Observable.Range(0, FrameCount / 2 - position, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    var (color, time, _) = ReadFrame();
                    //var _ = ReadFrame();
                    var (pointCloud, _, _) = ReadFrame();
                    time /= 10000;
                    var dt = time - _pretime > 15 ? (int)(time - _pretime - 15) : 0;
                    Thread.Sleep(dt);
                    _pretime = time;
                    return (new BgrXyzMat(color, pointCloud), position++);
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
        unsafe public BgrXyzMat GetOneFrameSet(int position)
        {
            Seek(position * 2);
            var (color, time, _) = ReadFrame();
            //var _ = ReadFrame();
            var (pointCloud, _, _) = ReadFrame();
            _pretime = time;
            return new BgrXyzMat(color, pointCloud);
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
