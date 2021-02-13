using System;
using System.Net;
using System.Net.Sockets;

namespace Yamashita.TcpSocket
{
    public class Server : TcpSocket
    {
        /// <summary>
        /// TCPサーバーの待機から接続まで
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">ポート番号</param>
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
                Console.WriteLine("Connection failed!");
                return;
            }
        }

        /// <summary>
        /// サーバーを閉じる
        /// </summary>
        public override void Close()
        {
            _stream.Close();
            _client.Close();
            _listener.Stop();
        }

    }
}
