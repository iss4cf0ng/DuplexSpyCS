using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmClientConfig : Form
    {
        public Victim v;

        public frmClientConfig()
        {
            InitializeComponent();
        }

        public void ShowConfig(ClientConfig config)
        {
            
        }

        void setup()
        {
            
        }

        private void frmClientConfig_Load(object sender, EventArgs e)
        {
            setup();
        }
    }
}
