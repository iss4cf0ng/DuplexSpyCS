namespace DuplexSpyCS
{
    partial class frmKillProc
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
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            button1 = new Button();
            textBox1 = new TextBox();
            button2 = new Button();
            label1 = new Label();
            button3 = new Button();
            button4 = new Button();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            button5 = new Button();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            listView1.Location = new Point(15, 15);
            listView1.Margin = new Padding(4);
            listView1.Name = "listView1";
            listView1.Size = new Size(235, 419);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Process Name";
            columnHeader1.Width = 200;
            // 
            // button1
            // 
            button1.Location = new Point(261, 172);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(223, 46);
            button1.TabIndex = 1;
            button1.Text = "Add";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(262, 38);
            textBox1.Margin = new Padding(4);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(222, 126);
            textBox1.TabIndex = 2;
            // 
            // button2
            // 
            button2.Location = new Point(261, 226);
            button2.Margin = new Padding(4);
            button2.Name = "button2";
            button2.Size = new Size(223, 46);
            button2.TabIndex = 4;
            button2.Text = "Remove Selected";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(261, 15);
            label1.Name = "label1";
            label1.Size = new Size(58, 19);
            label1.TabIndex = 5;
            label1.Text = "Name :";
            // 
            // button3
            // 
            button3.Location = new Point(260, 334);
            button3.Margin = new Padding(4);
            button3.Name = "button3";
            button3.Size = new Size(223, 46);
            button3.TabIndex = 7;
            button3.Text = "Unselect All";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Location = new Point(260, 280);
            button4.Margin = new Padding(4);
            button4.Name = "button4";
            button4.Size = new Size(223, 46);
            button4.TabIndex = 6;
            button4.Text = "Select All";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 445);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(499, 24);
            statusStrip1.TabIndex = 8;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(158, 19);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // button5
            // 
            button5.Location = new Point(260, 388);
            button5.Margin = new Padding(4);
            button5.Name = "button5";
            button5.Size = new Size(223, 46);
            button5.TabIndex = 9;
            button5.Text = "OK";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // frmKillProc
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(499, 469);
            Controls.Add(button5);
            Controls.Add(statusStrip1);
            Controls.Add(button3);
            Controls.Add(button4);
            Controls.Add(label1);
            Controls.Add(button2);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Controls.Add(listView1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmKillProc";
            Text = "frmKillProc";
            Load += frmKillProc_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView listView1;
        private Button button1;
        private TextBox textBox1;
        private Button button2;
        private Label label1;
        private Button button3;
        private Button button4;
        private ColumnHeader columnHeader1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Button button5;
    }
}