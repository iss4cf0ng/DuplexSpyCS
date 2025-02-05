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
    public partial class frmWindowCapture : Form
    {
        public frmWindowCapture()
        {
            InitializeComponent();
        }

        public void ShowImage(string handle, Image img)
        {
            TabPage page = new TabPage();
            PictureBox pb = new PictureBox();
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            pb.Image = img;
            page.Controls.Add(pb);
            tabControl1.TabPages.Add(page);

            pb.Dock = DockStyle.Fill;
        }

        private void frmWindowCapture_Load(object sender, EventArgs e)
        {

        }
    }
}
