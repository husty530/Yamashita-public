using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Yamashita.DepthCamera
{
    /// <summary>
    /// Save BGRXYZ movie as binary file.
    /// </summary>
    public class VideoRecorder : IDisposable
    {

        //
        // Data Structure
        // 
        //   byte        content
        //  
        //    1        Format Code
        //    8       Stream Length
        //    
        //    8        Time Stamp
        //    4         BGR Size
        // BGR Size     BGR Frame
        //    4         XYZ Size
        // XYZ Size     XYZ Frame
        // 
        //    .
        //    .
        //    .
        //    
        //    8      Frame 1 Position
        //    8      Frame 2 Position
        //    8      Frame 3 Position
        //   
        //    .
        //    .
        //    .
        //    

        // ------- Fields ------- //

        private readonly BinaryWriter _binWriter;
        private readonly List<long> _indexes = new List<long>();
        private readonly DateTimeOffset _firstTime;


        // ------- Constructor ------- //

        /// <summary>
        /// Movie recorder for Depth camera
        /// </summary>
        /// <param name="filePath"></param>
        public VideoRecorder(string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath))) throw new Exception("Directory doesn't Exist!");
            _binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create), Encoding.ASCII);
            var fileFormatCode = Encoding.ASCII.GetBytes("HUSTY000");
            _binWriter.Write(fileFormatCode);
            _binWriter.Write(-1L);
            _firstTime = DateTimeOffset.Now;
        }


        // ------- Methods ------- //

        /// <summary>
        /// Write frames of the time.
        /// </summary>
        /// <param name="Bgrxyz"></param>
        public void WriteFrames(BgrXyzMat Bgrxyz)
        {
            _indexes.Add(_binWriter.BaseStream.Position);
            _binWriter.Write((DateTimeOffset.Now - _firstTime).Ticks);
            var (bgr, xyz) = Bgrxyz.YmsEncode();
            _binWriter.Write(bgr.Length);
            _binWriter.Write(bgr);
            _binWriter.Write(xyz.Length);
            _binWriter.Write(xyz);

        }

        /// <summary>
        /// Finalize recording
        /// </summary>
        public void Dispose()
        {
            _binWriter.Seek(8, SeekOrigin.Begin);
            _binWriter.Write(_binWriter.BaseStream.Length);
            _binWriter.Seek(0, SeekOrigin.End);
            _indexes.ForEach(p => _binWriter.Write(p));
            _binWriter.Flush();
            _binWriter.Close();
            _binWriter.Dispose();
        }

    }
}
