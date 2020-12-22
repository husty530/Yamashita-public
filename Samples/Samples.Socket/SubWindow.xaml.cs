using System.Windows;
using System.Windows.Controls;
using Yamashita.TcpSocket;

namespace Samples.Socket
{
    /// <summary>
    /// SubWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SubWindow : Window
    {

        private Client _client;

        public SubWindow()
        {
            InitializeComponent();
            _client = new Client("127.0.0.1", 3000);
            Closed += (sender, args) => _client.Close();
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _client.Send(InputBox.Text);
        }
    }
}
