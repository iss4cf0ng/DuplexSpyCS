namespace DuplexSpyCS
{
    partial class frmLogs
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogs));
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            richTextBox1 = new RichTextBox();
            textBox1 = new TextBox();
            tabPage2 = new TabPage();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            textBox2 = new TextBox();
            tabPage3 = new TabPage();
            richTextBox2 = new RichTextBox();
            textBox3 = new TextBox();
            tabPage4 = new TabPage();
            richTextBox3 = new RichTextBox();
            textBox4 = new TextBox();
            toolStrip1 = new ToolStrip();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            toolStripMenuItem1 = new ToolStripMenuItem();
            toolStripButton2 = new ToolStripButton();
            toolStripButton1 = new ToolStripButton();
            timer1 = new System.Windows.Forms.Timer(components);
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Location = new Point(0, 28);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(922, 435);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(richTextBox1);
            tabPage1.Controls.Add(textBox1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(914, 403);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "System Logs";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(3, 3);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(908, 370);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Bottom;
            textBox1.Location = new Point(3, 373);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(908, 27);
            textBox1.TabIndex = 1;
            textBox1.TextChanged += textBox1_TextChanged;
            textBox1.KeyDown += textBox1_KeyDown;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(listView1);
            tabPage2.Controls.Add(textBox2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(914, 407);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Key Exchange";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            listView1.Dock = DockStyle.Fill;
            listView1.Location = new Point(3, 3);
            listView1.Name = "listView1";
            listView1.Size = new Size(908, 374);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Remote Host";
            columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Message";
            columnHeader2.Width = 300;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Creation Date";
            columnHeader3.Width = 150;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Last Update Date";
            columnHeader4.Width = 150;
            // 
            // textBox2
            // 
            textBox2.Dock = DockStyle.Bottom;
            textBox2.Location = new Point(3, 377);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(908, 27);
            textBox2.TabIndex = 1;
            textBox2.TextChanged += textBox2_TextChanged;
            textBox2.KeyDown += textBox2_KeyDown;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(richTextBox2);
            tabPage3.Controls.Add(textBox3);
            tabPage3.Location = new Point(4, 28);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(914, 403);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Run Payload";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // richTextBox2
            // 
            richTextBox2.Dock = DockStyle.Fill;
            richTextBox2.Location = new Point(0, 0);
            richTextBox2.Name = "richTextBox2";
            richTextBox2.Size = new Size(914, 376);
            richTextBox2.TabIndex = 0;
            richTextBox2.Text = "";
            // 
            // textBox3
            // 
            textBox3.Dock = DockStyle.Bottom;
            textBox3.Location = new Point(0, 376);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(914, 27);
            textBox3.TabIndex = 1;
            textBox3.TextChanged += textBox3_TextChanged;
            textBox3.KeyDown += textBox3_KeyDown;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(richTextBox3);
            tabPage4.Controls.Add(textBox4);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(914, 407);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Error Logs";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // richTextBox3
            // 
            richTextBox3.Dock = DockStyle.Fill;
            richTextBox3.Location = new Point(0, 0);
            richTextBox3.Name = "richTextBox3";
            richTextBox3.Size = new Size(914, 380);
            richTextBox3.TabIndex = 0;
            richTextBox3.Text = "";
            // 
            // textBox4
            // 
            textBox4.Dock = DockStyle.Bottom;
            textBox4.Location = new Point(0, 380);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(914, 27);
            textBox4.TabIndex = 1;
            textBox4.TextChanged += textBox4_TextChanged;
            textBox4.KeyDown += textBox4_KeyDown;
            // 
            // toolStrip1
            // 
            toolStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripDropDownButton1, toolStripButton2, toolStripButton1 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(922, 26);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem1 });
            toolStripDropDownButton1.Image = (Image)resources.GetObject("toolStripDropDownButton1.Image");
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(46, 23);
            toolStripDropDownButton1.Text = "File";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(111, 24);
            toolStripMenuItem1.Text = "Save";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(63, 23);
            toolStripButton2.Text = "Setting";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(45, 23);
            toolStripButton1.Text = "Help";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // frmLogs
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(922, 463);
            Controls.Add(toolStrip1);
            Controls.Add(tabControl1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "frmLogs";
            Text = "frmLogs";
            FormClosing += frmLogs_FormClosing;
            FormClosed += frmLogs_FormClosed;
            Load += frmLogs_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private ToolStrip toolStrip1;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private RichTextBox richTextBox1;
        private ListView listView1;
        private RichTextBox richTextBox2;
        private RichTextBox richTextBox3;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private System.Windows.Forms.Timer timer1;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripButton toolStripButton1;
        private TextBox textBox1;
        private TextBox textBox2;
        private TextBox textBox3;
        private TextBox textBox4;
        private ToolStripButton toolStripButton2;
    }
}