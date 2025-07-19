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
    public partial class frmListener : Form
    {
        public frmListener()
        {
            InitializeComponent();
        }

        void fnSetup()
        {
            //List all listener


            toolStripStatusLabel1.Text = $"Listener[{listView1.Items.Count}]";
        }

        private void frmListener_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
