using System;
using OpenCvSharp;

namespace Yamashita.DepthCamera
{
    public interface IDepthCamera
    {

        /// <summary>
        /// カメラのストリーム配信開始
        /// </summary>
        /// <returns>RGB, D, PointCloud画像のタプル</returns>
        public IObservable<(Mat ColorMat, Mat DepthMat, Mat PointCloudMat)> Connect();

        /// <summary>
        /// 配信の停止
        /// </summary>
        public void Disconnect();

    }
}
