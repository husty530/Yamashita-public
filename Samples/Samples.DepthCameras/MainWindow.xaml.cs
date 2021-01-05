using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Reactive.Linq;
using Reactive.Bindings;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Yamashita.DepthCameras;

namespace Samples.DepthCameras
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private IDepthCamera _camera;
        private bool _isConnected;

        public ReactiveProperty<string> StartButtonFace { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> SaveDir { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<BitmapSource> ColorFrame { private set; get; } = new ReactiveProperty<BitmapSource>();
        public ReactiveProperty<BitmapSource> DepthFrame { private set; get; } = new ReactiveProperty<BitmapSource>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _isConnected = false;
            StartButtonFace.Value = "▶";
            SaveDir.Value = $"{Directory.GetCurrentDirectory()}";
            ShutterButton.IsEnabled = false;
            this.Closed += (sender, args) =>
            {
                GC.Collect();
                _camera?.Disconnect();
            };
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                StartButtonFace.Value = "| |";
                _isConnected = AttemptConnection();
                if (!_isConnected) new Exception("Device Connection Failed.");
                ShutterButton.IsEnabled = true;
                _camera.Connect()
                        .ObserveOnDispatcher()
                        .Subscribe(imgs =>
                        {
                            ColorFrame.Value = imgs.ColorMat.ToBitmapSource();
                            DepthFrame.Value = imgs.DepthMat.ToBitmapSource();
                        });
            }
            else
            {
                StartButtonFace.Value = "▶";
                ShutterButton.IsEnabled = false;
                _isConnected = false;
                _camera.Disconnect();
            }
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            _camera.Connect()
                    .TakeWhile(imgs =>
                    {
                        if (imgs.ColorMat.Empty()) return true;
                        _camera.SaveAsZip(SaveDir.Value, "", imgs.ColorMat, imgs.DepthMat, imgs.PointCloudMat);
                        return false;
                    })
                    .Subscribe();
        }

        private void SelectDirButton_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = ("保存先フォルダを選択");
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveDir.Value = fbd.SelectedPath;
            }
        }

        private bool AttemptConnection()
        {
            try
            {
                _camera = new Kinect();
            }
            catch
            {
                try
                {
                    _camera = new Realsense();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

    }
}
