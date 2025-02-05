namespace DuplexSpyCS
{
    partial class frmShell
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmShell));
            richTextBox1 = new RichTextBox();
            textBox1 = new TextBox();
            button1 = new Button();
            toolStrip1 = new ToolStrip();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            toolStripMenuItem1 = new ToolStripMenuItem();
            toolStripDropDownButton2 = new ToolStripDropDownButton();
            toolStripMenuItem2 = new ToolStripMenuItem();
            toolStripMenuItem4 = new ToolStripMenuItem();
            toolStripMenuItem5 = new ToolStripMenuItem();
            toolStripMenuItem3 = new ToolStripMenuItem();
            toolStripButton1 = new ToolStripButton();
            contextMenuStrip1 = new ContextMenuStrip(components);
            textBox2 = new TextBox();
            button2 = new Button();
            toolStripButton2 = new ToolStripButton();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.BackColor = Color.Black;
            richTextBox1.Font = new Font("NSimSun", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            richTextBox1.ForeColor = Color.White;
            richTextBox1.Location = new Point(0, 56);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(932, 414);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            richTextBox1.SelectionChanged += richTextBox1_SelectionChanged;
            richTextBox1.KeyDown += richTextBox1_KeyDown;
            richTextBox1.KeyPress += richTextBox1_KeyPress;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.BackColor = Color.Black;
            textBox1.ForeColor = Color.White;
            textBox1.Location = new Point(0, 28);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(823, 27);
            textBox1.TabIndex = 1;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.BackColor = Color.Black;
            button1.ForeColor = Color.White;
            button1.Location = new Point(822, 28);
            button1.Name = "button1";
            button1.Size = new Size(110, 27);
            button1.TabIndex = 2;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripDropDownButton1, toolStripDropDownButton2, toolStripButton2, toolStripButton1 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(932, 26);
            toolStrip1.TabIndex = 3;
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
            // toolStripDropDownButton2
            // 
            toolStripDropDownButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem2, toolStripMenuItem3 });
            toolStripDropDownButton2.Image = (Image)resources.GetObject("toolStripDropDownButton2.Image");
            toolStripDropDownButton2.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            toolStripDropDownButton2.Size = new Size(52, 23);
            toolStripDropDownButton2.Text = "Tool";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem4, toolStripMenuItem5 });
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(114, 24);
            toolStripMenuItem2.Text = "Goto";
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new Size(129, 24);
            toolStripMenuItem4.Text = "Top";
            toolStripMenuItem4.Click += toolStripMenuItem4_Click;
            // 
            // toolStripMenuItem5
            // 
            toolStripMenuItem5.Name = "toolStripMenuItem5";
            toolStripMenuItem5.Size = new Size(129, 24);
            toolStripMenuItem5.Text = "Bottom";
            toolStripMenuItem5.Click += toolStripMenuItem5_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new Size(114, 24);
            toolStripMenuItem3.Text = "Clear";
            toolStripMenuItem3.Click += toolStripMenuItem3_Click;
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
            // contextMenuStrip1
            // 
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // textBox2
            // 
            textBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox2.BackColor = Color.Black;
            textBox2.ForeColor = Color.White;
            textBox2.Location = new Point(0, 469);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(823, 27);
            textBox2.TabIndex = 5;
            textBox2.KeyDown += textBox2_KeyDown;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button2.BackColor = Color.Black;
            button2.ForeColor = Color.White;
            button2.Location = new Point(822, 469);
            button2.Name = "button2";
            button2.Size = new Size(110, 27);
            button2.TabIndex = 6;
            button2.Text = "Exec";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
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
            // frmShell
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(932, 498);
            Controls.Add(button2);
            Controls.Add(textBox2);
            Controls.Add(toolStrip1);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(richTextBox1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "frmShell";
            Text = "frmShell";
            Load += frmShell_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox richTextBox1;
        private TextBox textBox1;
        private Button button1;
        private ToolStrip toolStrip1;
        private ContextMenuStrip contextMenuStrip1;
        private TextBox textBox2;
        private Button button2;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripButton toolStripButton1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripDropDownButton toolStripDropDownButton2;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripButton toolStripButton2;
    }
}