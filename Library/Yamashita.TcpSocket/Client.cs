using System;
using System.Net.Sockets;

namespace Yamashita.TcpSocket
{
    public class Client : TcpSocket
    {

        // コンストラクタ

        /// <summary>
        /// TCPクライアントの接続開始
        /// </summary>
        /// <param name="ip">サーバーのIP</param>
        /// <param name="port">ポート番号</param>
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
                Console.WriteLine("Connection failed!");
                return;
            }
        }


        // メソッド

        /// <summary>
        /// クライアントを閉じる
        /// </summary>
        public override void Close()
        {
            _stream.Close();
            _client.Close();
            Console.WriteLine("Closed.");
        }

    }
}
