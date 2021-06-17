using System;
using System.Net;
using System.Net.Sockets;

namespace Yamashita.TcpSocket
{
    /// <summary>
    /// Tcp socket server class
    /// </summary>
    public class Server : TcpSocket
    {

        // ------- Constructor ------- //

        /// <summary>
        /// Listening and connection
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Server(string ip, int port)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Parse(ip), port);
                _listener.Start();
                _client = _listener.AcceptTcpClient();
                _stream = _client.GetStream();
                Console.WriteLine("Connected.");
            }
            catch
            {
                throw new Exception("Connection failed!");
            }
        }


        // ------- Methods ------- //

        /// <summary>
        /// Close server
        /// </summary>
        public override void Close()
        {
            _stream?.Close();
            _client?.Close();
            _listener?.Stop();
        }

    }
}
