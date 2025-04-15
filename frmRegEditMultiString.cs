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
    public partial class frmRegEditMultiString : Form
    {
        public frmManager f_mgr;

        public string regFullPath;
        public string valName;
        public string valData;

        public frmRegEditMultiString()
        {
            InitializeComponent();
        }

        void setup()
        {
            textBox1.Text = valName;
            textBox1.ReadOnly = true;

            foreach (string s in valData.Split('\n'))
            {
                textBox2.AppendText(s);
                textBox2.AppendText(Environment.NewLine);
            }
        }

        private void frmRegEditMultiString_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            valData = textBox2.Text;
            new Thread(() => f_mgr.Reg_ReqEditValue(regFullPath, valName, Microsoft.Win32.RegistryValueKind.MultiString, valData)).Start();
        }
    }
}
