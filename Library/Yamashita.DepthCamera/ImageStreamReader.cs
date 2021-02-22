using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{
    public class ImageStreamReader
    {

        private readonly BinaryReader _binReader;
        private readonly long[] _indexes;
        private byte[] _imageBytes = new byte[0];

        public int PositionIndex { private set; get; }
        public int FrameCount => _indexes.Length;
        public bool AtEndOfStream => (PositionIndex >= FrameCount);


        public ImageStreamReader(string filePath)
        {
            _binReader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read), Encoding.ASCII);
            var fileFormatCode = Encoding.ASCII.GetString(_binReader.ReadBytes(8));
            if (fileFormatCode != "HQIMST00" && fileFormatCode != "HUSTY000") throw new Exception();
            _binReader.ReadBytes(8);
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
        }

        public (Mat Frame, long Time, byte[]? NULL) ReadFrame()
        {
            var pos = _indexes[PositionIndex];
            _binReader.BaseStream.Seek(pos, SeekOrigin.Begin);
            var time = _binReader.ReadInt64();
            _binReader.ReadUInt16();
            var imageDataSize = _binReader.ReadInt32();

            if (imageDataSize > _imageBytes.Length) _imageBytes = new byte[imageDataSize * 2];
            _binReader.Read(_imageBytes, 0, imageDataSize);
            var image = Cv2.ImDecode(_imageBytes, ImreadModes.Unchanged);
            PositionIndex++;
            return (image, time, null);
        }

        public void Seek(int index)
        {
            if (index > -1 && index < FrameCount) PositionIndex = index;
        }

        public void Close()
        {
            _binReader.Close();
            _binReader.Dispose();
        }

    }
}
