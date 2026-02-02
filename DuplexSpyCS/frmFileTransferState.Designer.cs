namespace DuplexSpyCS
{
    partial class frmFileTransferState
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileTransferState));
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStrip1 = new ToolStrip();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripButton4 = new ToolStripButton();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            contextMenuStrip1 = new ContextMenuStrip(components);
            toolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripMenuItem();
            toolStripProgressBar1 = new ToolStripProgressBar();
            statusStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripProgressBar1, toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 539);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(512, 24);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(158, 19);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton2, toolStripButton3, toolStripSeparator1, toolStripButton4 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(512, 26);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton2.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(54, 23);
            toolStripButton2.Text = "Pause";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStripButton3.Image = (Image)resources.GetObject("toolStripButton3.Image");
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(59, 23);
            toolStripButton3.Text = "Cancel";
            toolStripButton3.Click += toolStripButton3_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 26);
            // 
            // toolStripButton4
            // 
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton4.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(58, 23);
            toolStripButton4.Text = "Folder";
            toolStripButton4.Click += toolStripButton4_Click;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView1.ContextMenuStrip = contextMenuStrip1;
            listView1.Dock = DockStyle.Fill;
            listView1.FullRowSelect = true;
            listView1.Location = new Point(0, 26);
            listView1.Name = "listView1";
            listView1.Size = new Size(512, 513);
            listView1.TabIndex = 2;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Filename";
            columnHeader1.Width = 300;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Status";
            columnHeader2.Width = 100;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2, toolStripMenuItem3 });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(138, 76);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(137, 24);
            toolStripMenuItem1.Text = "Retry";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(137, 24);
            toolStripMenuItem2.Text = "Retry All";
            toolStripMenuItem2.Click += toolStripMenuItem2_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(137, 24);
            toolStripMenuItem3.Text = "Next";
            toolStripMenuItem3.Click += toolStripMenuItem3_Click;
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(200, 18);
            // 
            // frmFileTransferState
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(512, 563);
            Controls.Add(listView1);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "frmFileTransferState";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmFileTransferStatus";
            FormClosed += frmFileTransferState_FormClosed;
            Load += frmFileTransferStatus_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStrip toolStrip1;
        private ListView listView1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButton4;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripProgressBar toolStripProgressBar1;
    }
}