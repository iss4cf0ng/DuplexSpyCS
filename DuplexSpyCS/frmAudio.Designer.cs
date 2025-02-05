namespace DuplexSpyCS
{
    partial class frmAudio
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            button8 = new Button();
            label2 = new Label();
            label1 = new Label();
            button1 = new Button();
            trackBar1 = new TrackBar();
            tabPage2 = new TabPage();
            button3 = new Button();
            textBox1 = new TextBox();
            button2 = new Button();
            tabPage3 = new TabPage();
            textBox4 = new TextBox();
            textBox5 = new TextBox();
            textBox3 = new TextBox();
            textBox2 = new TextBox();
            label6 = new Label();
            label5 = new Label();
            button9 = new Button();
            progressBar1 = new ProgressBar();
            button5 = new Button();
            button4 = new Button();
            label3 = new Label();
            comboBox1 = new ComboBox();
            tabPage4 = new TabPage();
            textBox6 = new TextBox();
            textBox7 = new TextBox();
            textBox8 = new TextBox();
            textBox9 = new TextBox();
            label7 = new Label();
            label8 = new Label();
            progressBar2 = new ProgressBar();
            button10 = new Button();
            button6 = new Button();
            button7 = new Button();
            label4 = new Label();
            comboBox2 = new ComboBox();
            tabPage5 = new TabPage();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(452, 239);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(button8);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(button1);
            tabPage1.Controls.Add(trackBar1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(444, 207);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "General";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            button8.Location = new Point(227, 111);
            button8.Name = "button8";
            button8.Size = new Size(209, 88);
            button8.TabIndex = 4;
            button8.Text = "Disable Mute";
            button8.UseVisualStyleBackColor = true;
            button8.Click += button8_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(175, 21);
            label2.Name = "label2";
            label2.Size = new Size(70, 19);
            label2.TabIndex = 3;
            label2.Text = "Volume :";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 21);
            label1.Name = "label1";
            label1.Size = new Size(59, 19);
            label1.TabIndex = 2;
            label1.Text = "Status :";
            // 
            // button1
            // 
            button1.Location = new Point(6, 111);
            button1.Name = "button1";
            button1.Size = new Size(215, 88);
            button1.TabIndex = 1;
            button1.Text = "Mute";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // trackBar1
            // 
            trackBar1.Location = new Point(6, 60);
            trackBar1.Maximum = 100;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(430, 45);
            trackBar1.TabIndex = 0;
            trackBar1.ValueChanged += trackBar1_ValueChanged;
            trackBar1.KeyUp += trackBar1_KeyUp;
            trackBar1.MouseUp += trackBar1_MouseUp;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(button3);
            tabPage2.Controls.Add(textBox1);
            tabPage2.Controls.Add(button2);
            tabPage2.Location = new Point(4, 28);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(444, 207);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Speech";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(8, 153);
            button3.Name = "button3";
            button3.Size = new Size(425, 46);
            button3.TabIndex = 2;
            button3.Text = "Play MP3";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(8, 6);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(425, 91);
            textBox1.TabIndex = 1;
            // 
            // button2
            // 
            button2.Location = new Point(8, 103);
            button2.Name = "button2";
            button2.Size = new Size(425, 44);
            button2.TabIndex = 0;
            button2.Text = "Speak Text";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(textBox4);
            tabPage3.Controls.Add(textBox5);
            tabPage3.Controls.Add(textBox3);
            tabPage3.Controls.Add(textBox2);
            tabPage3.Controls.Add(label6);
            tabPage3.Controls.Add(label5);
            tabPage3.Controls.Add(button9);
            tabPage3.Controls.Add(progressBar1);
            tabPage3.Controls.Add(button5);
            tabPage3.Controls.Add(button4);
            tabPage3.Controls.Add(label3);
            tabPage3.Controls.Add(comboBox1);
            tabPage3.Location = new Point(4, 28);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(444, 207);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Microphone";
            tabPage3.UseVisualStyleBackColor = true;
            tabPage3.Click += tabPage3_Click;
            // 
            // textBox4
            // 
            textBox4.Location = new Point(215, 125);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(221, 27);
            textBox4.TabIndex = 11;
            // 
            // textBox5
            // 
            textBox5.Location = new Point(215, 93);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(221, 27);
            textBox5.TabIndex = 10;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(109, 125);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(100, 27);
            textBox3.TabIndex = 9;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(109, 93);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(100, 27);
            textBox2.TabIndex = 8;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(9, 128);
            label6.Name = "label6";
            label6.Size = new Size(94, 19);
            label6.TabIndex = 7;
            label6.Text = "Online wav :";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(8, 96);
            label5.Name = "label5";
            label5.Size = new Size(95, 19);
            label5.TabIndex = 6;
            label5.Text = "Offline wav :";
            // 
            // button9
            // 
            button9.Location = new Point(297, 157);
            button9.Name = "button9";
            button9.Size = new Size(139, 42);
            button9.TabIndex = 5;
            button9.Text = "Download";
            button9.UseVisualStyleBackColor = true;
            button9.Click += button9_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(76, 39);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(360, 42);
            progressBar1.TabIndex = 4;
            // 
            // button5
            // 
            button5.Location = new Point(152, 157);
            button5.Name = "button5";
            button5.Size = new Size(139, 42);
            button5.TabIndex = 3;
            button5.Text = "Record";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button4
            // 
            button4.Location = new Point(6, 157);
            button4.Name = "button4";
            button4.Size = new Size(141, 42);
            button4.TabIndex = 2;
            button4.Text = "Start";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(8, 9);
            label3.Name = "label3";
            label3.Size = new Size(62, 19);
            label3.TabIndex = 1;
            label3.Text = "Device :";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(76, 6);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(360, 27);
            comboBox1.TabIndex = 0;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(textBox6);
            tabPage4.Controls.Add(textBox7);
            tabPage4.Controls.Add(textBox8);
            tabPage4.Controls.Add(textBox9);
            tabPage4.Controls.Add(label7);
            tabPage4.Controls.Add(label8);
            tabPage4.Controls.Add(progressBar2);
            tabPage4.Controls.Add(button10);
            tabPage4.Controls.Add(button6);
            tabPage4.Controls.Add(button7);
            tabPage4.Controls.Add(label4);
            tabPage4.Controls.Add(comboBox2);
            tabPage4.Location = new Point(4, 28);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(444, 207);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "System Audio";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // textBox6
            // 
            textBox6.Location = new Point(215, 122);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(221, 27);
            textBox6.TabIndex = 17;
            // 
            // textBox7
            // 
            textBox7.Location = new Point(215, 90);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(221, 27);
            textBox7.TabIndex = 16;
            // 
            // textBox8
            // 
            textBox8.Location = new Point(109, 122);
            textBox8.Name = "textBox8";
            textBox8.Size = new Size(100, 27);
            textBox8.TabIndex = 15;
            // 
            // textBox9
            // 
            textBox9.Location = new Point(109, 90);
            textBox9.Name = "textBox9";
            textBox9.Size = new Size(100, 27);
            textBox9.TabIndex = 14;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(9, 125);
            label7.Name = "label7";
            label7.Size = new Size(94, 19);
            label7.TabIndex = 13;
            label7.Text = "Online wav :";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(8, 93);
            label8.Name = "label8";
            label8.Size = new Size(95, 19);
            label8.TabIndex = 12;
            label8.Text = "Offline wav :";
            // 
            // progressBar2
            // 
            progressBar2.Location = new Point(76, 39);
            progressBar2.Name = "progressBar2";
            progressBar2.Size = new Size(362, 42);
            progressBar2.TabIndex = 9;
            // 
            // button10
            // 
            button10.Location = new Point(300, 159);
            button10.Name = "button10";
            button10.Size = new Size(141, 42);
            button10.TabIndex = 8;
            button10.Text = "Download";
            button10.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            button6.Location = new Point(153, 159);
            button6.Name = "button6";
            button6.Size = new Size(141, 42);
            button6.TabIndex = 7;
            button6.Text = "Record";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // button7
            // 
            button7.Location = new Point(6, 159);
            button7.Name = "button7";
            button7.Size = new Size(141, 42);
            button7.TabIndex = 6;
            button7.Text = "Start";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(8, 9);
            label4.Name = "label4";
            label4.Size = new Size(62, 19);
            label4.TabIndex = 5;
            label4.Text = "Device :";
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(76, 6);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(362, 27);
            comboBox2.TabIndex = 4;
            // 
            // tabPage5
            // 
            tabPage5.Location = new Point(4, 28);
            tabPage5.Name = "tabPage5";
            tabPage5.Size = new Size(444, 207);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "SysSound";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // frmAudio
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(452, 239);
            Controls.Add(tabControl1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmAudio";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmAudio";
            Load += frmAudio_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private Button button1;
        private TrackBar trackBar1;
        private TabPage tabPage2;
        private TextBox textBox1;
        private Button button2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private Label label1;
        private Button button3;
        private Label label2;
        private ComboBox comboBox1;
        private Label label3;
        private Button button5;
        private Button button4;
        private Button button6;
        private Button button7;
        private Label label4;
        private ComboBox comboBox2;
        private ProgressBar progressBar1;
        private Button button8;
        private Button button9;
        private Button button10;
        private ProgressBar progressBar2;
        private Label label6;
        private Label label5;
        private TextBox textBox4;
        private TextBox textBox5;
        private TextBox textBox3;
        private TextBox textBox2;
        private TextBox textBox6;
        private TextBox textBox7;
        private TextBox textBox8;
        private TextBox textBox9;
        private Label label7;
        private Label label8;
        private TabPage tabPage5;
    }
}