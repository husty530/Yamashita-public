using System;
using System.Net.Sockets;

namespace Yamashita.TcpSocket
{
    /// <summary>
    /// Tcp socket client class
    /// </summary>
    public class Client : TcpSocket
    {

        // ------- Constructor ------- //

        /// <summary>
        /// Start connection
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
                throw new Exception("Connection failed!");
            }
        }


        // ------- Methods ------- //

        /// <summary>
        /// Close client
        /// </summary>
        public override void Close()
        {
            _stream?.Close();
            _client?.Close();
            Console.WriteLine("Closed.");
        }

    }
}
