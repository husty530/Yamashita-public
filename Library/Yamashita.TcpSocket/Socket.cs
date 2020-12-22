using System;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using OpenCvSharp;

namespace Yamashita.TcpSocket
{
    public abstract class Socket
    {

        protected TcpListener _listener;
        protected TcpClient _client;
        protected NetworkStream _stream;

        public abstract void Close();

        /// <summary>
        /// 送る
        /// </summary>
        /// <param name="sendmsg"></param>
        public void Send<T>(T sendmsg)
        {
            var sendBytes = Encoding.UTF8.GetBytes($"{sendmsg}\n");
            _stream.Write(sendBytes, 0, sendBytes.Length);
        }

        /// <summary>
        /// 受け取る
        /// </summary>
        /// <returns></returns>
        public T Receive<T>()
        {
            var ms = new MemoryStream();
            byte[] resBytes = new byte[256];
            int resSize = 0;
            do
            {
                resSize = _stream.Read(resBytes, 0, resBytes.Length);
                if (resSize == 0) break;
                ms.Write(resBytes, 0, resSize);
            } while (_stream.DataAvailable || resBytes[resSize - 1] != '\n');
            var receivedMsg = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            receivedMsg = receivedMsg.TrimEnd('\n');
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(receivedMsg);
        }

        /// <summary>
        /// 配列を送る
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public void SendArray<T>(T[] array)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < array.Length - 1; i++)
            {
                sb.Append(array[i]);
                sb.Append(",");
            }
            sb.Append(array[array.Length - 1]);
            sb.Append("\n");
            var sendBytes = Encoding.UTF8.GetBytes(sb.ToString());
            _stream.Write(sendBytes, 0, sendBytes.Length);
        }

        /// <summary>
        /// 配列を受け取る
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] ReceiveArray<T>()
        {
            var stringArray = Receive<string>().Split(",");
            var tArray = new T[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                tArray[i] = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(stringArray[i]);
            }
            return tArray;
        }

        /// <summary>
        /// 画像を送る
        /// </summary>
        /// <param name="image"></param>
        public void SendImage(Mat image)
        {
            Cv2.ImEncode(".png", image, out byte[] buf);
            var data = Convert.ToBase64String(buf);
            var sendmsg = $"data:image/png;base64,{data}";
            Send(sendmsg);
        }

        /// <summary>
        /// 画像を受け取る
        /// </summary>
        /// <returns></returns>
        public Mat ReceiveImage()
        {
            var res = Receive<string>();
            var data = res.Split(",")[1];
            var bytes = Convert.FromBase64String(data);
            return Cv2.ImDecode(bytes, ImreadModes.Color);
        }

    }
}
