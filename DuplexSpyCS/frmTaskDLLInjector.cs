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
        public frmTaskDLLInjector()
        {
            InitializeComponent();
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

            }
        }
    }
}
