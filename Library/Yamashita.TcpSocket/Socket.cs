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
        /// 文字列を送る
        /// </summary>
        /// <param name="sendmsg"></param>
        public void SendMessage(string sendmsg)
        {
            var sendBytes = Encoding.UTF8.GetBytes(sendmsg);
            _stream.Write(sendBytes, 0, sendBytes.Length);
            Console.WriteLine($"Send : {sendmsg}");
        }

        /// <summary>
        /// 文字列を受け取る
        /// </summary>
        /// <returns></returns>
        public string ReceiveMessage()
        {
            var ms = new MemoryStream();
            byte[] resBytes = new byte[256];
            int resSize = 0;
            do
            {
                resSize = _stream.Read(resBytes, 0, resBytes.Length);
                if (resSize == 0)
                {
                    Console.WriteLine("Disconnect");
                    break;
                }
                ms.Write(resBytes, 0, resSize);
            } while (_stream.DataAvailable || resBytes[resSize - 1] != '\n');
            var receivedMsg = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            receivedMsg = receivedMsg.TrimEnd('\n');
            Console.WriteLine($"Receive : {receivedMsg}");
            return receivedMsg;
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
            Console.WriteLine($"Send : Array");
        }

        /// <summary>
        /// 配列を受け取る
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] ReceiveArray<T>()
        {
            var stringArray = ReceiveMessage().Split(",");
            var tArray = new T[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                tArray[i] = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(stringArray[i]);
            }
            Console.WriteLine("Receive : Array");
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
            SendMessage(sendmsg);
            Console.WriteLine("Send : Image");
        }

        /// <summary>
        /// 画像を受け取る
        /// </summary>
        /// <returns></returns>
        public Mat ReceiveImage()
        {
            var res = ReceiveMessage();
            var data = res.Split(",")[1];
            var bytes = Convert.FromBase64String(data);
            Console.WriteLine("Receive : Image");
            return Cv2.ImDecode(bytes, ImreadModes.Color);
        }

    }
}
