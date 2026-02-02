namespace DuplexSpyCS
{
    partial class frmRunScript
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRunScript));
            comboBox1 = new ComboBox();
            label1 = new Label();
            button1 = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            richTextBox1 = new RichTextBox();
            label2 = new Label();
            textBox1 = new TextBox();
            menuStrip1 = new MenuStrip();
            toolStripMenuItem1 = new ToolStripMenuItem();
            tabControl1.SuspendLayout();
            tabPage2.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Batch", "CSharp", "VB" });
            comboBox1.Location = new Point(106, 30);
            comboBox1.Margin = new Padding(4);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(140, 27);
            comboBox1.TabIndex = 1;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 33);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(86, 19);
            label1.TabIndex = 2;
            label1.Text = "Language :";
            // 
            // button1
            // 
            button1.Location = new Point(528, 30);
            button1.Name = "button1";
            button1.Size = new Size(109, 28);
            button1.TabIndex = 3;
            button1.Text = "Run";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(3, 64);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(635, 357);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 4;
            tabControl1.KeyDown += tabControl1_KeyDown;
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(627, 325);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Code";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(richTextBox1);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(627, 318);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Output";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(3, 3);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(621, 312);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(254, 33);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(61, 19);
            label2.TabIndex = 5;
            label2.Text = "Param :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(322, 30);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(200, 27);
            textBox1.TabIndex = 6;
            // 
            // menuStrip1
            // 
            menuStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            menuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1 });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(639, 27);
            menuStrip1.TabIndex = 7;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(53, 23);
            toolStripMenuItem1.Text = "Help";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            // 
            // frmRunScript
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(639, 424);
            Controls.Add(textBox1);
            Controls.Add(label2);
            Controls.Add(tabControl1);
            Controls.Add(button1);
            Controls.Add(label1);
            Controls.Add(comboBox1);
            Controls.Add(menuStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4);
            Name = "frmRunScript";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmRunScript";
            FormClosed += frmRunScript_FormClosed;
            Load += frmRunScript_Load;
            tabControl1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ComboBox comboBox1;
        private Label label1;
        private Button button1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private RichTextBox richTextBox1;
        private Label label2;
        private TextBox textBox1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
    }
}