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
    public partial class frmNewVictim : Form
    {
        public frmNewVictim()
        {
            InitializeComponent();
        }

        public void SetInfo(clsVictim v, string ip, string os)
        {
            label1.Text = v.ID;
            label2.Text = ip;
            label3.Text = ""; //LOCATION
            label4.Text = os;
        }

        private void frmNewVictim_Load(object sender, EventArgs e)
        {
            Screen screen = Screen.PrimaryScreen;
            Rectangle rect = screen.Bounds;
            Size size = Size;
            Point point = new Point(rect.Width - size.Width, rect.Height - size.Height);
            Location = point;
        }

        private void frmNewVictim_LocationChanged(object sender, EventArgs e)
        {
            
        }
    }
}
