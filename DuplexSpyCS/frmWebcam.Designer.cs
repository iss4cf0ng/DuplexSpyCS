namespace DuplexSpyCS
{
    partial class frmWebcam
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmWebcam));
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripButton5 = new ToolStripButton();
            toolStripButton6 = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripLabel1 = new ToolStripLabel();
            toolStripComboBox1 = new ToolStripComboBox();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripButton4 = new ToolStripButton();
            pictureBox1 = new PictureBox();
            timer1 = new System.Windows.Forms.Timer(components);
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton3, toolStripSeparator3, toolStripButton5, toolStripButton6, toolStripSeparator1, toolStripLabel1, toolStripComboBox1, toolStripSeparator2, toolStripButton4 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(589, 26);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(46, 23);
            toolStripButton1.Text = "Start";
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(45, 23);
            toolStripButton2.Text = "Stop";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.Image = (Image)resources.GetObject("toolStripButton3.Image");
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(78, 23);
            toolStripButton3.Text = "Snapshot";
            toolStripButton3.Click += toolStripButton3_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 26);
            // 
            // toolStripButton5
            // 
            toolStripButton5.CheckOnClick = true;
            toolStripButton5.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton5.Image = (Image)resources.GetObject("toolStripButton5.Image");
            toolStripButton5.ImageTransparentColor = Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new Size(63, 23);
            toolStripButton5.Text = "Record";
            toolStripButton5.CheckedChanged += toolStripButton5_CheckedChanged;
            // 
            // toolStripButton6
            // 
            toolStripButton6.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton6.Image = (Image)resources.GetObject("toolStripButton6.Image");
            toolStripButton6.ImageTransparentColor = Color.Magenta;
            toolStripButton6.Name = "toolStripButton6";
            toolStripButton6.Size = new Size(94, 23);
            toolStripButton6.Text = "Save Image";
            toolStripButton6.Click += toolStripButton6_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 26);
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(55, 23);
            toolStripLabel1.Text = "Device";
            // 
            // toolStripComboBox1
            // 
            toolStripComboBox1.Name = "toolStripComboBox1";
            toolStripComboBox1.Size = new Size(121, 26);
            toolStripComboBox1.SelectedIndexChanged += toolStripComboBox1_SelectedIndexChanged;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 26);
            // 
            // toolStripButton4
            // 
            toolStripButton4.Checked = true;
            toolStripButton4.CheckOnClick = true;
            toolStripButton4.CheckState = CheckState.Checked;
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(39, 23);
            toolStripButton4.Text = "FPS";
            toolStripButton4.CheckedChanged += toolStripButton4_CheckedChanged;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Black;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 26);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(589, 386);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // timer1
            // 
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // frmWebcam
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(589, 412);
            Controls.Add(pictureBox1);
            Controls.Add(toolStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "frmWebcam";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmWebcam";
            FormClosing += frmWebcam_FormClosing;
            Load += frmWebcam_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private PictureBox pictureBox1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox toolStripComboBox1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolStripButton4;
        private System.Windows.Forms.Timer timer1;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton toolStripButton5;
        private ToolStripButton toolStripButton6;
    }
}