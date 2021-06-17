using System;

namespace Yamashita.DepthCamera
{
    /// <summary>
    /// Now supporting for Microsoft Azure Kinect & Intel RealSense D415 - 455, L515
    /// </summary>
    public interface IDepthCamera
    {

        /// <summary>
        /// Start streaming
        /// </summary>
        /// <returns></returns>
        public IObservable<BgrXyzMat> Connect();

        /// <summary>
        /// Stop streaming
        /// </summary>
        public void Disconnect();

    }

}
