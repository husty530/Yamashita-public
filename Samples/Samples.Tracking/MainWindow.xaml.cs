using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
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
        private VideoCapture cap;
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
            cap?.Dispose();
            var op = new OpenFileDialog();
            op.Filter = "Video(*.mp4, *.avi)|*.mp4;*.avi";
            if (op.ShowDialog() == true)
            {
                cap = new VideoCapture(op.FileName);
                var tracker = new MultiTracker(0.2f, 7, 3);
                Task.Run(() =>
                {
                    var frame = new Mat();
                    while (!cap.IsDisposed)
                    {
                        cap.Read(frame);
                        if (frame.Empty()) break;
                        Cv2.Resize(frame, frame, new OpenCvSharp.Size(512, 288));
                        detector.Run(ref frame, out var results);
                        tracker.Update(ref frame, results.Select(r => (r.Label, r.Center, r.Size)).ToList(), out var _);
                        Cv2.ImShow(" ", frame);
                        Cv2.WaitKey(1);
                        //Dispatcher.Invoke(() => Image.Source = frame.ToBitmapSource());
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
                while (!cap.IsDisposed)
                {
                    cap.Read(frame);
                    if (frame.Empty()) break;
                    detector.Run(ref frame, out var yolo);
                    var inputs = new List<(string, OpenCvSharp.Point, OpenCvSharp.Size)>();
                    for (int i = 0; i < yolo.Count; i++) inputs.Add((yolo[i].Label, yolo[i].Center, yolo[i].Size));
                    tracker.Update(ref frame, inputs, out var results);
                    Dispatcher.Invoke(() => Image.Source = frame.ToBitmapSource());
                }
            });
        }
    }
}
