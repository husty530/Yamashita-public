using System;

namespace Yamashita.DepthCamera
{
    public interface IDepthCamera
    {

        /// <summary>
        /// Start Streaming
        /// </summary>
        /// <returns></returns>
        public IObservable<BgrXyzMat> Connect();

        /// <summary>
        /// Stop Streaming
        /// </summary>
        public void Disconnect();

    }

}
