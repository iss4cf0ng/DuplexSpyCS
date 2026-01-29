namespace DuplexSpyCS
{
    partial class frmNewVictim
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
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.BackColor = Color.Black;
            button1.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            button1.Location = new Point(283, 0);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(46, 42);
            button1.TabIndex = 0;
            button1.Text = "X";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 70);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(51, 19);
            label1.TabIndex = 1;
            label1.Text = "label1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 100);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(51, 19);
            label2.TabIndex = 2;
            label2.Text = "label2";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(13, 132);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(51, 19);
            label3.TabIndex = 3;
            label3.Text = "label3";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(13, 161);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(51, 19);
            label4.TabIndex = 4;
            label4.Text = "label4";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(13, 193);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(51, 19);
            label5.TabIndex = 5;
            label5.Text = "label5";
            // 
            // frmNewVictim
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(64, 64, 64);
            ClientSize = new Size(329, 288);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4);
            Name = "frmNewVictim";
            Text = "frmNewVictim";
            TransparencyKey = Color.FromArgb(64, 64, 64);
            Load += frmNewVictim_Load;
            LocationChanged += frmNewVictim_LocationChanged;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
    }
}