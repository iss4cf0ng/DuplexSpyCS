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
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
