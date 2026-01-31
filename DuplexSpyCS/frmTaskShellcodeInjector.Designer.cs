namespace DuplexSpyCS
{
    partial class frmTaskShellcodeInjector
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
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            tabPage2 = new TabPage();
            richTextBox1 = new RichTextBox();
            button2 = new Button();
            radioButton1 = new RadioButton();
            radioButton2 = new RadioButton();
            textBox1 = new TextBox();
            button1 = new Button();
            button3 = new Button();
            label1 = new Label();
            comboBox1 = new ComboBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            statusStrip1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(1, 155);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(415, 359);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(statusStrip1);
            tabPage1.Controls.Add(listView1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(407, 327);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Process";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(3, 300);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(401, 24);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(158, 19);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView1.Dock = DockStyle.Fill;
            listView1.FullRowSelect = true;
            listView1.Location = new Point(3, 3);
            listView1.Name = "listView1";
            listView1.Size = new Size(401, 321);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Process";
            columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "PID";
            columnHeader2.Width = 200;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(richTextBox1);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(407, 331);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Logs";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(3, 3);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(401, 325);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // button2
            // 
            button2.Location = new Point(12, 111);
            button2.Name = "button2";
            button2.Size = new Size(397, 38);
            button2.TabIndex = 8;
            button2.Text = "Inject";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(32, 41);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(51, 23);
            radioButton1.TabIndex = 9;
            radioButton1.TabStop = true;
            radioButton1.Text = "File";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Location = new Point(32, 76);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(55, 23);
            radioButton2.TabIndex = 10;
            radioButton2.TabStop = true;
            radioButton2.Text = "Text";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(93, 39);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(209, 27);
            textBox1.TabIndex = 11;
            // 
            // button1
            // 
            button1.Location = new Point(308, 39);
            button1.Name = "button1";
            button1.Size = new Size(99, 27);
            button1.TabIndex = 12;
            button1.Text = "...";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button3
            // 
            button3.Location = new Point(93, 70);
            button3.Name = "button3";
            button3.Size = new Size(314, 35);
            button3.TabIndex = 13;
            button3.Text = "...";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 9);
            label1.Name = "label1";
            label1.Size = new Size(71, 19);
            label1.TabIndex = 14;
            label1.Text = "Method :";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "APC", "EarlyBird", "CreateRemoteThread" });
            comboBox1.Location = new Point(93, 6);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(314, 27);
            comboBox1.TabIndex = 15;
            // 
            // frmTaskShellcodeInjector
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(419, 517);
            Controls.Add(comboBox1);
            Controls.Add(label1);
            Controls.Add(button3);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(radioButton2);
            Controls.Add(radioButton1);
            Controls.Add(button2);
            Controls.Add(tabControl1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            Name = "frmTaskShellcodeInjector";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmTaskShellcodeInjector";
            FormClosed += frmTaskShellcodeInjector_FormClosed;
            Load += frmTaskShellcodeInjector_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tabPage2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ListView listView1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private TabPage tabPage2;
        private RichTextBox richTextBox1;
        private Button button2;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private TextBox textBox1;
        private Button button1;
        private Button button3;
        private Label label1;
        private ComboBox comboBox1;
    }
}