using System;

namespace Yamashita.DepthCamera
{
    public interface IDepthCamera
    {

        /// <summary>
        /// カメラのストリーム配信開始
        /// </summary>
        /// <returns></returns>
        public IObservable<BgrXyzMat> Connect();

        /// <summary>
        /// 配信の停止
        /// </summary>
        public void Disconnect();

    }

}
