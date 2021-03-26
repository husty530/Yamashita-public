using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yamashita.DepthCamera
{
    public class VideoRecorder
    {

        // フィールド

        private readonly BinaryWriter _binWriter;
        private readonly List<long> _indexes = new List<long>();
        private readonly DateTimeOffset _firstTime;


        // コンストラクタ

        /// <summary>
        /// DepthCamera用のレコーダ
        /// </summary>
        /// <param name="filePath"></param>
        public VideoRecorder(string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath))) throw new Exception("Directory doesn't Exist!");
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


        // メソッド

        /// <summary>
        /// 一時刻分の書き込み
        /// </summary>
        /// <param name="Bgrxyz"></param>
        public void WriteFrame(BgrXyzMat Bgrxyz)
        {

            _indexes.Add(_binWriter.BaseStream.Position);
            _binWriter.Write((DateTimeOffset.Now - _firstTime).Ticks);
            _binWriter.Write((ushort)0);
            var buffer = Bgrxyz.BGR.ImEncode();
            _binWriter.Write(buffer.Length);
            _binWriter.Write(buffer);
            //_indexes.Add(_binWriter.BaseStream.Position);
            //_binWriter.Write((DateTimeOffset.Now - _firstTime).Ticks);
            //_binWriter.Write((ushort)0);
            //buffer = Bgrxyz.Depth16.ImEncode();
            //_binWriter.Write(buffer.Length);
            //_binWriter.Write(buffer);
            _indexes.Add(_binWriter.BaseStream.Position);
            _binWriter.Write((DateTimeOffset.Now - _firstTime).Ticks);
            _binWriter.Write((ushort)0);
            buffer = Bgrxyz.XYZ.ImEncode();
            _binWriter.Write(buffer.Length);
            _binWriter.Write(buffer);

        }

        /// <summary>
        /// 閉じる処理
        /// </summary>
        public void Close()
        {
            _binWriter.Seek(16, SeekOrigin.Begin);
            _binWriter.Write(_binWriter.BaseStream.Length);
            _binWriter.Seek(0, SeekOrigin.End);
            _indexes.ForEach(p => _binWriter.Write(p));
            _binWriter.Flush();
            _binWriter.Close();
            _binWriter.Dispose();
        }

    }
}
