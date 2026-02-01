namespace DuplexSpyCS
{
    partial class frmListenerEdit
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
            comboBox1 = new ComboBox();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            checkBox1 = new CheckBox();
            label9 = new Label();
            textBox4 = new TextBox();
            button3 = new Button();
            button2 = new Button();
            textBox3 = new TextBox();
            label5 = new Label();
            tabPage4 = new TabPage();
            tabControl2 = new TabControl();
            tabPage3 = new TabPage();
            comboBox6 = new ComboBox();
            label12 = new Label();
            label7 = new Label();
            textBox7 = new TextBox();
            comboBox4 = new ComboBox();
            textBox5 = new TextBox();
            label11 = new Label();
            label10 = new Label();
            tabPage5 = new TabPage();
            comboBox7 = new ComboBox();
            comboBox5 = new ComboBox();
            comboBox3 = new ComboBox();
            comboBox2 = new ComboBox();
            label6 = new Label();
            label8 = new Label();
            label13 = new Label();
            label14 = new Label();
            button1 = new Button();
            textBox1 = new TextBox();
            label2 = new Label();
            label3 = new Label();
            textBox2 = new TextBox();
            numericUpDown1 = new NumericUpDown();
            label4 = new Label();
            menuStrip1 = new MenuStrip();
            toolStripMenuItem1 = new ToolStripMenuItem();
            tabControl1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage4.SuspendLayout();
            tabControl2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(15, 33);
            label1.Name = "label1";
            label1.Size = new Size(54, 19);
            label1.TabIndex = 0;
            label1.Text = "Proto :";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(75, 30);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(281, 27);
            comboBox1.TabIndex = 1;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Location = new Point(12, 200);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(344, 222);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(336, 190);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "TCP";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(checkBox1);
            tabPage2.Controls.Add(label9);
            tabPage2.Controls.Add(textBox4);
            tabPage2.Controls.Add(button3);
            tabPage2.Controls.Add(button2);
            tabPage2.Controls.Add(textBox3);
            tabPage2.Controls.Add(label5);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(336, 194);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "TLS";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(251, 41);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(67, 23);
            checkBox1.TabIndex = 13;
            checkBox1.Text = "Show";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(16, 42);
            label9.Name = "label9";
            label9.Size = new Size(47, 19);
            label9.TabIndex = 12;
            label9.Text = "Pass :";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(69, 39);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(176, 27);
            textBox4.TabIndex = 11;
            // 
            // button3
            // 
            button3.Location = new Point(20, 72);
            button3.Name = "button3";
            button3.Size = new Size(298, 45);
            button3.TabIndex = 10;
            button3.Text = "Create Cert";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button2
            // 
            button2.Location = new Point(251, 6);
            button2.Name = "button2";
            button2.Size = new Size(67, 27);
            button2.TabIndex = 9;
            button2.Text = "...";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(69, 6);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(176, 27);
            textBox3.TabIndex = 8;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(18, 10);
            label5.Name = "label5";
            label5.Size = new Size(45, 19);
            label5.TabIndex = 7;
            label5.Text = "Cert :";
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(tabControl2);
            tabPage4.Location = new Point(4, 28);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(336, 190);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "HTTP";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabControl2
            // 
            tabControl2.Controls.Add(tabPage3);
            tabControl2.Controls.Add(tabPage5);
            tabControl2.Dock = DockStyle.Fill;
            tabControl2.Location = new Point(0, 0);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new Size(336, 190);
            tabControl2.SizeMode = TabSizeMode.Fixed;
            tabControl2.TabIndex = 25;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(comboBox6);
            tabPage3.Controls.Add(label12);
            tabPage3.Controls.Add(label7);
            tabPage3.Controls.Add(textBox7);
            tabPage3.Controls.Add(comboBox4);
            tabPage3.Controls.Add(textBox5);
            tabPage3.Controls.Add(label11);
            tabPage3.Controls.Add(label10);
            tabPage3.Location = new Point(4, 28);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(328, 158);
            tabPage3.TabIndex = 0;
            tabPage3.Text = "Request";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // comboBox6
            // 
            comboBox6.FormattingEnabled = true;
            comboBox6.Location = new Point(85, 105);
            comboBox6.Name = "comboBox6";
            comboBox6.Size = new Size(237, 27);
            comboBox6.TabIndex = 25;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(31, 9);
            label12.Name = "label12";
            label12.Size = new Size(48, 19);
            label12.TabIndex = 24;
            label12.Text = "Host :";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(11, 42);
            label7.Name = "label7";
            label7.Size = new Size(71, 19);
            label7.TabIndex = 17;
            label7.Text = "Method :";
            // 
            // textBox7
            // 
            textBox7.Location = new Point(85, 6);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(237, 27);
            textBox7.TabIndex = 23;
            // 
            // comboBox4
            // 
            comboBox4.FormattingEnabled = true;
            comboBox4.Location = new Point(85, 39);
            comboBox4.Name = "comboBox4";
            comboBox4.Size = new Size(237, 27);
            comboBox4.TabIndex = 18;
            // 
            // textBox5
            // 
            textBox5.Location = new Point(85, 72);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(237, 27);
            textBox5.TabIndex = 19;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(42, 108);
            label11.Name = "label11";
            label11.Size = new Size(37, 19);
            label11.TabIndex = 21;
            label11.Text = "UA :";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(32, 75);
            label10.Name = "label10";
            label10.Size = new Size(47, 19);
            label10.TabIndex = 20;
            label10.Text = "Path :";
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(comboBox7);
            tabPage5.Controls.Add(comboBox5);
            tabPage5.Controls.Add(comboBox3);
            tabPage5.Controls.Add(comboBox2);
            tabPage5.Controls.Add(label6);
            tabPage5.Controls.Add(label8);
            tabPage5.Controls.Add(label13);
            tabPage5.Controls.Add(label14);
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(328, 162);
            tabPage5.TabIndex = 1;
            tabPage5.Text = "Response";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // comboBox7
            // 
            comboBox7.FormattingEnabled = true;
            comboBox7.Location = new Point(83, 106);
            comboBox7.Name = "comboBox7";
            comboBox7.Size = new Size(237, 27);
            comboBox7.TabIndex = 32;
            // 
            // comboBox5
            // 
            comboBox5.FormattingEnabled = true;
            comboBox5.Location = new Point(83, 40);
            comboBox5.Name = "comboBox5";
            comboBox5.Size = new Size(237, 27);
            comboBox5.TabIndex = 31;
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(83, 73);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(237, 27);
            comboBox3.TabIndex = 30;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(83, 7);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(237, 27);
            comboBox2.TabIndex = 29;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(18, 10);
            label6.Name = "label6";
            label6.Size = new Size(59, 19);
            label6.TabIndex = 28;
            label6.Text = "Status :";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(28, 76);
            label8.Name = "label8";
            label8.Size = new Size(49, 19);
            label8.TabIndex = 25;
            label8.Text = "Type :";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(25, 109);
            label13.Name = "label13";
            label13.Size = new Size(52, 19);
            label13.TabIndex = 27;
            label13.Text = "Body :";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(16, 43);
            label14.Name = "label14";
            label14.Size = new Size(61, 19);
            label14.TabIndex = 26;
            label14.Text = "Server :";
            // 
            // button1
            // 
            button1.Location = new Point(12, 429);
            button1.Name = "button1";
            button1.Size = new Size(344, 64);
            button1.TabIndex = 3;
            button1.Text = "Save";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(75, 63);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(280, 27);
            textBox1.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(11, 66);
            label2.Name = "label2";
            label2.Size = new Size(58, 19);
            label2.TabIndex = 5;
            label2.Text = "Name :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 129);
            label3.Name = "label3";
            label3.Size = new Size(49, 19);
            label3.TabIndex = 7;
            label3.Text = "Desc :";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(75, 129);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(280, 65);
            textBox2.TabIndex = 6;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(75, 96);
            numericUpDown1.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(280, 27);
            numericUpDown1.TabIndex = 8;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(24, 98);
            label4.Name = "label4";
            label4.Size = new Size(45, 19);
            label4.TabIndex = 9;
            label4.Text = "Port :";
            // 
            // menuStrip1
            // 
            menuStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            menuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1 });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(371, 27);
            menuStrip1.TabIndex = 10;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(53, 23);
            toolStripMenuItem1.Text = "Help";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            // 
            // frmListenerEdit
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(371, 505);
            Controls.Add(label4);
            Controls.Add(numericUpDown1);
            Controls.Add(label3);
            Controls.Add(textBox2);
            Controls.Add(label2);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Controls.Add(tabControl1);
            Controls.Add(comboBox1);
            Controls.Add(label1);
            Controls.Add(menuStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmListenerEdit";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmListenerEdit";
            Load += frmListenerEdit_Load;
            tabControl1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabControl2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ComboBox comboBox1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Button button1;
        private TextBox textBox1;
        private Label label2;
        private Label label3;
        private TextBox textBox2;
        private NumericUpDown numericUpDown1;
        private Label label4;
        private TabPage tabPage4;
        private CheckBox checkBox1;
        private Label label9;
        private TextBox textBox4;
        private Button button3;
        private Button button2;
        private TextBox textBox3;
        private Label label5;
        private Label label12;
        private TextBox textBox7;
        private Label label11;
        private Label label10;
        private TextBox textBox5;
        private ComboBox comboBox4;
        private Label label7;
        private TabControl tabControl2;
        private TabPage tabPage3;
        private TabPage tabPage5;
        private ComboBox comboBox3;
        private ComboBox comboBox2;
        private Label label6;
        private Label label8;
        private Label label13;
        private Label label14;
        private ComboBox comboBox5;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ComboBox comboBox6;
        private ComboBox comboBox7;
    }
}