using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Yamashita.Yolo;
using Yamashita.MultiTracker;

namespace Samples.Tracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        VideoCapture cap;
        Yolo detector;

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
            var op = new OpenFileDialog();
            op.Filter = "Video(*.mp4, *.avi)|*.mp4;*.avi";
            if (op.ShowDialog() == true)
            {
                cap?.Dispose();
                cap = new VideoCapture(op.FileName);
                var tracker = new MultiTracker(0.2f, 7, 3);
                Task.Run(() =>
                {
                    var frame = new Mat();
                    while (cap.Read(frame))
                    {
                        detector.Run(ref frame, out var yoloResults);
                        tracker.Update(ref frame, yoloResults, out var results);
                        Dispatcher.Invoke(() =>
                        {
                            Image.Source = frame.ToBitmapSource();
                        });
                    }
                });
            }
        }

        private void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            cap?.Dispose();
            cap = new VideoCapture(0);
            var tracker = new MultiTracker(0.2f, 7, 3);
            Task.Run(() =>
            {
                var frame = new Mat();
                while (cap.Read(frame))
                {
                    detector.Run(ref frame, out var yoloResults);
                    tracker.Update(ref frame, yoloResults, out var results);
                    Dispatcher.Invoke(() =>
                    {
                        Image.Source = frame.ToBitmapSource();
                    });
                }
            });
        }
    }
}
