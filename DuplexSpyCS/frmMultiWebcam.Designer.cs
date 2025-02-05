namespace DuplexSpyCS
{
    partial class frmMultiWebcam
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMultiWebcam));
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tableLayoutPanel1 = new TableLayoutPanel();
            statusStrip2 = new StatusStrip();
            toolStrip1 = new ToolStrip();
            timer1 = new System.Windows.Forms.Timer(components);
            toolStrip2 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripButton4 = new ToolStripButton();
            toolStripButton5 = new ToolStripButton();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            toolStrip1.SuspendLayout();
            toolStrip2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 26);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(915, 480);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(tableLayoutPanel1);
            tabPage1.Controls.Add(statusStrip2);
            tabPage1.Controls.Add(toolStrip1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(907, 448);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Monitor";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(3, 29);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tableLayoutPanel1.Size = new Size(901, 394);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // statusStrip2
            // 
            statusStrip2.Location = new Point(3, 423);
            statusStrip2.Name = "statusStrip2";
            statusStrip2.Size = new Size(901, 22);
            statusStrip2.TabIndex = 1;
            statusStrip2.Text = "statusStrip2";
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton3, toolStripButton4 });
            toolStrip1.Location = new Point(3, 3);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(901, 26);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // timer1
            // 
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // toolStrip2
            // 
            toolStrip2.Items.AddRange(new ToolStripItem[] { toolStripButton2, toolStripButton5, toolStripButton1 });
            toolStrip2.Location = new Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(915, 26);
            toolStrip2.TabIndex = 2;
            toolStrip2.Text = "toolStrip2";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(80, 23);
            toolStripButton1.Text = "SnapShot";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(46, 23);
            toolStripButton2.Text = "Start";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStripButton3.Image = (Image)resources.GetObject("toolStripButton3.Image");
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(30, 23);
            toolStripButton3.Text = "<-";
            toolStripButton3.Click += toolStripButton3_Click;
            // 
            // toolStripButton4
            // 
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton4.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(30, 23);
            toolStripButton4.Text = "->";
            toolStripButton4.Click += toolStripButton4_Click;
            // 
            // toolStripButton5
            // 
            toolStripButton5.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton5.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStripButton5.Image = (Image)resources.GetObject("toolStripButton5.Image");
            toolStripButton5.ImageTransparentColor = Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new Size(45, 23);
            toolStripButton5.Text = "Stop";
            toolStripButton5.ToolTipText = "Stop";
            toolStripButton5.Click += toolStripButton5_Click;
            // 
            // frmMultiWebcam
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(915, 506);
            Controls.Add(tabControl1);
            Controls.Add(toolStrip2);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "frmMultiWebcam";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmMonitorWebcam";
            Load += frmMultiWebcam_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TabControl tabControl1;
        private TabPage tabPage1;
        private StatusStrip statusStrip2;
        private ToolStrip toolStrip1;
        private TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Timer timer1;
        private ToolStrip toolStrip2;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripButton toolStripButton4;
        private ToolStripButton toolStripButton5;
    }
}