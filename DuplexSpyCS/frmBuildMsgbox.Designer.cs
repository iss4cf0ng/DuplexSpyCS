namespace DuplexSpyCS
{
    partial class frmBuildMsgbox
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
            pictureBox1 = new PictureBox();
            label1 = new Label();
            textBox1 = new TextBox();
            comboBox1 = new ComboBox();
            button1 = new Button();
            label2 = new Label();
            textBox2 = new TextBox();
            label3 = new Label();
            label4 = new Label();
            comboBox2 = new ComboBox();
            button2 = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(362, 71);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(60, 60);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(70, 19);
            label1.TabIndex = 1;
            label1.Text = "Caption :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(91, 6);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(331, 27);
            textBox1.TabIndex = 2;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "None", "Error", "Hand", "Stop", "Question", "Exclamation", "Warning", "Asterisk", "Information" });
            comboBox1.Location = new Point(91, 105);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(265, 27);
            comboBox1.TabIndex = 3;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // button1
            // 
            button1.Location = new Point(12, 138);
            button1.Name = "button1";
            button1.Size = new Size(198, 52);
            button1.TabIndex = 4;
            button1.Text = "Test";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(37, 108);
            label2.Name = "label2";
            label2.Size = new Size(45, 19);
            label2.TabIndex = 5;
            label2.Text = "Icon :";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(91, 39);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(331, 27);
            textBox2.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(38, 42);
            label3.Name = "label3";
            label3.Size = new Size(44, 19);
            label3.TabIndex = 6;
            label3.Text = "Text :";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(20, 75);
            label4.Name = "label4";
            label4.Size = new Size(62, 19);
            label4.TabIndex = 9;
            label4.Text = "Button :";
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Items.AddRange(new object[] { "OK", "OKCancel", "RetryCancel", "YesNo", "YesNoCancel", "AbortRetryIgnore", "CancelTryContinue" });
            comboBox2.Location = new Point(91, 72);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(265, 27);
            comboBox2.TabIndex = 8;
            // 
            // button2
            // 
            button2.Location = new Point(225, 137);
            button2.Name = "button2";
            button2.Size = new Size(198, 52);
            button2.TabIndex = 10;
            button2.Text = "Apply";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // frmBuildMsgbox
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(435, 201);
            Controls.Add(button2);
            Controls.Add(label4);
            Controls.Add(comboBox2);
            Controls.Add(textBox2);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(button1);
            Controls.Add(comboBox1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmBuildMsgbox";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmBuildMsgbox";
            Load += frmBuildMsgbox_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Label label1;
        private TextBox textBox1;
        private ComboBox comboBox1;
        private Button button1;
        private Label label2;
        private TextBox textBox2;
        private Label label3;
        private Label label4;
        private ComboBox comboBox2;
        private Button button2;
    }
}