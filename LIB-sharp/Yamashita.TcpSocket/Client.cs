using System;
using System.Net.Sockets;

namespace Yamashita.TcpSocket
{
    public class Client : TcpSocket
    {

        // ------- Constructor ------- //

        /// <summary>
        /// Start Connection
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Client(string ip, int port)
        {
            try
            {
                _client = new TcpClient(ip, port);
                _stream = _client.GetStream();
                Console.WriteLine("Connected.");
            }
            catch
            {
                _client?.Dispose();
                _stream?.Dispose();
                Console.WriteLine("Connection failed!");
                return;
            }
        }


        // ------- Methods ------- //

        /// <summary>
        /// Close Client
        /// </summary>
        public override void Close()
        {
            _stream?.Close();
            _client?.Close();
            Console.WriteLine("Closed.");
        }

    }
}
