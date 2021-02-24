using System.IO;
using OpenCvSharp;

namespace Yamashita.TcpSocket
{
    public interface ITcpSocket
    {
        public abstract void Close();

        /// <summary>
        /// 送る
        /// </summary>
        /// <param name="sendmsg"></param>
        public void Send<T>(T sendmsg);

        /// <summary>
        /// 受け取る
        /// </summary>
        /// <returns></returns>
        public T Receive<T>();

        /// <summary>
        /// ストリームを送る
        /// </summary>
        /// <param name="stream"></param>
        public void SendStream(MemoryStream stream);

        /// <summary>
        /// ストリームを受け取る
        /// </summary>
        /// <returns></returns>
        public MemoryStream ReceiveStream();

        /// <summary>
        /// 配列を送る
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public void SendArray<T>(T[] array);

        /// <summary>
        /// 配列を受け取る
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] ReceiveArray<T>();

        /// <summary>
        /// 画像を送る
        /// </summary>
        /// <param name="image"></param>
        public void SendImage(Mat image);

        /// <summary>
        /// 画像を受け取る
        /// </summary>
        /// <returns></returns>
        public Mat ReceiveImage();

    }
}
