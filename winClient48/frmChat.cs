using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace winClient48
{
    public partial class frmChat : Form
    {
        private bool allow_close = false;
        public clsVictim v;

        public frmChat()
        {
            InitializeComponent();
        }

        public void CloseForm()
        {
            allow_close = true;
            timer1.Stop();
            Close();
        }

        public void ShowMsg(string user, string msg)
        {
            string text = $"{user}[{DateTime.Now.ToString("t")}] : {msg}";
            Invoke(new Action(() =>
            {
                richTextBox1.AppendText(text);
                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        void SendMsg()
        {
            Invoke(new Action(() =>
            {
                string msg = textBox1.Text.Trim();
                v.SendCommand("chat|msg|" + clsCrypto.b64E2Str(msg));
                ShowMsg("You", msg);
                textBox1.Text = string.Empty;
            }));
        }

        void setup()
        {
            v.SendCommand("chat|init");
            timer1.Start();
        }

        private void frmChat_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void frmChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !allow_close;
        }

        private void frmChat_MinimumSizeChanged(object sender, EventArgs e)
        {
            MinimizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendMsg();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMsg();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            BringToFront();
            Activate();
        }
    }
}
