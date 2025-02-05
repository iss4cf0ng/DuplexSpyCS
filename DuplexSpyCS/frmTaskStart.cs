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
    public partial class frmTaskStart : Form
    {
        public Victim v;

        public frmTaskStart()
        {
            InitializeComponent();
        }

        void Execute()
        {

        }

        void setup()
        {

        }

        private void frmTaskStart_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Execute();
        }
    }
}
