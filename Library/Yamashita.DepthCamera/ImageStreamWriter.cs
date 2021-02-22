using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{
    public class ImageStreamWriter
    {

        private readonly BinaryWriter _binWriter;
        private readonly List<long> _indexes = new List<long>();
        private DateTimeOffset _firstTime;

        public ImageStreamWriter(string filePath)
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

        public void WriteFrame(Mat frame)
        {
            _indexes.Add(_binWriter.BaseStream.Position);
            _binWriter.Write((DateTimeOffset.Now - _firstTime).Ticks);
            _binWriter.Write((ushort)0);
            var buffer = frame.ImEncode();
            _binWriter.Write(buffer.Length);
            _binWriter.Write(buffer);
        }

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
