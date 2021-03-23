using System;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using OpenCvSharp;

namespace Yamashita.TcpSocket
{
    public abstract class TcpSocket : ITcpSocket
    {

        // フィールド

        protected TcpListener _listener;
        protected TcpClient _client;
        protected NetworkStream _stream;


        // メソッド

        public abstract void Close();

        public void Send<T>(T sendmsg)
        {
            var bytes = Encoding.UTF8.GetBytes($"{sendmsg}\n");
            _stream.Write(bytes, 0, bytes.Length);
        }

        public T Receive<T>()
        {
            using var ms = new MemoryStream();
            var bytes = new byte[1024];
            var size = 0;
            do
            {
                size = _stream.Read(bytes, 0, bytes.Length);
                if (size == 0) break;
                ms.Write(bytes, 0, size);
            } while (_stream.DataAvailable || bytes[size - 1] != '\n');
            var receivedMsg = Encoding.UTF8.GetString(ms.ToArray());
            receivedMsg = receivedMsg.TrimEnd('\n');
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(receivedMsg);
        }

        public void SendBytes(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }

        public byte[] ReceiveBytes()
        {
            using var ms = new MemoryStream();
            var bytes = new byte[1024];
            var size = 0;
            do
            {
                size = _stream.Read(bytes, 0, bytes.Length);
                if (size == 0) break;
                ms.Write(bytes, 0, size);
            } while (_stream.DataAvailable || bytes[size - 1] != '\n');
            return ms.ToArray();
        }

        public void SendArray<T>(T[] array)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < array.Length - 1; i++)
            {
                sb.Append(array[i]);
                sb.Append(',');
            }
            sb.Append(array[array.Length - 1]);
            sb.Append('\n');
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            _stream.Write(bytes, 0, bytes.Length);
        }

        public T[] ReceiveArray<T>()
        {
            var stringArray = Receive<string>().Split(',');
            var tArray = new T[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                tArray[i] = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(stringArray[i]);
            }
            return tArray;
        }

        public void SendImage(Mat image)
        {
            Cv2.ImEncode(".png", image, out byte[] buf);
            var data = Convert.ToBase64String(buf);
            var sendmsg = $"data:image/png;base64,{data}";
            Send(sendmsg);
        }

        public Mat ReceiveImage()
        {
            var recv = Receive<string>();
            var data = recv.Split(',')[1];
            var bytes = Convert.FromBase64String(data);
            return Cv2.ImDecode(bytes, ImreadModes.Color);
        }

    }
}
