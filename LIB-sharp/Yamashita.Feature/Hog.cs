using System;
using OpenCvSharp;

namespace Yamashita.Feature
{
    /// <summary>
    /// OpenCvSharp 'HOGDescriptor' class wrapper.
    /// </summary>
    public class Hog
    {

        // ------- Fields ------- //

        private readonly HOGDescriptor _hog;
        private readonly Size _imageSize;


        // ------- Constructor ------- //

        /// <summary>
        /// Simple HOG descriptor
        /// </summary>
        /// <param name="imageSize"></param>
        /// <param name="blockSize">For normalization</param>
        /// <param name="blockStride"></param>
        /// <param name="cellSize">Unit size for computation</param>
        public Hog(Size? imageSize = null, Size? blockSize = null, Size? blockStride = null, Size? cellSize = null)
        {
            var s1 = (imageSize == null) ? new Size(64, 64) : imageSize;
            var s2 = (blockSize == null) ? new Size(16, 16) : blockSize;
            var s3 = (blockStride == null) ? new Size(8, 8) : blockStride;
            var s4 = (cellSize == null) ? new Size(8, 8) : cellSize;
            _hog = new HOGDescriptor(s1, s2, s3, s4);
            _imageSize = (Size)s1;
        }


        // ------- Methods ------- //

        /// <summary>
        /// Process one frame.
        /// </summary>
        /// <param name="input">8 bit gray-scale image. It's going to be resized automatically.</param>
        /// <param name="result">Feature of values</param>
        public void Compute(Mat input, out float[] result)
        {
            if (input.Type() != MatType.CV_8U) new Exception("MatType should be 'CV_8U'.");
            Cv2.Resize(input, input, _imageSize);
            result = _hog.Compute(input);
        }

    }
}
