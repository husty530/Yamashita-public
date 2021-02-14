using System;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{
    public interface IDepthCamera
    {
        public IObservable<(Mat ColorMat, Mat DepthMat, Mat PointCloudMat)> Connect();
        public void Disconnect();
        public void SaveAsZip(string saveDirectory, string baseName, Mat colorMat, Mat depthMat, Mat pointCloudMat);

    }
}
