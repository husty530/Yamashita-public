using OpenCvSharp;

namespace Yamashita.TcpSocket
{
    public interface ITcpSocket
    {

        public bool Available { get; }

        public abstract void Close();

        public void Send<T>(T sendmsg);

        public T Receive<T>();

        public void SendBytes(byte[] bytes);

        public byte[] ReceiveBytes();

        public void SendArray<T>(T[] array);

        public T[] ReceiveArray<T>();

        public void SendImage(Mat image);

        public Mat ReceiveImage();

    }
}
