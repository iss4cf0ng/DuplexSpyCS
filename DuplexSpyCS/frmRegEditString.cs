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
    public partial class frmRegEditString : Form
    {
        public frmManager f_mgr;

        public string regFullPath;
        public string valName;
        public string valData;

        public frmRegEditString()
        {
            InitializeComponent();
        }

        void setup()
        {
            textBox1.Text = valName;
            textBox2.Text = valData;

            textBox1.ReadOnly = true;
        }

        private void frmRegEditString_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            valData = textBox2.Text;
            new Thread(() => f_mgr.Reg_ReqEditValue(regFullPath, valName, Microsoft.Win32.RegistryValueKind.String, valData)).Start();
        }
    }
}
