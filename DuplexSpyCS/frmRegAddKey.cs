using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmRegAddKey : Form
    {
        public frmManager f_mgr;
        public string currentPath;

        public clsVictim v;

        public frmRegAddKey()
        {
            InitializeComponent();
        }

        private void frmRegAddKey_Load(object sender, EventArgs e)
        {
            
        }

        //Generate Name
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = clsTools.GenerateFileName();
        }

        //Add
        private void button2_Click(object sender, EventArgs e)
        {
            f_mgr.Reg_ReqAddKey(currentPath, textBox1.Text);
        }
    }
}
