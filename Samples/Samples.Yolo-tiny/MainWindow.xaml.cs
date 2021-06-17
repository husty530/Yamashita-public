using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Yamashita.ML;

namespace Samples.Yolo_tiny
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        VideoCapture cap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var detector = new Yolo(
                "..\\..\\..\\..\\YoloModel-tiny\\yolov4-tiny.cfg",
                "..\\..\\..\\..\\YoloModel-tiny\\coco.names", 
                "..\\..\\..\\..\\YoloModel-tiny\\yolov4-tiny.weights", 
                new OpenCvSharp.Size(640, 480),
                DrawingMode.Off
            );
            var op = new OpenFileDialog { Filter = "Image or Video(*.png, *.jpg, *.mp4, *.avi)|*.png;*.jpg;*mp4;*.avi" };
            if (op.ShowDialog() == true)
            {
                cap?.Dispose();
                var file = op.FileName;
                if(Path.GetExtension(file) == ".mp4" || Path.GetExtension(file) == ".avi")
                {
                    cap = new VideoCapture(file);
                    Task.Run(() =>
                    {
                        var frame = new Mat();
                        while (cap.Read(frame))
                        {
                            detector.Run(ref frame, out var results);
                            results
                                .Where(r => r.Label == "person")
                                .Where(r => r.Confidence > 0.5)
                                .ToList()
                                .ForEach(r => Cv2.Rectangle(frame, r.Box, new Scalar(0, 0, 255), 2));
                            Dispatcher.Invoke(() => Image.Source = frame.ToBitmapSource());
                        }
                        
                    });
                }
                else
                {
                    var img = Cv2.ImRead(file);
                    detector.Run(ref img, out var results);
                    Image.Source = img.ToBitmapSource();
                }
            }
            else
            {
                cap?.Dispose();
                cap = new VideoCapture(0);
                Task.Run(() =>
                {
                    var frame = new Mat();
                    while (cap.Read(frame))
                    {
                        detector.Run(ref frame, out var results);
                        results
                            .Where(r => r.Label == "person")
                            .Where(r => r.Confidence > 0.5)
                            .ToList()
                            .ForEach(r => Cv2.Rectangle(frame, r.Box, new Scalar(0, 0, 255), 2));
                        Dispatcher.Invoke(() => Image.Source = frame.ToBitmapSource());
                    }

                });
            }
        }
    }
}
