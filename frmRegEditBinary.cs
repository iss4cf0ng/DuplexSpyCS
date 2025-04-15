using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DuplexSpyCS
{
    public partial class frmRegEditBinary : Form
    {
        public frmManager f_mgr;

        public string regFullPath;
        public string valName;

        public frmRegEditBinary()
        {
            InitializeComponent();
        }

        void setup()
        {

        }

        private void frmRegEditBinary_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] payload = null;
            int binLen = textBox2.Text.Length;
        }
    }
}
