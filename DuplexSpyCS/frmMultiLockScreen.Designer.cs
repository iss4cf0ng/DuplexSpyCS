namespace DuplexSpyCS
{
    partial class frmMultiLockScreen
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
            statusStrip1 = new StatusStrip();
            label1 = new Label();
            textBox1 = new TextBox();
            button1 = new Button();
            button2 = new Button();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            label2 = new Label();
            comboBox1 = new ComboBox();
            button3 = new Button();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 411);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(823, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 17);
            label1.Name = "label1";
            label1.Size = new Size(60, 19);
            label1.TabIndex = 2;
            label1.Text = "Image :";
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(78, 14);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(188, 27);
            textBox1.TabIndex = 3;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(454, 8);
            button1.Name = "button1";
            button1.Size = new Size(116, 37);
            button1.TabIndex = 4;
            button1.Text = "Choose";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button2.Location = new Point(576, 8);
            button2.Name = "button2";
            button2.Size = new Size(116, 37);
            button2.TabIndex = 5;
            button2.Text = "Lock";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3 });
            listView1.Location = new Point(12, 51);
            listView1.Name = "listView1";
            listView1.Size = new Size(802, 357);
            listView1.TabIndex = 6;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Index";
            columnHeader1.Width = 70;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "OnlineID";
            columnHeader2.Width = 130;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Status";
            columnHeader3.Width = 100;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(272, 17);
            label2.Name = "label2";
            label2.Size = new Size(49, 19);
            label2.TabIndex = 7;
            label2.Text = "Type :";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(327, 14);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 27);
            comboBox1.TabIndex = 8;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button3.Location = new Point(698, 8);
            button3.Name = "button3";
            button3.Size = new Size(116, 37);
            button3.TabIndex = 9;
            button3.Text = "Unlock";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // frmMultiLockScreen
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(823, 433);
            Controls.Add(button3);
            Controls.Add(comboBox1);
            Controls.Add(label2);
            Controls.Add(listView1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(statusStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "frmMultiLockScreen";
            Text = "frmMultiLockScreen";
            Load += frmMultiLockScreen_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private Label label1;
        private TextBox textBox1;
        private Button button1;
        private Button button2;
        private ListView listView1;
        private Label label2;
        private ComboBox comboBox1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private Button button3;
    }
}