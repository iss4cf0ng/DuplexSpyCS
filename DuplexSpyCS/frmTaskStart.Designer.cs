namespace DuplexSpyCS
{
    partial class frmTaskStart
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
            label1 = new Label();
            radioButton1 = new RadioButton();
            radioButton2 = new RadioButton();
            textBox1 = new TextBox();
            button1 = new Button();
            textBox2 = new TextBox();
            label2 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(23, 54);
            label1.Name = "label1";
            label1.Size = new Size(71, 19);
            label1.TabIndex = 0;
            label1.Text = "FilePath :";
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(31, 12);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(63, 23);
            radioButton1.TabIndex = 1;
            radioButton1.TabStop = true;
            radioButton1.Text = "Local";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Location = new Point(130, 12);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(81, 23);
            radioButton2.TabIndex = 2;
            radioButton2.TabStop = true;
            radioButton2.Text = "Remote";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(106, 51);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(327, 27);
            textBox1.TabIndex = 3;
            // 
            // button1
            // 
            button1.Location = new Point(12, 117);
            button1.Name = "button1";
            button1.Size = new Size(421, 53);
            button1.TabIndex = 4;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(106, 84);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(327, 27);
            textBox2.TabIndex = 6;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 87);
            label2.Name = "label2";
            label2.Size = new Size(88, 19);
            label2.TabIndex = 5;
            label2.Text = "Parameter :";
            // 
            // frmTaskStart
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(451, 186);
            Controls.Add(textBox2);
            Controls.Add(label2);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(radioButton2);
            Controls.Add(radioButton1);
            Controls.Add(label1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmTaskStart";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmTaskStart";
            Load += frmTaskStart_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private TextBox textBox1;
        private Button button1;
        private TextBox textBox2;
        private Label label2;
    }
}