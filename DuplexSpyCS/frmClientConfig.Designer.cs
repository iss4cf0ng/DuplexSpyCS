namespace DuplexSpyCS
{
    partial class frmClientConfig
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
            toolStrip1 = new ToolStrip();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            button1 = new Button();
            textBox1 = new TextBox();
            label1 = new Label();
            tabPage2 = new TabPage();
            groupBox1 = new GroupBox();
            radioButton2 = new RadioButton();
            button2 = new Button();
            radioButton1 = new RadioButton();
            label4 = new Label();
            label5 = new Label();
            numericUpDown2 = new NumericUpDown();
            label3 = new Label();
            label2 = new Label();
            numericUpDown1 = new NumericUpDown();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 334);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(599, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(599, 25);
            toolStrip1.TabIndex = 1;
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
            tabControl1.Size = new Size(599, 309);
            tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(groupBox1);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(numericUpDown2);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(numericUpDown1);
            tabPage1.Controls.Add(button1);
            tabPage1.Controls.Add(textBox1);
            tabPage1.Controls.Add(label1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(591, 277);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "General";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(464, 213);
            button1.Name = "button1";
            button1.Size = new Size(119, 58);
            button1.TabIndex = 2;
            button1.Text = "Apply";
            button1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(95, 6);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(293, 27);
            textBox1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 9);
            label1.Name = "label1";
            label1.Size = new Size(81, 19);
            label1.TabIndex = 0;
            label1.Text = "Online ID :";
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 28);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(837, 402);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Remote";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(radioButton2);
            groupBox1.Controls.Add(button2);
            groupBox1.Controls.Add(radioButton1);
            groupBox1.Location = new Point(8, 39);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(287, 80);
            groupBox1.TabIndex = 22;
            groupBox1.TabStop = false;
            groupBox1.Text = "Anti-Process";
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Location = new Point(85, 36);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(78, 23);
            radioButton2.TabIndex = 11;
            radioButton2.TabStop = true;
            radioButton2.Text = "Disable";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(169, 31);
            button2.Name = "button2";
            button2.Size = new Size(106, 32);
            button2.TabIndex = 10;
            button2.Text = "...";
            button2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(6, 36);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(73, 23);
            radioButton1.TabIndex = 10;
            radioButton1.TabStop = true;
            radioButton1.Text = "Enable";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(234, 215);
            label4.Name = "label4";
            label4.Size = new Size(30, 19);
            label4.TabIndex = 21;
            label4.Text = "ms";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(19, 215);
            label5.Name = "label5";
            label5.Size = new Size(83, 19);
            label5.TabIndex = 20;
            label5.Text = "Send info :";
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(108, 213);
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(120, 27);
            numericUpDown2.TabIndex = 19;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(234, 182);
            label3.Name = "label3";
            label3.Size = new Size(30, 19);
            label3.TabIndex = 18;
            label3.Text = "ms";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(14, 182);
            label2.Name = "label2";
            label2.Size = new Size(88, 19);
            label2.TabIndex = 17;
            label2.Text = "Reconnect :";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(108, 180);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(120, 27);
            numericUpDown1.TabIndex = 16;
            // 
            // frmClientConfig
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(599, 356);
            Controls.Add(tabControl1);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "frmClientConfig";
            Text = "frmClientConfig";
            Load += frmClientConfig_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStrip toolStrip1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Button button1;
        private TextBox textBox1;
        private Label label1;
        private GroupBox groupBox1;
        private RadioButton radioButton2;
        private Button button2;
        private RadioButton radioButton1;
        private Label label4;
        private Label label5;
        private NumericUpDown numericUpDown2;
        private Label label3;
        private Label label2;
        private NumericUpDown numericUpDown1;
    }
}