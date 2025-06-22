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
    public partial class frmInfoBox : Form
    {
        public frmInfoBox()
        {
            InitializeComponent();
        }

        public void ShowInfo(string szContent, string szCaption, Icon icon)
        {
            Invoke(new Action(() =>
            {
                pictureBox1.Image = icon.ToBitmap();
                Text = szCaption;
                richTextBox1.Text = szContent;
            }));
        }

        void setup()
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            richTextBox1.WordWrap = false;
        }

        private void frmInfoBox_Load(object sender, EventArgs e)
        {
            setup();
        }
    }
}
