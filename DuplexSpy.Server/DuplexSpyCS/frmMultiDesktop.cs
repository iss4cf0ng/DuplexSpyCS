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
    public partial class frmMultiDesktop : Form
    {
        private int idx_page = 1;
        public List<clsVictim> l_victim;

        public frmMultiDesktop()
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

            timer1.Start();

            toolStripStatusLabel1.Text = $"Screen[{l_victim.Count}]";
            toolStripStatusLabel3.Text = string.Empty;
        }

        private void frmMultiDesktop_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int idx_start = (idx_page - 1) * 9;
            for (int i = 0; i < 9 && idx_start + i < l_victim.Count; i++)
            {
                clsVictim v = l_victim[idx_start + i];
                int row = i / 3;
                int col = i % 3;
                PictureBox pb = (PictureBox)tableLayoutPanel1.Controls[i];
                pb.Image = v.img_LastDesktop;
            }
        }

        private void pb_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            if (pb.Tag != null && pb.Image != null)
            {
                int idx_start = (idx_page - 1) * 9;
                int idx = (int)pb.Tag;

                clsVictim victim = l_victim[idx + idx_start];
                frmDesktop f = clsTools.fnFindForm<frmDesktop>(victim);
                if (f == null)
                {
                    f = new frmDesktop(victim);
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        private void tableLayoutPanel1_DoubleClick(object sender, EventArgs e)
        {
            
        }

        //Next Page
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int idx_start = idx_page * 9;
            if (idx_start <= l_victim.Count)
            {
                AllClear();
                idx_page++;
            }
        }
        //Previous Page
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (idx_page - 1 > 0)
            {
                AllClear();
                idx_page--;
            }
        }
    }
}
