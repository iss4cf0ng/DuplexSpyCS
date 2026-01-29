namespace DuplexSpyCS
{
    partial class frmTaskDLLInjector
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
            textBox1 = new TextBox();
            label1 = new Label();
            label2 = new Label();
            textBox2 = new TextBox();
            button2 = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            textBox4 = new TextBox();
            tabPage2 = new TabPage();
            textBox5 = new TextBox();
            textBox3 = new TextBox();
            label4 = new Label();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(313, 10);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(96, 29);
            button1.TabIndex = 0;
            button1.Text = "...";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(63, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(243, 27);
            textBox1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(17, 15);
            label1.Name = "label1";
            label1.Size = new Size(40, 19);
            label1.TabIndex = 2;
            label1.Text = "File :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 9);
            label2.Name = "label2";
            label2.Size = new Size(40, 19);
            label2.TabIndex = 4;
            label2.Text = "PID :";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(52, 6);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(325, 27);
            textBox2.TabIndex = 3;
            // 
            // button2
            // 
            button2.Location = new Point(17, 212);
            button2.Name = "button2";
            button2.Size = new Size(392, 38);
            button2.TabIndex = 5;
            button2.Text = "Inject";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(12, 45);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(396, 161);
            tabControl1.TabIndex = 6;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(textBox4);
            tabPage1.Controls.Add(textBox2);
            tabPage1.Controls.Add(label2);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(388, 129);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Native DLL";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // textBox4
            // 
            textBox4.Location = new Point(6, 39);
            textBox4.Multiline = true;
            textBox4.Name = "textBox4";
            textBox4.ReadOnly = true;
            textBox4.Size = new Size(371, 84);
            textBox4.TabIndex = 5;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(textBox5);
            tabPage2.Controls.Add(textBox3);
            tabPage2.Controls.Add(label4);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(383, 133);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "CSharp DLL";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBox5
            // 
            textBox5.Location = new Point(6, 39);
            textBox5.Multiline = true;
            textBox5.Name = "textBox5";
            textBox5.ReadOnly = true;
            textBox5.Size = new Size(371, 84);
            textBox5.TabIndex = 7;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(67, 6);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(310, 27);
            textBox3.TabIndex = 5;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 9);
            label4.Name = "label4";
            label4.Size = new Size(49, 19);
            label4.TabIndex = 6;
            label4.Text = "Args :";
            // 
            // frmTaskDLLInjector
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 262);
            Controls.Add(tabControl1);
            Controls.Add(button2);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmTaskDLLInjector";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmTaskDLLInjector";
            Load += frmTaskDLLInjector_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox textBox1;
        private Label label1;
        private Label label2;
        private TextBox textBox2;
        private Button button2;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TextBox textBox3;
        private Label label4;
        private TextBox textBox4;
        private TextBox textBox5;
    }
}