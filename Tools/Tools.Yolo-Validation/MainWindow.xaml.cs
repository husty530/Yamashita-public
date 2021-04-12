using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Tools.Yolo_Validation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string selectedDir = "C:";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ModelDirButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = selectedDir,
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ModelDirButton.Content = cofd.FileName;
                selectedDir = cofd.FileName;
            }
        }

        private void ImageDirButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = selectedDir,
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ImageDirButton.Content = cofd.FileName;
                selectedDir = cofd.FileName;
            }
        }

        private void LabelDirButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = selectedDir,
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                LabelDirButton.Content = cofd.FileName;
                selectedDir = cofd.FileName;
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            ResultLabel.Content = "Result";
            var modelFolder = (string)ModelDirButton.Content;
            var imgDir = (string)ImageDirButton.Content;
            var labDir = (string)LabelDirButton.Content;
            var width = int.Parse(WidthTx.Text);
            var height = int.Parse(HeightTx.Text);
            var classNum = int.Parse(ClassNumTx.Text);
            var iouThresh = double.Parse(IouTx.Text);
            var response = Process.Init(modelFolder, width, height, classNum, iouThresh, imgDir, labDir);
            if (response != "Success")
            {
                ResultLabel.Content = response;
                return;
            }
            var r = Process.Go(SaveCheck.IsChecked);
            ResultLabel.Content = $"ClassName : {r.ClassName}\n";
            ResultLabel.Content += $"AverageTime : {r.AvgTime}\n";
            ResultLabel.Content += $"TP : {r.Tp}\n";
            ResultLabel.Content += $"FP : {r.Fp}\n";
            ResultLabel.Content += $"FN : {r.Fn}\n";
            ResultLabel.Content += $"Precision : {r.P:f3}\n";
            ResultLabel.Content += $"Recall : {r.R:f3}\n";
            ResultLabel.Content += $"AP : {r.Ap:f3}\n";
        }

    }
}
