namespace DuplexSpyCS
{
    partial class frmDllLoader
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
            label1 = new Label();
            textBox1 = new TextBox();
            button1 = new Button();
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
            tabControl1.Location = new Point(-1, 85);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(417, 477);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 11;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(statusStrip1);
            tabPage1.Controls.Add(listView1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(409, 445);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Victim";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(3, 418);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(403, 24);
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
            listView1.Size = new Size(403, 439);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Victim";
            columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Status";
            columnHeader2.Width = 200;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(richTextBox1);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(409, 449);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Logs";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(3, 3);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(403, 443);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // button2
            // 
            button2.Location = new Point(12, 41);
            button2.Name = "button2";
            button2.Size = new Size(392, 39);
            button2.TabIndex = 10;
            button2.Text = "Inject";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 11);
            label1.Name = "label1";
            label1.Size = new Size(40, 19);
            label1.TabIndex = 9;
            label1.Text = "File :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(58, 8);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(243, 27);
            textBox1.TabIndex = 8;
            // 
            // button1
            // 
            button1.Location = new Point(308, 5);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(96, 30);
            button1.TabIndex = 7;
            button1.Text = "...";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // frmDllLoader
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(417, 564);
            Controls.Add(tabControl1);
            Controls.Add(button2);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmDllLoader";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmDllLoader";
            FormClosed += frmDllLoader_FormClosed;
            Load += frmDllLoader_Load;
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
        private Label label1;
        private TextBox textBox1;
        private Button button1;
    }
}