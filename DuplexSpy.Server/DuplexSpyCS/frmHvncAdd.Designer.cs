namespace DuplexSpyCS
{
    partial class frmHvncAdd
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
            button1 = new Button();
            label2 = new Label();
            label3 = new Label();
            textBox2 = new TextBox();
            textBox3 = new TextBox();
            comboBox1 = new ComboBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(48, 15);
            label1.Name = "label1";
            label1.Size = new Size(58, 19);
            label1.TabIndex = 0;
            label1.Text = "Name :";
            // 
            // button1
            // 
            button1.Location = new Point(10, 115);
            button1.Name = "button1";
            button1.Size = new Size(446, 51);
            button1.TabIndex = 2;
            button1.Text = "Save";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 48);
            label2.Name = "label2";
            label2.Size = new Size(94, 19);
            label2.TabIndex = 3;
            label2.Text = "Application :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(19, 81);
            label3.Name = "label3";
            label3.Size = new Size(87, 19);
            label3.TabIndex = 4;
            label3.Text = "Argument :";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(112, 45);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(344, 27);
            textBox2.TabIndex = 5;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(112, 78);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(344, 27);
            textBox3.TabIndex = 6;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(112, 12);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(344, 27);
            comboBox1.TabIndex = 7;
            // 
            // frmHvncAdd
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(466, 178);
            Controls.Add(comboBox1);
            Controls.Add(textBox3);
            Controls.Add(textBox2);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(button1);
            Controls.Add(label1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmHvncAdd";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmHvncAdd";
            Load += frmHvncAdd_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button button1;
        private Label label2;
        private Label label3;
        private TextBox textBox2;
        private TextBox textBox3;
        private ComboBox comboBox1;
    }
}