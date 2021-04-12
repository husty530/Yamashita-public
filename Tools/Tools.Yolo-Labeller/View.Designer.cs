
namespace Tools.Yolo_Labeller
{
    partial class View
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.StartButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.UndoButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ProgressCount_label = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.comboBox = new System.Windows.Forms.ComboBox();
            this.WidthTx = new System.Windows.Forms.TextBox();
            this.HeightTx = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(12, 12);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(82, 50);
            this.StartButton.TabIndex = 0;
            this.StartButton.Text = "Open";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // BackButton
            // 
            this.BackButton.Location = new System.Drawing.Point(325, 11);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(77, 50);
            this.BackButton.TabIndex = 1;
            this.BackButton.Text = "◀(A)";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // NextButton
            // 
            this.NextButton.Location = new System.Drawing.Point(408, 11);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(77, 50);
            this.NextButton.TabIndex = 2;
            this.NextButton.Text = "▶(D)";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(491, 11);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(77, 50);
            this.SaveButton.TabIndex = 3;
            this.SaveButton.Text = "Save(S)";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // UndoButton
            // 
            this.UndoButton.Location = new System.Drawing.Point(574, 11);
            this.UndoButton.Name = "UndoButton";
            this.UndoButton.Size = new System.Drawing.Size(77, 50);
            this.UndoButton.TabIndex = 4;
            this.UndoButton.Text = "Undo(X)";
            this.UndoButton.UseVisualStyleBackColor = true;
            this.UndoButton.Click += new System.EventHandler(this.UndoButton_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(657, 11);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(77, 50);
            this.ClearButton.TabIndex = 5;
            this.ClearButton.Text = "Clear(C)";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(100, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Size";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Progress";
            // 
            // ProgressCount_label
            // 
            this.ProgressCount_label.AutoSize = true;
            this.ProgressCount_label.Location = new System.Drawing.Point(273, 29);
            this.ProgressCount_label.Name = "ProgressCount_label";
            this.ProgressCount_label.Size = new System.Drawing.Size(30, 15);
            this.ProgressCount_label.TabIndex = 8;
            this.ProgressCount_label.Text = "0 / 0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(754, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "Selected Class";
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(13, 69);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(100, 50);
            this.pictureBox.TabIndex = 12;
            this.pictureBox.TabStop = false;
            this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
            this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            this.pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseUp);
            // 
            // comboBox
            // 
            this.comboBox.FormattingEnabled = true;
            this.comboBox.Location = new System.Drawing.Point(754, 30);
            this.comboBox.Name = "comboBox";
            this.comboBox.Size = new System.Drawing.Size(121, 23);
            this.comboBox.TabIndex = 13;
            this.comboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboBox_KeyPress);
            // 
            // WidthTx
            // 
            this.WidthTx.Location = new System.Drawing.Point(134, 26);
            this.WidthTx.Name = "WidthTx";
            this.WidthTx.Size = new System.Drawing.Size(29, 23);
            this.WidthTx.TabIndex = 14;
            this.WidthTx.Text = "640";
            // 
            // HeightTx
            // 
            this.HeightTx.Location = new System.Drawing.Point(169, 26);
            this.HeightTx.Name = "HeightTx";
            this.HeightTx.Size = new System.Drawing.Size(29, 23);
            this.HeightTx.TabIndex = 15;
            this.HeightTx.Text = "480";
            // 
            // View
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(899, 607);
            this.Controls.Add(this.HeightTx);
            this.Controls.Add(this.WidthTx);
            this.Controls.Add(this.comboBox);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ProgressCount_label);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.UndoButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.StartButton);
            this.KeyPreview = true;
            this.Name = "View";
            this.Text = "Yolo-Labeller";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.View_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button UndoButton;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label ProgressCount_label;
        private System.Windows.Forms.ComboBox comboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox WidthTx;
        private System.Windows.Forms.TextBox HeightTx;
    }
}

