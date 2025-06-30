namespace DuplexSpyCS
{
    partial class frmTipoff
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
            button1 = new Button();
            label1 = new Label();
            comboBox1 = new ComboBox();
            comboBox2 = new ComboBox();
            label3 = new Label();
            numericUpDown1 = new NumericUpDown();
            numericUpDown2 = new NumericUpDown();
            label2 = new Label();
            label4 = new Label();
            groupBox1 = new GroupBox();
            button2 = new Button();
            label5 = new Label();
            textBox1 = new TextBox();
            groupBox2 = new GroupBox();
            textBox11 = new TextBox();
            label16 = new Label();
            checkBox7 = new CheckBox();
            textBox10 = new TextBox();
            label15 = new Label();
            groupBox5 = new GroupBox();
            textBox7 = new TextBox();
            label12 = new Label();
            textBox6 = new TextBox();
            label11 = new Label();
            textBox5 = new TextBox();
            label10 = new Label();
            comboBox3 = new ComboBox();
            label9 = new Label();
            groupBox4 = new GroupBox();
            numericUpDown5 = new NumericUpDown();
            numericUpDown4 = new NumericUpDown();
            label14 = new Label();
            label13 = new Label();
            groupBox3 = new GroupBox();
            numericUpDown3 = new NumericUpDown();
            checkBox6 = new CheckBox();
            label8 = new Label();
            checkBox5 = new CheckBox();
            checkBox4 = new CheckBox();
            checkBox3 = new CheckBox();
            checkBox2 = new CheckBox();
            checkBox1 = new CheckBox();
            textBox3 = new TextBox();
            label7 = new Label();
            label6 = new Label();
            textBox2 = new TextBox();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown4).BeginInit();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 507);
            button1.Name = "button1";
            button1.Size = new Size(542, 75);
            button1.TabIndex = 0;
            button1.Text = "Send Request";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(34, 90);
            label1.Name = "label1";
            label1.Size = new Size(74, 19);
            label1.TabIndex = 1;
            label1.Text = "Protocol :";
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(114, 87);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(145, 27);
            comboBox1.TabIndex = 2;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(114, 54);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(145, 27);
            comboBox2.TabIndex = 6;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(51, 57);
            label3.Name = "label3";
            label3.Size = new Size(57, 19);
            label3.TabIndex = 5;
            label3.Text = "Mode :";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(376, 120);
            numericUpDown1.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(120, 27);
            numericUpDown1.TabIndex = 7;
            numericUpDown1.Value = new decimal(new int[] { 4444, 0, 0, 0 });
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(126, 59);
            numericUpDown2.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(226, 27);
            numericUpDown2.TabIndex = 8;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(290, 122);
            label2.Name = "label2";
            label2.Size = new Size(80, 19);
            label2.TabIndex = 9;
            label2.Text = "Dest Port :";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(13, 61);
            label4.Name = "label4";
            label4.Size = new Size(107, 19);
            label4.TabIndex = 10;
            label4.Text = "Callback Port :";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button2);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(numericUpDown2);
            groupBox1.Controls.Add(label4);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(542, 98);
            groupBox1.TabIndex = 11;
            groupBox1.TabStop = false;
            groupBox1.Text = "DuplexSpy Server";
            // 
            // button2
            // 
            button2.Location = new Point(358, 25);
            button2.Name = "button2";
            button2.Size = new Size(178, 61);
            button2.TabIndex = 15;
            button2.Text = "Test";
            button2.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 29);
            label5.Name = "label5";
            label5.Size = new Size(108, 19);
            label5.TabIndex = 14;
            label5.Text = "Callback IPv4 :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(126, 26);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(226, 27);
            textBox1.TabIndex = 13;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(textBox11);
            groupBox2.Controls.Add(label16);
            groupBox2.Controls.Add(checkBox7);
            groupBox2.Controls.Add(textBox10);
            groupBox2.Controls.Add(label15);
            groupBox2.Controls.Add(groupBox5);
            groupBox2.Controls.Add(groupBox4);
            groupBox2.Controls.Add(groupBox3);
            groupBox2.Controls.Add(textBox3);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(textBox2);
            groupBox2.Controls.Add(comboBox2);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(comboBox1);
            groupBox2.Controls.Add(numericUpDown1);
            groupBox2.Controls.Add(label3);
            groupBox2.Location = new Point(12, 116);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(542, 385);
            groupBox2.TabIndex = 12;
            groupBox2.TabStop = false;
            groupBox2.Text = "Target";
            // 
            // textBox11
            // 
            textBox11.Location = new Point(114, 21);
            textBox11.Name = "textBox11";
            textBox11.Size = new Size(145, 27);
            textBox11.TabIndex = 28;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(24, 24);
            label16.Name = "label16";
            label16.Size = new Size(84, 19);
            label16.TabIndex = 27;
            label16.Text = "Password :";
            // 
            // checkBox7
            // 
            checkBox7.AutoSize = true;
            checkBox7.Location = new Point(293, 153);
            checkBox7.Name = "checkBox7";
            checkBox7.Size = new Size(156, 23);
            checkBox7.TabIndex = 5;
            checkBox7.Text = "Random Dest Port";
            checkBox7.UseVisualStyleBackColor = true;
            // 
            // textBox10
            // 
            textBox10.Location = new Point(376, 87);
            textBox10.Name = "textBox10";
            textBox10.Size = new Size(160, 27);
            textBox10.TabIndex = 26;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(268, 90);
            label15.Name = "label15";
            label15.Size = new Size(102, 19);
            label15.TabIndex = 25;
            label15.Text = "Boardcast IP :";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(textBox7);
            groupBox5.Controls.Add(label12);
            groupBox5.Controls.Add(textBox6);
            groupBox5.Controls.Add(label11);
            groupBox5.Controls.Add(textBox5);
            groupBox5.Controls.Add(label10);
            groupBox5.Controls.Add(comboBox3);
            groupBox5.Controls.Add(label9);
            groupBox5.Location = new Point(287, 213);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(246, 158);
            groupBox5.TabIndex = 23;
            groupBox5.TabStop = false;
            groupBox5.Text = "HTTP";
            // 
            // textBox7
            // 
            textBox7.Location = new Point(83, 122);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(145, 27);
            textBox7.TabIndex = 24;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(40, 125);
            label12.Name = "label12";
            label12.Size = new Size(37, 19);
            label12.TabIndex = 23;
            label12.Text = "UA :";
            // 
            // textBox6
            // 
            textBox6.Location = new Point(83, 89);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(145, 27);
            textBox6.TabIndex = 22;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(26, 92);
            label11.Name = "label11";
            label11.Size = new Size(51, 19);
            label11.TabIndex = 21;
            label11.Text = "Page :";
            // 
            // textBox5
            // 
            textBox5.Location = new Point(83, 58);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(145, 27);
            textBox5.TabIndex = 20;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(6, 61);
            label10.Name = "label10";
            label10.Size = new Size(71, 19);
            label10.TabIndex = 19;
            label10.Text = "Domain :";
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(83, 26);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(145, 27);
            comboBox3.TabIndex = 8;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(17, 29);
            label9.Name = "label9";
            label9.Size = new Size(60, 19);
            label9.TabIndex = 7;
            label9.Text = "Action :";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(numericUpDown5);
            groupBox4.Controls.Add(numericUpDown4);
            groupBox4.Controls.Add(label14);
            groupBox4.Controls.Add(label13);
            groupBox4.Location = new Point(13, 276);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(246, 95);
            groupBox4.TabIndex = 22;
            groupBox4.TabStop = false;
            groupBox4.Text = "ICMP";
            // 
            // numericUpDown5
            // 
            numericUpDown5.Location = new Point(65, 56);
            numericUpDown5.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDown5.Name = "numericUpDown5";
            numericUpDown5.Size = new Size(163, 27);
            numericUpDown5.TabIndex = 31;
            // 
            // numericUpDown4
            // 
            numericUpDown4.Location = new Point(65, 23);
            numericUpDown4.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDown4.Name = "numericUpDown4";
            numericUpDown4.Size = new Size(163, 27);
            numericUpDown4.TabIndex = 30;
            numericUpDown4.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(6, 58);
            label14.Name = "label14";
            label14.Size = new Size(53, 19);
            label14.TabIndex = 27;
            label14.Text = "Code :";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(10, 25);
            label13.Name = "label13";
            label13.Size = new Size(49, 19);
            label13.TabIndex = 25;
            label13.Text = "Type :";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(numericUpDown3);
            groupBox3.Controls.Add(checkBox6);
            groupBox3.Controls.Add(label8);
            groupBox3.Controls.Add(checkBox5);
            groupBox3.Controls.Add(checkBox4);
            groupBox3.Controls.Add(checkBox3);
            groupBox3.Controls.Add(checkBox2);
            groupBox3.Controls.Add(checkBox1);
            groupBox3.Location = new Point(13, 120);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(246, 150);
            groupBox3.TabIndex = 21;
            groupBox3.TabStop = false;
            groupBox3.Text = "TCP Flag";
            // 
            // numericUpDown3
            // 
            numericUpDown3.Location = new Point(113, 30);
            numericUpDown3.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericUpDown3.Name = "numericUpDown3";
            numericUpDown3.Size = new Size(115, 27);
            numericUpDown3.TabIndex = 29;
            numericUpDown3.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            // 
            // checkBox6
            // 
            checkBox6.AutoSize = true;
            checkBox6.Location = new Point(113, 120);
            checkBox6.Name = "checkBox6";
            checkBox6.Size = new Size(60, 23);
            checkBox6.TabIndex = 4;
            checkBox6.Text = "URG";
            checkBox6.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(11, 32);
            label8.Name = "label8";
            label8.Size = new Size(81, 19);
            label8.TabIndex = 19;
            label8.Text = "TCP Wnd :";
            // 
            // checkBox5
            // 
            checkBox5.AutoSize = true;
            checkBox5.Location = new Point(113, 91);
            checkBox5.Name = "checkBox5";
            checkBox5.Size = new Size(68, 23);
            checkBox5.TabIndex = 3;
            checkBox5.Text = "PUSH";
            checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            checkBox4.AutoSize = true;
            checkBox4.Location = new Point(10, 120);
            checkBox4.Name = "checkBox4";
            checkBox4.Size = new Size(55, 23);
            checkBox4.TabIndex = 2;
            checkBox4.Text = "RST";
            checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.Location = new Point(10, 91);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new Size(52, 23);
            checkBox3.TabIndex = 2;
            checkBox3.Text = "FIN";
            checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(113, 62);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(57, 23);
            checkBox2.TabIndex = 1;
            checkBox2.Text = "ACK";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(10, 62);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(58, 23);
            checkBox1.TabIndex = 0;
            checkBox1.Text = "SYN";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(376, 54);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(160, 27);
            textBox3.TabIndex = 18;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(293, 57);
            label7.Name = "label7";
            label7.Size = new Size(77, 19);
            label7.TabIndex = 17;
            label7.Text = "End IPv4 :";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(287, 24);
            label6.Name = "label6";
            label6.Size = new Size(83, 19);
            label6.TabIndex = 16;
            label6.Text = "Start IPv4 :";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(376, 21);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(160, 27);
            textBox2.TabIndex = 15;
            textBox2.TextChanged += textBox2_TextChanged;
            // 
            // frmTipoff
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(566, 594);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(button1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmTipoff";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmTipoff";
            Load += frmTipoff_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown5).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown4).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Label label1;
        private ComboBox comboBox1;
        private ComboBox comboBox2;
        private Label label3;
        private NumericUpDown numericUpDown1;
        private NumericUpDown numericUpDown2;
        private Label label2;
        private Label label4;
        private GroupBox groupBox1;
        private Label label5;
        private TextBox textBox1;
        private GroupBox groupBox2;
        private Button button2;
        private TextBox textBox3;
        private Label label7;
        private Label label6;
        private TextBox textBox2;
        private GroupBox groupBox3;
        private Label label8;
        private CheckBox checkBox6;
        private CheckBox checkBox5;
        private CheckBox checkBox4;
        private CheckBox checkBox3;
        private CheckBox checkBox2;
        private CheckBox checkBox1;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private TextBox textBox5;
        private Label label10;
        private ComboBox comboBox3;
        private Label label9;
        private TextBox textBox6;
        private Label label11;
        private TextBox textBox7;
        private Label label12;
        private CheckBox checkBox7;
        private Label label14;
        private Label label13;
        private TextBox textBox10;
        private Label label15;
        private TextBox textBox11;
        private Label label16;
        private NumericUpDown numericUpDown3;
        private NumericUpDown numericUpDown4;
        private NumericUpDown numericUpDown5;
    }
}