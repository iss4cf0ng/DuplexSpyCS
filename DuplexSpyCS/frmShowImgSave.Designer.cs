namespace DuplexSpyCS
{
    partial class frmShowImgSave
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
            progressBar1 = new ProgressBar();
            richTextBox1 = new RichTextBox();
            SuspendLayout();
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 400);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(368, 43);
            progressBar1.TabIndex = 0;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(12, 12);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(368, 382);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = "";
            // 
            // frmShowImgSave
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(392, 455);
            Controls.Add(richTextBox1);
            Controls.Add(progressBar1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            Name = "frmShowImgSave";
            Text = "frmShowImgSave";
            Load += frmShowImgSave_Load;
            ResumeLayout(false);
        }

        #endregion

        private ProgressBar progressBar1;
        private RichTextBox richTextBox1;
    }
}