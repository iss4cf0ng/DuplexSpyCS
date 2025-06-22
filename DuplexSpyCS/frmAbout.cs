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
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        void setup()
        {
            richTextBox1.Text = "" +
                "Author: ISSAC\n" +
                "Version: 1.0.0\n" +
                "Github: https://github.com/iss4cf0ng/DuplexSpyCS\n" +
                "";
        }

        private void frmAbout_Load(object sender, EventArgs e)
        {
            setup();
        }
    }
}
