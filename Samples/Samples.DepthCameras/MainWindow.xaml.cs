using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reactive.Linq;
using Reactive.Bindings;
using Microsoft.WindowsAPICodePack.Dialogs;
using OpenCvSharp.WpfExtensions;
using Yamashita.DepthCamera;

namespace Samples.DepthCameras
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private IDepthCamera _camera;
        private IDisposable _cameraConnector;
        private IDisposable _videoConnector;
        private bool _isConnected;

        public ReactiveProperty<string> StartButtonFace { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> RecButtonFace { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> SaveDir { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<BitmapSource> ColorFrame { private set; get; } = new ReactiveProperty<BitmapSource>();
        public ReactiveProperty<BitmapSource> DepthFrame { private set; get; } = new ReactiveProperty<BitmapSource>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _isConnected = false;
            StartButtonFace.Value = "Open";
            SaveDir.Value = $"D:";
            ShutterButton.IsEnabled = false;
            Closed += (sender, args) =>
            {
                GC.Collect();
                _videoConnector?.Dispose();
                _cameraConnector?.Dispose();
                _camera?.Disconnect();
            };
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                StartButtonFace.Value = "Close";
                RecButton.IsEnabled = false;
                PlayButton.IsEnabled = false;
                _isConnected = AttemptConnection();
                if (!_isConnected) new Exception("Device Connection Failed.");
                ShutterButton.IsEnabled = true;
                _cameraConnector = _camera.Connect()
                        .Subscribe(imgs =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                ColorFrame.Value = imgs.ColorMat.ToBitmapSource();
                                DepthFrame.Value = imgs.DepthMat.ToBitmapSource();
                            });
                        });
            }
            else
            {
                StartButtonFace.Value = "Open";
                RecButton.IsEnabled = true;
                PlayButton.IsEnabled = true;
                ShutterButton.IsEnabled = false;
                _isConnected = false;
                _cameraConnector?.Dispose();
                _camera?.Disconnect();
            }
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            _camera.Connect()
                    .TakeWhile(imgs =>
                    {
                        if (imgs.ColorMat.Empty()) return true;
                        ImageIO.SaveAsZip(SaveDir.Value, "", imgs.ColorMat, imgs.DepthMat, imgs.PointCloudMat);
                        return false;
                    })
                    .Subscribe();
        }

        private void RecButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isConnected)
            {
                RecButtonFace.Value = "Stop";
                StartPauseButton.IsEnabled = false;
                PlayButton.IsEnabled = false;
                _isConnected = AttemptConnection();
                if (!_isConnected) new Exception("Device Connection Failed.");
                var fileNumber = 0;
                while (File.Exists($"{SaveDir.Value}\\Movie_{fileNumber:D4}.yms")) fileNumber++;
                var filePath = $"{SaveDir.Value}\\Movie_{fileNumber:D4}.yms";
                var writer = new ImageStreamWriter(filePath);
                _cameraConnector = _camera.Connect()
                    .Finally(() => writer.Close())
                    .Subscribe(www =>
                    {
                        writer.WriteFrame(www.ColorMat);
                        writer.WriteFrame(www.DepthMat);
                        writer.WriteFrame(www.PointCloudMat);
                        Dispatcher.Invoke(() =>
                        {
                            ColorFrame.Value = www.ColorMat.ToBitmapSource();
                            DepthFrame.Value = www.DepthMat.ToBitmapSource();
                        });
                    });
            }
            else
            {
                RecButtonFace.Value = "Rec";
                StartPauseButton.IsEnabled = true;
                PlayButton.IsEnabled = true;
                _isConnected = false;
                _cameraConnector?.Dispose();
                _camera?.Disconnect();
            }
        }

        private void SelectDirButton_Click(object sender, RoutedEventArgs e)
        {
            using (var cofd = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = "D:",
                IsFolderPicker = true,
            })
            {
                if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
                    SaveDir.Value = cofd.FileName;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            _videoConnector?.Dispose();
            using (var cofd = new CommonOpenFileDialog()
            {
                Title = "動画を選択してください",
                InitialDirectory = "D:",
                IsFolderPicker = false,
            })
            {
                if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var player = new Playback(cofd.FileName);
                    _videoConnector = player.Connect(0)
                        .Finally(() => player.Disconnect())
                        .Subscribe(www =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                ColorFrame.Value = www.Color.ToBitmapSource();
                                DepthFrame.Value = www.Depth8.ToBitmapSource();
                            });
                        });
                }
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
                    throw new Exception("No Device !!");
                }
            }
            return true;
        }
    }
}
