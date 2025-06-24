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
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(71, 68);
            button1.Name = "button1";
            button1.Size = new Size(148, 47);
            button1.TabIndex = 0;
            button1.Text = "Go";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // frmTipoff
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(425, 416);
            Controls.Add(button1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            Name = "frmTipoff";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmTipoff";
            Load += frmTipoff_Load;
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
    }
}