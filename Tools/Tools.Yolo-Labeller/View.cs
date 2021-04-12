using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenCvSharp.Extensions;

namespace Tools.Yolo_Labeller
{
    public partial class View : Form
    {
        private bool _mouseMove;
        private string[] _labels;

        public View()
        {
            InitializeComponent();
            BackButton.Enabled = false;
            NextButton.Enabled = false;
            SaveButton.Enabled = false;
            UndoButton.Enabled = false;
            ClearButton.Enabled = false;
            _mouseMove = false;
            var labelPath = $"..\\..\\..\\classes.txt";
            _labels = File.ReadAllLines(labelPath).ToArray();
            comboBox.Text = _labels[0];
            comboBox.Items.AddRange(_labels.ToArray());
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            var fbd1 = new FolderBrowserDialog();
            fbd1.Description = "画像フォルダを選択";
            if (fbd1.ShowDialog() == DialogResult.OK)
            {
                var fbd2 = new FolderBrowserDialog();
                fbd2.Description = "保存先を選択";
                if (fbd2.ShowDialog() == DialogResult.OK)
                {
                    var img = Process.Initialize(fbd1.SelectedPath, fbd2.SelectedPath, _labels, int.Parse(WidthTx.Text), int.Parse(HeightTx.Text));
                    pictureBox.Width = img.Width;
                    pictureBox.Height = img.Height;
                    pictureBox.Image = img.ToBitmap();
                    pictureBox.Enabled = true;
                    ProgressCount_label.Text = $"{Process.FrameNumber + 1} / {Process.FileCount}";
                    BackButton.Enabled = false;
                    NextButton.Enabled = false;
                    SaveButton.Enabled = false;
                    UndoButton.Enabled = false;
                    ClearButton.Enabled = false;
                    if (Process.FrameNumber != Process.FileCount - 1) NextButton.Enabled = true;
                }
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            pictureBox.Image = Process.GoBack().ToBitmap();
            ProgressCount_label.Text = $"{Process.FrameNumber + 1} / {Process.FileCount}";
            NextButton.Enabled = true;
            if (Process.FrameNumber == 0) BackButton.Enabled = false;
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            pictureBox.Image = Process.GoNext().ToBitmap();
            ProgressCount_label.Text = $"{Process.FrameNumber + 1} / {Process.FileCount}";
            BackButton.Enabled = true;
            if (Process.FrameNumber == Process.FileCount - 1) NextButton.Enabled = false;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            Process.Save();
            SaveButton.Enabled = false;
            if (Process.FrameNumber != 0) BackButton.Enabled = true;
            if (Process.FrameNumber != Process.FileCount - 1) NextButton.Enabled = true;
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            pictureBox.Image = Process.RemoveLast().ToBitmap();
            SaveButton.Enabled = true;
            if (Process.RectCount == 0)
            {
                UndoButton.Enabled = false;
                ClearButton.Enabled = false;
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            pictureBox.Image = Process.Clear().ToBitmap();
            UndoButton.Enabled = false;
            ClearButton.Enabled = false;
            SaveButton.Enabled = false;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseMove = true;
            pictureBox.Image = Process.SelectStart(e.X, e.Y).ToBitmap();
            BackButton.Enabled = false;
            NextButton.Enabled = false;
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseMove) pictureBox.Image = Process.Drag(e.X, e.Y).ToBitmap();
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_mouseMove) return;
            _mouseMove = false;
            var index = comboBox.SelectedIndex;
            if (index == -1) index = 0;
            pictureBox.Image = Process.SelectGoal(e.X, e.Y, index).ToBitmap();
            UndoButton.Enabled = true;
            ClearButton.Enabled = true;
            SaveButton.Enabled = true;
        }

        private void View_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar.ToString())
            {
                case "a":
                    if (BackButton.Enabled == false) break;
                    pictureBox.Image = Process.GoBack().ToBitmap();
                    ProgressCount_label.Text = $"{Process.FrameNumber + 1} / {Process.FileCount}";
                    NextButton.Enabled = true;
                    if (Process.FrameNumber == 0) BackButton.Enabled = false;
                    break;
                case "d":
                    if (NextButton.Enabled == false) break;
                    pictureBox.Image = Process.GoNext().ToBitmap();
                    ProgressCount_label.Text = $"{Process.FrameNumber + 1} / {Process.FileCount}";
                    BackButton.Enabled = true;
                    if (Process.FrameNumber == Process.FileCount - 1) NextButton.Enabled = false;
                    break;
                case "s":
                    if (SaveButton.Enabled == false) break;
                    Process.Save();
                    SaveButton.Enabled = false;
                    if (Process.FrameNumber != 0) BackButton.Enabled = true;
                    if (Process.FrameNumber != Process.FileCount - 1) NextButton.Enabled = true;
                    break;
                case "c":
                    if (ClearButton.Enabled == false) break;
                    pictureBox.Image = Process.Clear().ToBitmap();
                    UndoButton.Enabled = false;
                    ClearButton.Enabled = false;
                    SaveButton.Enabled = false;
                    break;
                case "x":
                    if (UndoButton.Enabled == false) break;
                    pictureBox.Image = Process.RemoveLast().ToBitmap();
                    SaveButton.Enabled = true;
                    if (Process.RectCount == 0)
                    {
                        UndoButton.Enabled = false;
                        ClearButton.Enabled = false;
                    }
                    break;
            }
        }

        private void comboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }
    }
}
