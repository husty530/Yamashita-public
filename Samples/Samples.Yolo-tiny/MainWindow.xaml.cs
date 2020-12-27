using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Yamashita.Yolo;

namespace Samples.Yolo_tiny
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var detector = new Yolo("yolov4-tiny.cfg", "coco.names", "yolov4-tiny.weights", new OpenCvSharp.Size(640, 480));
            var op = new OpenFileDialog();
            op.Filter = "Image or Video(*.png, *.jpg, *.mp4)|*.png;*.jpg;*mp4";
            if(op.ShowDialog() == true)
            {
                var file = op.FileName;
                if(Path.GetExtension(file) == ".mp4")
                {
                    var v = new VideoCapture(file);
                    var frame = new Mat();
                    Task.Run(() =>
                    {
                        while (v.Read(frame))
                        {
                            detector.Run(ref frame, out var results);
                            Dispatcher.Invoke(() =>
                            {
                                Image.Source = frame.ToBitmapSource();
                            });
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
        }
    }
}
