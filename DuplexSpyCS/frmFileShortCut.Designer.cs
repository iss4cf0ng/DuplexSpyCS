namespace DuplexSpyCS
{
    partial class frmFileShortCut
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
            radioButton1 = new RadioButton();
            groupBox1 = new GroupBox();
            radioButton2 = new RadioButton();
            label1 = new Label();
            textBox1 = new TextBox();
            groupBox2 = new GroupBox();
            textBox2 = new TextBox();
            label2 = new Label();
            button1 = new Button();
            textBox3 = new TextBox();
            label3 = new Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(109, 35);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(56, 23);
            radioButton1.TabIndex = 0;
            radioButton1.TabStop = true;
            radioButton1.Text = "URL";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(radioButton2);
            groupBox1.Controls.Add(radioButton1);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(427, 81);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Type";
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Location = new Point(21, 35);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(51, 23);
            radioButton2.TabIndex = 1;
            radioButton2.TabStop = true;
            radioButton2.Text = "File";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 32);
            label1.Name = "label1";
            label1.Size = new Size(64, 19);
            label1.TabIndex = 2;
            label1.Text = "Source :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(76, 29);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(334, 27);
            textBox1.TabIndex = 3;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(textBox3);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(textBox2);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(textBox1);
            groupBox2.Controls.Add(label1);
            groupBox2.Location = new Point(12, 99);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(427, 134);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Attribute";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(76, 62);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(334, 27);
            textBox2.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(23, 65);
            label2.Name = "label2";
            label2.Size = new Size(47, 19);
            label2.TabIndex = 4;
            label2.Text = "Dest :";
            // 
            // button1
            // 
            button1.Location = new Point(12, 239);
            button1.Name = "button1";
            button1.Size = new Size(427, 67);
            button1.TabIndex = 5;
            button1.Text = "OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(76, 95);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(334, 27);
            textBox3.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(27, 98);
            label3.Name = "label3";
            label3.Size = new Size(43, 19);
            label3.TabIndex = 6;
            label3.Text = "Info :";
            // 
            // frmFileShortCut
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(451, 318);
            Controls.Add(button1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmFileShortCut";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmFileShortCut";
            Load += frmFileShortCut_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private RadioButton radioButton1;
        private GroupBox groupBox1;
        private RadioButton radioButton2;
        private Label label1;
        private TextBox textBox1;
        private GroupBox groupBox2;
        private TextBox textBox2;
        private Label label2;
        private Button button1;
        private TextBox textBox3;
        private Label label3;
    }
}