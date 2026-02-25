namespace DuplexSpyCS
{
    partial class frmSSLCert
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
            button1 = new Button();
            numericUpDown1 = new NumericUpDown();
            label4 = new Label();
            textBox2 = new TextBox();
            label3 = new Label();
            comboBox1 = new ComboBox();
            label2 = new Label();
            textBox1 = new TextBox();
            label1 = new Label();
            checkBox1 = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 144);
            button1.Name = "button1";
            button1.Size = new Size(288, 49);
            button1.TabIndex = 1;
            button1.Text = "Create";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(103, 110);
            numericUpDown1.Margin = new Padding(4);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(197, 27);
            numericUpDown1.TabIndex = 17;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(42, 112);
            label4.Name = "label4";
            label4.Size = new Size(54, 19);
            label4.TabIndex = 16;
            label4.Text = "Years :";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(102, 77);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(131, 27);
            textBox2.TabIndex = 15;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 80);
            label3.Name = "label3";
            label3.Size = new Size(84, 19);
            label3.TabIndex = 14;
            label3.Text = "Password :";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "2048", "4096" });
            comboBox1.Location = new Point(102, 44);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(198, 27);
            comboBox1.TabIndex = 13;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(19, 47);
            label2.Name = "label2";
            label2.Size = new Size(77, 19);
            label2.TabIndex = 12;
            label2.Text = "RSA Size :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(102, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(198, 27);
            textBox1.TabIndex = 11;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(38, 15);
            label1.Name = "label1";
            label1.Size = new Size(58, 19);
            label1.TabIndex = 10;
            label1.Text = "Name :";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(239, 77);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(67, 23);
            checkBox1.TabIndex = 18;
            checkBox1.Text = "Show";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // frmSSLCert
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(312, 203);
            Controls.Add(checkBox1);
            Controls.Add(numericUpDown1);
            Controls.Add(label4);
            Controls.Add(textBox2);
            Controls.Add(label3);
            Controls.Add(comboBox1);
            Controls.Add(label2);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(button1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            Name = "frmSSLCert";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmSSLCert";
            Load += frmSSLCert_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button button1;
        private NumericUpDown numericUpDown1;
        private Label label4;
        private TextBox textBox2;
        private Label label3;
        private ComboBox comboBox1;
        private Label label2;
        private TextBox textBox1;
        private Label label1;
        private CheckBox checkBox1;
    }
}