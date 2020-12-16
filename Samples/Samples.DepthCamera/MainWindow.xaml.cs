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
using Yamashita.Kinect;
using Yamashita.Realsense;

namespace Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private enum Device { None, Kinect, Realsense }
        private Device _device;
        private Kinect _kinect;
        private Realsense _realsense;
        private Yamashita.Kinect.Converter _kinectConverter;
        private Yamashita.Realsense.Converter _realsenseConverter;
        private bool _isConnected;
        private IDisposable _kinectReceiver;
        private IDisposable _realsenseReceiver;

        public ReactiveProperty<string> StartButtonFace { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> SaveDir { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<BitmapSource> ColorFrame { private set; get; } = new ReactiveProperty<BitmapSource>();
        public ReactiveProperty<BitmapSource> DepthFrame { private set; get; } = new ReactiveProperty<BitmapSource>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _device = Device.None;
            _isConnected = false;
            StartButtonFace.Value = "▶";
            SaveDir.Value = $"{Directory.GetCurrentDirectory()}";
            ShutterButton.IsEnabled = false;

            this.Closed += (sender, args) =>
            {
                GC.Collect();
                if (_kinectReceiver != null) _kinectReceiver.Dispose();
                if (_realsenseReceiver != null) _realsenseReceiver.Dispose();
                if(_kinect != null) _kinect.Dispose();
                if(_realsense != null) _realsense.Dispose();
            };
        }

        private bool AttemptConnection()
        {
            // Kinect接続
            try
            {
                _kinect = new Kinect();
                _kinectConverter = new Yamashita.Kinect.Converter(_kinect.FrameSize);
                _device = Device.Kinect;
                return true;
            }
            // だめなら
            catch
            {
                // Realsense接続
                try
                {
                    _realsense = new Realsense();
                    _realsenseConverter = new Yamashita.Realsense.Converter(_realsense.FrameSize.Width, _realsense.FrameSize.Height);
                    _device = Device.Realsense;
                    return true;
                }
                // それもだめならエラー吐く
                catch
                {
                    _device = Device.None;
                    return false;
                }
            }
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            // スタートするとき
            if (!_isConnected)
            {
                StartButtonFace.Value = "| |";
                _isConnected = AttemptConnection();
                if (!_isConnected) new Exception("Device Connection Failed.");
                ShutterButton.IsEnabled = true;
                var colorMat = new Mat();
                var depthMat = new Mat();
                // Kinect
                if (_device == Device.Kinect)
                {
                    _kinectReceiver = _kinect.StartStream()
                        .Select(imgs =>
                        {
                            using (imgs.ColorFrame)
                            using (imgs.DepthFrame)
                            {
                                _kinectConverter.ImageToMat(imgs.ColorFrame, ref colorMat);
                                _kinectConverter.To8bitDepthMat(imgs.DepthFrame, ref depthMat);
                                return (colorMat, depthMat);
                            }
                        })
                        .ObserveOnDispatcher()
                        .Subscribe(imgs =>
                        {
                            ColorFrame.Value = imgs.colorMat.ToBitmapSource();
                            DepthFrame.Value = imgs.depthMat.ToBitmapSource();
                        });
                        
                }
                // Realsense
                else if(_device == Device.Realsense)
                {
                    _realsenseReceiver = _realsense.CaptureStream()
                        .Select(imgs =>
                        {
                            _realsenseConverter.ToColorMat(imgs.ColorFrame, ref colorMat);
                            _realsenseConverter.To8bitDepthMat(imgs.DepthFrame, ref depthMat);
                            imgs.ColorFrame.Dispose();
                            imgs.DepthFrame.Dispose();
                            return (colorMat, depthMat);
                        })
                        .ObserveOnDispatcher()
                        .Subscribe(imgs =>
                        {
                            ColorFrame.Value = imgs.colorMat.ToBitmapSource();
                            DepthFrame.Value = imgs.depthMat.ToBitmapSource();
                        });
                }
            }
            // ストップするとき
            else
            {
                StartButtonFace.Value = "▶";
                ShutterButton.IsEnabled = false;
                _isConnected = false;
                if (_device == Device.Kinect)
                {
                    _kinectReceiver.Dispose();
                    _kinect.Dispose();
                }
                else if (_device == Device.Realsense)
                {
                    _realsenseReceiver.Dispose();
                    _realsense.Dispose();
                }
            }
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            // Kinect
            if (_device == Device.Kinect)
            {
                _kinect.StartStream()
                    .TakeWhile(imgs =>
                    {
                        if (imgs.ColorFrame == null) return true;
                        using (imgs.ColorFrame)
                        using (imgs.DepthFrame)
                        using (imgs.PointCloudFrame)
                        {
                            var colorMat = new Mat();
                            _kinectConverter.ImageToMat(imgs.ColorFrame, ref colorMat);
                            var depthMat = new Mat();
                            _kinectConverter.ImageToMat(imgs.DepthFrame, ref depthMat);
                            var pointCloudMat = new Mat();
                            _kinectConverter.ImageToMat(imgs.PointCloudFrame, ref pointCloudMat);
                            Yamashita.Kinect.Saving.SaveAsZip(SaveDir.Value, "", colorMat, depthMat, pointCloudMat);
                            return false;
                        }
                    })
                    .Subscribe();
            }
            // Realsense
            else if (_device == Device.Realsense)
            {
                _realsense.CaptureStream()
                    .TakeWhile(imgs =>
                    {
                        if (imgs.ColorFrame == null) return true;
                        using (imgs.ColorFrame)
                        using (imgs.DepthFrame)
                        {
                            var colorMat = new Mat();
                            _realsenseConverter.ToColorMat(imgs.ColorFrame, ref colorMat);
                            var depthMat = new Mat();
                            _realsenseConverter.ToDepthMat(imgs.DepthFrame, ref depthMat);
                            var pointCloudMat = new Mat();
                            _realsenseConverter.ToPointCloudMat(imgs.DepthFrame, ref pointCloudMat);
                            Yamashita.Realsense.Saving.SaveAsZip(SaveDir.Value, "", colorMat, depthMat, pointCloudMat);
                        }
                        return false;
                    })
                    .Subscribe();
            }
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

    }
}
