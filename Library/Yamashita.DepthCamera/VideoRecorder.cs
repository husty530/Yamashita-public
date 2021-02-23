using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{
    public class VideoRecorder
    {

        private readonly BinaryWriter _binWriter;
        private readonly List<long> _indexes = new List<long>();
        private readonly DateTimeOffset _firstTime;
        private byte[] _buffer;

        /// <summary>
        /// DepthCamera用のレコーダ
        /// </summary>
        /// <param name="filePath"></param>
        public VideoRecorder(string filePath)
        {
            _binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create), Encoding.ASCII);
            var fileFormatCode = Encoding.ASCII.GetBytes("HUSTY000");
            _binWriter.Write(fileFormatCode);
            _binWriter.Write(Encoding.ASCII.GetBytes("        "));
            _binWriter.Write(-1L);
            var time = DateTimeOffset.Now;
            _binWriter.Write(time.Ticks);
            _binWriter.Write(time.Offset.Ticks);
            _firstTime = time;
        }

        /// <summary>
        /// 一時刻分の書き込み
        /// </summary>
        /// <param name="color"></param>
        /// <param name="depth"></param>
        /// <param name="pointCloud"></param>
        public void WriteFrame(Mat color, Mat depth, Mat pointCloud)
        {
            _indexes.Add(_binWriter.BaseStream.Position);
            _binWriter.Write((DateTimeOffset.Now - _firstTime).Ticks);
            _binWriter.Write((ushort)0);
            _buffer = color.ImEncode();
            _binWriter.Write(_buffer.Length);
            _binWriter.Write(_buffer);
            _indexes.Add(_binWriter.BaseStream.Position);
            _binWriter.Write((DateTimeOffset.Now - _firstTime).Ticks);
            _binWriter.Write((ushort)0);
            _buffer = depth.ImEncode();
            _binWriter.Write(_buffer.Length);
            _binWriter.Write(_buffer);
            _indexes.Add(_binWriter.BaseStream.Position);
            _binWriter.Write((DateTimeOffset.Now - _firstTime).Ticks);
            _binWriter.Write((ushort)0);
            _buffer = pointCloud.ImEncode();
            _binWriter.Write(_buffer.Length);
            _binWriter.Write(_buffer);
        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        public void Close()
        {
            _binWriter.Seek(16, SeekOrigin.Begin);
            _binWriter.Write(_binWriter.BaseStream.Length);
            _binWriter.Seek(0, SeekOrigin.End);
            foreach (var pos in _indexes) _binWriter.Write(pos);
            _binWriter.Flush();
            _binWriter.Close();
            _binWriter.Dispose();
        }

    }
}
