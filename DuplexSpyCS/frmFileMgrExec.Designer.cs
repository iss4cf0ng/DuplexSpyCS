namespace DuplexSpyCS
{
    partial class frmFileMgrExec
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
            label2 = new Label();
            checkBox1 = new CheckBox();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            button1 = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 21);
            label1.Name = "label1";
            label1.Size = new Size(105, 19);
            label1.TabIndex = 0;
            label1.Text = "Remote Path :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(29, 60);
            label2.Name = "label2";
            label2.Size = new Size(88, 19);
            label2.TabIndex = 1;
            label2.Text = "Parameter :";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(12, 102);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(67, 23);
            checkBox1.TabIndex = 2;
            checkBox1.Text = "runas";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(123, 19);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(282, 27);
            textBox1.TabIndex = 3;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(123, 57);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(282, 27);
            textBox2.TabIndex = 4;
            // 
            // button1
            // 
            button1.Location = new Point(123, 90);
            button1.Name = "button1";
            button1.Size = new Size(282, 44);
            button1.TabIndex = 5;
            button1.Text = "Run";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // frmFileMgrExec
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(419, 142);
            Controls.Add(button1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(checkBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmFileMgrExec";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmFileMgrExec";
            Load += frmFileMgrExec_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private CheckBox checkBox1;
        private TextBox textBox1;
        private TextBox textBox2;
        private Button button1;
    }
}