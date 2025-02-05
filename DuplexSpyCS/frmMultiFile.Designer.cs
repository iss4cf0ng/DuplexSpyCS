namespace DuplexSpyCS
{
    partial class frmMultiFile
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
            toolStrip1 = new ToolStrip();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            comboBox2 = new ComboBox();
            checkBox2 = new CheckBox();
            textBox2 = new TextBox();
            comboBox1 = new ComboBox();
            label2 = new Label();
            checkBox1 = new CheckBox();
            button2 = new Button();
            button1 = new Button();
            textBox1 = new TextBox();
            label1 = new Label();
            listView1 = new ListView();
            statusStrip1 = new StatusStrip();
            tabPage2 = new TabPage();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(835, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 25);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(835, 428);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(comboBox2);
            tabPage1.Controls.Add(checkBox2);
            tabPage1.Controls.Add(textBox2);
            tabPage1.Controls.Add(comboBox1);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(checkBox1);
            tabPage1.Controls.Add(button2);
            tabPage1.Controls.Add(button1);
            tabPage1.Controls.Add(textBox1);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(listView1);
            tabPage1.Controls.Add(statusStrip1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(827, 396);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Upload";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Items.AddRange(new object[] { "File", "Folder" });
            comboBox2.Location = new Point(94, 11);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(127, 27);
            comboBox2.TabIndex = 11;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(751, 44);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(68, 23);
            checkBox2.TabIndex = 10;
            checkBox2.Text = "Unzip";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(227, 42);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(414, 27);
            textBox2.TabIndex = 9;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "FullPath", "Desktop", "Temp" });
            comboBox1.Location = new Point(94, 42);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(127, 27);
            comboBox1.TabIndex = 8;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(8, 44);
            label2.Name = "label2";
            label2.Size = new Size(70, 19);
            label2.TabIndex = 7;
            label2.Text = "Remote :";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(647, 44);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(98, 23);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "Create Dir";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(709, 6);
            button2.Name = "button2";
            button2.Size = new Size(115, 35);
            button2.TabIndex = 5;
            button2.Text = "Send";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(591, 6);
            button1.Name = "button1";
            button1.Size = new Size(115, 35);
            button1.TabIndex = 4;
            button1.Text = "Open";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(227, 11);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(358, 27);
            textBox1.TabIndex = 3;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(17, 14);
            label1.Name = "label1";
            label1.Size = new Size(61, 19);
            label1.TabIndex = 2;
            label1.Text = "Object :";
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView1.Location = new Point(0, 72);
            listView1.Name = "listView1";
            listView1.Size = new Size(827, 296);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(3, 371);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(821, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(827, 400);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Download";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Entry";
            columnHeader1.Width = 300;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "State";
            columnHeader2.Width = 200;
            // 
            // frmMultiFile
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(835, 453);
            Controls.Add(tabControl1);
            Controls.Add(toolStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "frmMultiFile";
            Text = "frmSendFile";
            Load += frmMultiFile_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private ListView listView1;
        private StatusStrip statusStrip1;
        private TabPage tabPage2;
        private TextBox textBox1;
        private CheckBox checkBox2;
        private TextBox textBox2;
        private ComboBox comboBox1;
        private Label label2;
        private CheckBox checkBox1;
        private Button button2;
        private Button button1;
        private ComboBox comboBox2;
        private Label label1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
    }
}