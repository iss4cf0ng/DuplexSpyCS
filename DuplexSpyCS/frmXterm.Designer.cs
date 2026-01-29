namespace DuplexSpyCS
{
    partial class frmXterm
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
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            textBox1 = new TextBox();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            SuspendLayout();
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Location = new Point(13, 48);
            webView21.Margin = new Padding(4);
            webView21.Name = "webView21";
            webView21.Size = new Size(794, 364);
            webView21.TabIndex = 0;
            webView21.ZoomFactor = 1D;
            webView21.Resize += webView21_Resize;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.Black;
            textBox1.ForeColor = Color.White;
            textBox1.Location = new Point(13, 13);
            textBox1.Margin = new Padding(4);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(642, 27);
            textBox1.TabIndex = 1;
            // 
            // button1
            // 
            button1.BackColor = Color.Black;
            button1.ForeColor = Color.White;
            button1.Location = new Point(663, 11);
            button1.Margin = new Padding(4);
            button1.Name = "button1";
            button1.Size = new Size(144, 29);
            button1.TabIndex = 2;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // frmXterm
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(820, 425);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(webView21);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            Margin = new Padding(4);
            Name = "frmXterm";
            Text = "frmXterm";
            FormClosed += frmXterm_FormClosed;
            Load += frmXterm_Load;
            SizeChanged += frmXterm_SizeChanged;
            Resize += frmXterm_Resize;
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private TextBox textBox1;
        private Button button1;
    }
}