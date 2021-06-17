using System;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using OpenCvSharp;

namespace Yamashita.TcpSocket
{
    /// <summary>
    /// This is abstract class so that you cannot create this instance.
    /// Use 'Server' or 'Client' classes.
    /// </summary>
    public abstract class TcpSocket : ITcpSocket
    {

        // ------- Fields ------- //

        protected TcpListener _listener;
        protected TcpClient _client;
        protected NetworkStream _stream;


        // ------- Properties ------- //

        /// <summary>
        /// If stream can available
        /// </summary>
        public bool Available => _stream != null;


        // ------- Methods ------- //

        public abstract void Close();

        /// <summary>
        /// Send any object as byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sendmsg">Any type of object</param>
        public void Send<T>(T sendmsg)
        {
            var bytes = Encoding.UTF8.GetBytes($"{sendmsg}\n");
            _stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Receive byte array & convert your preference type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T object</returns>
        public T Receive<T>()
        {
            using var ms = new MemoryStream();
            var bytes = new byte[2048];
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

        /// <summary>
        /// Send byte array directly
        /// </summary>
        /// <param name="bytes">Byte array</param>
        public void SendBytes(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Receive byte array directly
        /// </summary>
        /// <returns>Byte array</returns>
        public byte[] ReceiveBytes()
        {
            using var ms = new MemoryStream();
            var bytes = new byte[2048];
            var size = 0;
            do
            {
                size = _stream.Read(bytes, 0, bytes.Length);
                if (size == 0) break;
                ms.Write(bytes, 0, size);
            } while (_stream.DataAvailable || bytes[size - 1] != '\n');
            return ms.ToArray();
        }

        /// <summary>
        /// Send any object array as byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">Any type of object array</param>
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

        /// <summary>
        /// Receive byte array & convert your preference type of array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>T object array</returns>
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

        /// <summary>
        /// Send 'Mat' image as encoded byte array
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
        /// Receive byte array & convert 'Mat' image
        /// </summary>
        /// <returns></returns>
        public Mat ReceiveImage()
        {
            var recv = Receive<string>();
            var data = recv.Split(',')[1];
            var bytes = Convert.FromBase64String(data);
            return Cv2.ImDecode(bytes, ImreadModes.Unchanged);
        }

    }
}
