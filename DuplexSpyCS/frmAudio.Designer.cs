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
            textBox1 = new TextBox();
            button2 = new Button();
            tabPage3 = new TabPage();
            label5 = new Label();
            textBox4 = new TextBox();
            button13 = new Button();
            textBox2 = new TextBox();
            checkBox1 = new CheckBox();
            button9 = new Button();
            progressBar1 = new ProgressBar();
            button5 = new Button();
            button4 = new Button();
            label3 = new Label();
            comboBox1 = new ComboBox();
            tabPage4 = new TabPage();
            label6 = new Label();
            textBox5 = new TextBox();
            button14 = new Button();
            textBox3 = new TextBox();
            checkBox2 = new CheckBox();
            progressBar2 = new ProgressBar();
            button10 = new Button();
            button6 = new Button();
            button7 = new Button();
            label4 = new Label();
            comboBox2 = new ComboBox();
            tabPage5 = new TabPage();
            button12 = new Button();
            button11 = new Button();
            numericUpDown1 = new NumericUpDown();
            comboBox3 = new ComboBox();
            label10 = new Label();
            label9 = new Label();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
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
            // textBox1
            // 
            textBox1.Location = new Point(8, 6);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(425, 143);
            textBox1.TabIndex = 1;
            // 
            // button2
            // 
            button2.Location = new Point(8, 155);
            button2.Name = "button2";
            button2.Size = new Size(425, 44);
            button2.TabIndex = 0;
            button2.Text = "Speak Text";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(label5);
            tabPage3.Controls.Add(textBox4);
            tabPage3.Controls.Add(button13);
            tabPage3.Controls.Add(textBox2);
            tabPage3.Controls.Add(checkBox1);
            tabPage3.Controls.Add(button9);
            tabPage3.Controls.Add(progressBar1);
            tabPage3.Controls.Add(button5);
            tabPage3.Controls.Add(button4);
            tabPage3.Controls.Add(label3);
            tabPage3.Controls.Add(comboBox1);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(444, 211);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Microphone";
            tabPage3.UseVisualStyleBackColor = true;
            tabPage3.Click += tabPage3_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(8, 128);
            label5.Name = "label5";
            label5.Size = new Size(70, 19);
            label5.TabIndex = 12;
            label5.Text = "Remote :";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(84, 125);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(352, 27);
            textBox4.TabIndex = 11;
            // 
            // button13
            // 
            button13.Location = new Point(343, 92);
            button13.Name = "button13";
            button13.Size = new Size(93, 27);
            button13.TabIndex = 10;
            button13.Text = "Refresh";
            button13.UseVisualStyleBackColor = true;
            button13.Click += button13_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(134, 92);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(203, 27);
            textBox2.TabIndex = 8;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(7, 95);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(121, 23);
            checkBox1.TabIndex = 7;
            checkBox1.Text = "Remote wav :";
            checkBox1.UseVisualStyleBackColor = true;
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
            tabPage4.Controls.Add(label6);
            tabPage4.Controls.Add(textBox5);
            tabPage4.Controls.Add(button14);
            tabPage4.Controls.Add(textBox3);
            tabPage4.Controls.Add(checkBox2);
            tabPage4.Controls.Add(progressBar2);
            tabPage4.Controls.Add(button10);
            tabPage4.Controls.Add(button6);
            tabPage4.Controls.Add(button7);
            tabPage4.Controls.Add(label4);
            tabPage4.Controls.Add(comboBox2);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(444, 211);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "System Audio";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(10, 130);
            label6.Name = "label6";
            label6.Size = new Size(70, 19);
            label6.TabIndex = 16;
            label6.Text = "Remote :";
            // 
            // textBox5
            // 
            textBox5.Location = new Point(86, 127);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(352, 27);
            textBox5.TabIndex = 15;
            // 
            // button14
            // 
            button14.Location = new Point(345, 94);
            button14.Name = "button14";
            button14.Size = new Size(93, 27);
            button14.TabIndex = 14;
            button14.Text = "Refresh";
            button14.UseVisualStyleBackColor = true;
            button14.Click += button14_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(135, 94);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(204, 27);
            textBox3.TabIndex = 12;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(8, 97);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(121, 23);
            checkBox2.TabIndex = 11;
            checkBox2.Text = "Remote wav :";
            checkBox2.UseVisualStyleBackColor = true;
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
            button10.Click += button10_Click;
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
            tabPage5.Controls.Add(button12);
            tabPage5.Controls.Add(button11);
            tabPage5.Controls.Add(numericUpDown1);
            tabPage5.Controls.Add(comboBox3);
            tabPage5.Controls.Add(label10);
            tabPage5.Controls.Add(label9);
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Size = new Size(444, 211);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "SysSound";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // button12
            // 
            button12.Location = new Point(230, 143);
            button12.Name = "button12";
            button12.Size = new Size(206, 56);
            button12.TabIndex = 5;
            button12.Text = "Go";
            button12.UseVisualStyleBackColor = true;
            button12.Click += button12_Click;
            // 
            // button11
            // 
            button11.Location = new Point(8, 143);
            button11.Name = "button11";
            button11.Size = new Size(206, 56);
            button11.TabIndex = 4;
            button11.Text = "Test";
            button11.UseVisualStyleBackColor = true;
            button11.Click += button11_Click;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(94, 57);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(213, 27);
            numericUpDown1.TabIndex = 3;
            numericUpDown1.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Items.AddRange(new object[] { "Asterisk", "Beep", "Exclamation", "Hand", "Question" });
            comboBox3.Location = new Point(93, 19);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(214, 27);
            comboBox3.TabIndex = 2;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(22, 59);
            label10.Name = "label10";
            label10.Size = new Size(57, 19);
            label10.TabIndex = 1;
            label10.Text = "Times :";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(17, 22);
            label9.Name = "label9";
            label9.Size = new Size(62, 19);
            label9.TabIndex = 0;
            label9.Text = "Sound :";
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
            FormClosed += frmAudio_FormClosed;
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
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
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
        private TabPage tabPage5;
        private Label label9;
        private Button button12;
        private Button button11;
        private NumericUpDown numericUpDown1;
        private ComboBox comboBox3;
        private Label label10;
        private CheckBox checkBox1;
        private Button button13;
        private TextBox textBox2;
        private Button button14;
        private TextBox textBox3;
        private CheckBox checkBox2;
        private Label label5;
        private TextBox textBox4;
        private Label label6;
        private TextBox textBox5;
    }
}