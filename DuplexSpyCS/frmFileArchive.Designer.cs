namespace DuplexSpyCS
{
    partial class frmFileArchive
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileArchive));
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            button1 = new Button();
            label1 = new Label();
            textBox1 = new TextBox();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            tabPage2 = new TabPage();
            checkBox1 = new CheckBox();
            label3 = new Label();
            textBox2 = new TextBox();
            comboBox1 = new ComboBox();
            label2 = new Label();
            button2 = new Button();
            listView2 = new ListView();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            tabPage3 = new TabPage();
            richTextBox1 = new RichTextBox();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Location = new Point(0, 28);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(468, 437);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(button1);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(textBox1);
            tabPage1.Controls.Add(listView1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(460, 405);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "ZipFile";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(337, 12);
            button1.Name = "button1";
            button1.Size = new Size(115, 28);
            button1.TabIndex = 3;
            button1.Text = "Compress";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 16);
            label1.Name = "label1";
            label1.Size = new Size(114, 19);
            label1.TabIndex = 2;
            label1.Text = "Archive Name :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(128, 13);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(203, 27);
            textBox1.TabIndex = 1;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader5 });
            listView1.FullRowSelect = true;
            listView1.Location = new Point(8, 46);
            listView1.Name = "listView1";
            listView1.Size = new Size(444, 353);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Path";
            columnHeader1.Width = 240;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Directory";
            columnHeader2.Width = 100;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "State";
            columnHeader5.Width = 100;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(checkBox1);
            tabPage2.Controls.Add(label3);
            tabPage2.Controls.Add(textBox2);
            tabPage2.Controls.Add(comboBox1);
            tabPage2.Controls.Add(label2);
            tabPage2.Controls.Add(button2);
            tabPage2.Controls.Add(listView2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(460, 409);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "UnZipFile";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(348, 11);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(72, 23);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "Delete";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(8, 45);
            label3.Name = "label3";
            label3.Size = new Size(58, 19);
            label3.TabIndex = 5;
            label3.Text = "Name :";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(72, 42);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(270, 27);
            textBox2.TabIndex = 4;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Each to Seperate Folder", "Extract to Specific Folder", "Extract Here" });
            comboBox1.Location = new Point(72, 9);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(270, 27);
            comboBox1.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(17, 12);
            label2.Name = "label2";
            label2.Size = new Size(49, 19);
            label2.TabIndex = 2;
            label2.Text = "Type :";
            // 
            // button2
            // 
            button2.Location = new Point(348, 39);
            button2.Name = "button2";
            button2.Size = new Size(104, 31);
            button2.TabIndex = 1;
            button2.Text = "Extract";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // listView2
            // 
            listView2.Columns.AddRange(new ColumnHeader[] { columnHeader3, columnHeader4 });
            listView2.Location = new Point(6, 76);
            listView2.Name = "listView2";
            listView2.Size = new Size(448, 323);
            listView2.TabIndex = 0;
            listView2.UseCompatibleStateImageBehavior = false;
            listView2.View = View.Details;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Path";
            columnHeader3.Width = 300;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "State";
            columnHeader4.Width = 100;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(richTextBox1);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(460, 409);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Run Logs";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(0, 0);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(460, 409);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(468, 26);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(45, 23);
            toolStripButton1.Text = "Help";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // frmFileArchive
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(468, 467);
            Controls.Add(toolStrip1);
            Controls.Add(tabControl1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmFileArchive";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmFileArchive";
            Load += frmFileArchive_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
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
        private Button button1;
        private Label label1;
        private TextBox textBox1;
        private ListView listView1;
        private ListView listView2;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private TabPage tabPage3;
        private RichTextBox richTextBox1;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private Button button2;
        private Label label3;
        private TextBox textBox2;
        private ComboBox comboBox1;
        private Label label2;
        private ColumnHeader columnHeader5;
        private CheckBox checkBox1;
        private ToolStripButton toolStripButton1;
    }
}