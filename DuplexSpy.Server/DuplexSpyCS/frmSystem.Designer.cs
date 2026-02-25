namespace DuplexSpyCS
{
    partial class frmSystem
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSystem));
            toolStrip1 = new ToolStrip();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            listView1 = new ListView();
            toolStrip4 = new ToolStrip();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            tabPage2 = new TabPage();
            treeView1 = new TreeView();
            statusStrip2 = new StatusStrip();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            toolStrip2 = new ToolStrip();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            toolStripMenuItem8 = new ToolStripMenuItem();
            toolStripMenuItem9 = new ToolStripMenuItem();
            tabPage3 = new TabPage();
            listView2 = new ListView();
            statusStrip3 = new StatusStrip();
            toolStripStatusLabel3 = new ToolStripStatusLabel();
            toolStrip3 = new ToolStrip();
            tabPage4 = new TabPage();
            listView3 = new ListView();
            statusStrip4 = new StatusStrip();
            toolStripStatusLabel4 = new ToolStripStatusLabel();
            menuApp = new ContextMenuStrip(components);
            toolStripMenuItem1 = new ToolStripMenuItem();
            menuDevice = new ContextMenuStrip(components);
            toolStripMenuItem2 = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItem3 = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripMenuItem();
            menuInterface = new ContextMenuStrip(components);
            toolStripMenuItem5 = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripMenuItem6 = new ToolStripMenuItem();
            toolStripMenuItem7 = new ToolStripMenuItem();
            appMenu = new ContextMenuStrip(components);
            deviceMenu = new ContextMenuStrip(components);
            ifMenu = new ContextMenuStrip(components);
            evMenu = new ContextMenuStrip(components);
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            statusStrip1.SuspendLayout();
            tabPage2.SuspendLayout();
            statusStrip2.SuspendLayout();
            toolStrip2.SuspendLayout();
            tabPage3.SuspendLayout();
            statusStrip3.SuspendLayout();
            tabPage4.SuspendLayout();
            statusStrip4.SuspendLayout();
            menuApp.SuspendLayout();
            menuDevice.SuspendLayout();
            menuInterface.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(945, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 25);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(945, 466);
            tabControl1.TabIndex = 1;
            tabControl1.KeyDown += tabControl1_KeyDown;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(listView1);
            tabPage1.Controls.Add(toolStrip4);
            tabPage1.Controls.Add(statusStrip1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(937, 434);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Application";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            listView1.Dock = DockStyle.Fill;
            listView1.Location = new Point(3, 28);
            listView1.Name = "listView1";
            listView1.Size = new Size(931, 379);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.KeyDown += listView1_KeyDown;
            // 
            // toolStrip4
            // 
            toolStrip4.Location = new Point(3, 3);
            toolStrip4.Name = "toolStrip4";
            toolStrip4.Size = new Size(931, 25);
            toolStrip4.TabIndex = 2;
            toolStrip4.Text = "toolStrip4";
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(3, 407);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(931, 24);
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
            tabPage2.Controls.Add(treeView1);
            tabPage2.Controls.Add(statusStrip2);
            tabPage2.Controls.Add(toolStrip2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(937, 438);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Device Mgr";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            treeView1.Dock = DockStyle.Fill;
            treeView1.Location = new Point(3, 29);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(931, 382);
            treeView1.TabIndex = 0;
            treeView1.BeforeSelect += treeView1_BeforeSelect;
            treeView1.AfterSelect += treeView1_AfterSelect;
            treeView1.DoubleClick += treeView1_DoubleClick;
            treeView1.KeyDown += treeView1_KeyDown;
            // 
            // statusStrip2
            // 
            statusStrip2.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusStrip2.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel2 });
            statusStrip2.Location = new Point(3, 411);
            statusStrip2.Name = "statusStrip2";
            statusStrip2.Size = new Size(931, 24);
            statusStrip2.TabIndex = 2;
            statusStrip2.Text = "statusStrip2";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(158, 19);
            toolStripStatusLabel2.Text = "toolStripStatusLabel2";
            // 
            // toolStrip2
            // 
            toolStrip2.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStrip2.Items.AddRange(new ToolStripItem[] { toolStripDropDownButton1 });
            toolStrip2.Location = new Point(3, 3);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(931, 26);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem8, toolStripMenuItem9 });
            toolStripDropDownButton1.Image = (Image)resources.GetObject("toolStripDropDownButton1.Image");
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(68, 23);
            toolStripDropDownButton1.Text = "Nodes";
            // 
            // toolStripMenuItem8
            // 
            toolStripMenuItem8.Name = "toolStripMenuItem8";
            toolStripMenuItem8.Size = new Size(159, 24);
            toolStripMenuItem8.Text = "Expand All";
            toolStripMenuItem8.Click += toolStripMenuItem8_Click;
            // 
            // toolStripMenuItem9
            // 
            toolStripMenuItem9.Name = "toolStripMenuItem9";
            toolStripMenuItem9.Size = new Size(159, 24);
            toolStripMenuItem9.Text = "Collapse All";
            toolStripMenuItem9.Click += toolStripMenuItem9_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(listView2);
            tabPage3.Controls.Add(statusStrip3);
            tabPage3.Controls.Add(toolStrip3);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(937, 438);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Internet";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // listView2
            // 
            listView2.Dock = DockStyle.Fill;
            listView2.Location = new Point(0, 25);
            listView2.Name = "listView2";
            listView2.Size = new Size(937, 389);
            listView2.TabIndex = 2;
            listView2.UseCompatibleStateImageBehavior = false;
            listView2.KeyDown += listView2_KeyDown;
            // 
            // statusStrip3
            // 
            statusStrip3.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusStrip3.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel3 });
            statusStrip3.Location = new Point(0, 414);
            statusStrip3.Name = "statusStrip3";
            statusStrip3.Size = new Size(937, 24);
            statusStrip3.TabIndex = 1;
            statusStrip3.Text = "statusStrip3";
            // 
            // toolStripStatusLabel3
            // 
            toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            toolStripStatusLabel3.Size = new Size(158, 19);
            toolStripStatusLabel3.Text = "toolStripStatusLabel3";
            // 
            // toolStrip3
            // 
            toolStrip3.Location = new Point(0, 0);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.Size = new Size(937, 25);
            toolStrip3.TabIndex = 0;
            toolStrip3.Text = "toolStrip3";
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(listView3);
            tabPage4.Controls.Add(statusStrip4);
            tabPage4.Location = new Point(4, 28);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(937, 434);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Environment Variables";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // listView3
            // 
            listView3.Dock = DockStyle.Fill;
            listView3.Location = new Point(0, 0);
            listView3.Name = "listView3";
            listView3.Size = new Size(937, 410);
            listView3.TabIndex = 0;
            listView3.UseCompatibleStateImageBehavior = false;
            listView3.DoubleClick += listView3_DoubleClick;
            listView3.KeyDown += listView3_KeyDown;
            // 
            // statusStrip4
            // 
            statusStrip4.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusStrip4.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel4 });
            statusStrip4.Location = new Point(0, 410);
            statusStrip4.Name = "statusStrip4";
            statusStrip4.Size = new Size(937, 24);
            statusStrip4.TabIndex = 1;
            statusStrip4.Text = "statusStrip4";
            // 
            // toolStripStatusLabel4
            // 
            toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            toolStripStatusLabel4.Size = new Size(158, 19);
            toolStripStatusLabel4.Text = "toolStripStatusLabel4";
            // 
            // menuApp
            // 
            menuApp.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1 });
            menuApp.Name = "menuApp";
            menuApp.Size = new Size(108, 26);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(107, 22);
            toolStripMenuItem1.Text = "Detail";
            // 
            // menuDevice
            // 
            menuDevice.Items.AddRange(new ToolStripItem[] { toolStripMenuItem2, toolStripSeparator1, toolStripMenuItem3, toolStripMenuItem4 });
            menuDevice.Name = "menuDevice";
            menuDevice.Size = new Size(117, 76);
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(116, 22);
            toolStripMenuItem2.Text = "Detail";
            toolStripMenuItem2.Click += toolStripMenuItem2_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(113, 6);
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(116, 22);
            toolStripMenuItem3.Text = "Enable";
            toolStripMenuItem3.Click += toolStripMenuItem3_Click;
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(116, 22);
            toolStripMenuItem4.Text = "Disable";
            toolStripMenuItem4.Click += toolStripMenuItem4_Click;
            // 
            // menuInterface
            // 
            menuInterface.Items.AddRange(new ToolStripItem[] { toolStripMenuItem5, toolStripSeparator2, toolStripMenuItem6, toolStripMenuItem7 });
            menuInterface.Name = "menuInterface";
            menuInterface.Size = new Size(117, 76);
            // 
            // toolStripMenuItem5
            // 
            toolStripMenuItem5.Name = "toolStripMenuItem5";
            toolStripMenuItem5.Size = new Size(116, 22);
            toolStripMenuItem5.Text = "Detail";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(113, 6);
            // 
            // toolStripMenuItem6
            // 
            toolStripMenuItem6.Name = "toolStripMenuItem6";
            toolStripMenuItem6.Size = new Size(116, 22);
            toolStripMenuItem6.Text = "Enable";
            toolStripMenuItem6.Click += toolStripMenuItem6_Click;
            // 
            // toolStripMenuItem7
            // 
            toolStripMenuItem7.Name = "toolStripMenuItem7";
            toolStripMenuItem7.Size = new Size(116, 22);
            toolStripMenuItem7.Text = "Disable";
            toolStripMenuItem7.Click += toolStripMenuItem7_Click;
            // 
            // appMenu
            // 
            appMenu.Name = "appMenu";
            appMenu.Size = new Size(61, 4);
            // 
            // deviceMenu
            // 
            deviceMenu.Name = "deviceMenu";
            deviceMenu.Size = new Size(61, 4);
            // 
            // ifMenu
            // 
            ifMenu.Name = "ifMenu";
            ifMenu.Size = new Size(61, 4);
            // 
            // evMenu
            // 
            evMenu.Name = "evMenu";
            evMenu.Size = new Size(61, 4);
            // 
            // frmSystem
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(945, 491);
            Controls.Add(tabControl1);
            Controls.Add(toolStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "frmSystem";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmSystem";
            Load += frmSystem_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            statusStrip2.ResumeLayout(false);
            statusStrip2.PerformLayout();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            statusStrip3.ResumeLayout(false);
            statusStrip3.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            statusStrip4.ResumeLayout(false);
            statusStrip4.PerformLayout();
            menuApp.ResumeLayout(false);
            menuDevice.ResumeLayout(false);
            menuInterface.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private ListView listView1;
        private StatusStrip statusStrip1;
        private TreeView treeView1;
        private StatusStrip statusStrip2;
        private ToolStrip toolStrip2;
        private ListView listView2;
        private StatusStrip statusStrip3;
        private ToolStrip toolStrip3;
        private TabPage tabPage4;
        private ListView listView3;
        private ContextMenuStrip menuApp;
        private ContextMenuStrip menuDevice;
        private ContextMenuStrip menuInterface;
        private ToolStrip toolStrip4;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private ToolStripStatusLabel toolStripStatusLabel3;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem toolStripMenuItem6;
        private ToolStripMenuItem toolStripMenuItem7;
        private ContextMenuStrip appMenu;
        private ContextMenuStrip deviceMenu;
        private ContextMenuStrip ifMenu;
        private ContextMenuStrip evMenu;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem toolStripMenuItem8;
        private ToolStripMenuItem toolStripMenuItem9;
        private StatusStrip statusStrip4;
        private ToolStripStatusLabel toolStripStatusLabel4;
    }
}