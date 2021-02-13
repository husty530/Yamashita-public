using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Yamashita.ML;
using Yamashita.Control;

namespace Samples.Tracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private IDisposable _connector;
        private readonly IYolo detector;

        public MainWindow()
        {
            InitializeComponent();
            detector = new Yolo(
                "..\\..\\..\\..\\YoloModel-tiny\\yolov4-tiny.cfg",
                "..\\..\\..\\..\\YoloModel-tiny\\coco.names",
                "..\\..\\..\\..\\YoloModel-tiny\\yolov4-tiny.weights",
                new OpenCvSharp.Size(640, 480),
                DrawingMode.Off
                );
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            _connector?.Dispose();
            var op = new OpenFileDialog();
            op.Filter = "Video(*.mp4, *.avi)|*.mp4;*.avi";
            if (op.ShowDialog() == true)
            {
                var tracker = new MultiTracker(0.2f, 7, 3);
                _connector = PlayVideo(op.FileName)
                    .Subscribe(frame =>
                    {
                        Cv2.Resize(frame, frame, new OpenCvSharp.Size(512, 288));
                        detector.Run(ref frame, out var results);
                        tracker.Update(ref frame, results.Select(r => (r.Label, r.Center, r.Size)).ToList(), out var _);
                        Dispatcher.Invoke(() => Image.Source = frame.ToBitmapSource());
                    });
            }
        }

        private void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            _connector?.Dispose();
            var tracker = new MultiTracker(0.2f, 7, 3);
            _connector = ConnectCamera(0)
                    .Subscribe(frame =>
                    {
                        Cv2.Resize(frame, frame, new OpenCvSharp.Size(512, 288));
                        detector.Run(ref frame, out var results);
                        tracker.Update(ref frame, results.Select(r => (r.Label, r.Center, r.Size)).ToList(), out var _);
                        Dispatcher.Invoke(() => Image.Source = frame.ToBitmapSource());
                    });
        }

        private IObservable<Mat> PlayVideo(string path)
        {
            var frame = new Mat();
            var cap = new VideoCapture(path);
            var observable = Observable.Range(0, cap.FrameCount, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    cap.Read(frame);
                    return frame;
                })
                .Where(frame => !frame.Empty())
                .Publish()
                .RefCount();
            return observable;
        }

        private IObservable<Mat> ConnectCamera(int index = 0)
        {
            var frame = new Mat();
            var cap = new VideoCapture(index);
            var observable = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    cap.Read(frame);
                    return frame;
                })
                .Where(frame => !frame.Empty())
                .Publish()
                .RefCount();
            return observable;
        }

    }
}
