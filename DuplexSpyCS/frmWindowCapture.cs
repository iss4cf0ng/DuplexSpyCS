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
        public clsVictim v;

        public frmWindowCapture()
        {
            InitializeComponent();
        }

        public void ShowImage(string handle, string title, Image img)
        {
            TabPage page = new TabPage();
            PictureBox pb = new PictureBox();
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            pb.Image = img;
            page.Text = title;
            page.Controls.Add(pb);
            tabControl1.TabPages.Add(page);

            pb.Dock = DockStyle.Fill;
        }

        private void frmWindowCapture_Load(object sender, EventArgs e)
        {

        }

        //Save selected
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                TabPage page = tabControl1.SelectedTab;
                if (page == null)
                {
                    MessageBox.Show("This is empty", "Nothing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = v.dir_victim;
                sfd.FileName = clsTools.GenerateFileName("jpg");
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    Image img = ((PictureBox)page.Controls[0]).Image;
                    if (img == null)
                    {
                        MessageBox.Show("Null image", "Nothing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    img.Save(sfd.FileName);
                    MessageBox.Show("Save image successfully: " + sfd.FileName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //Save all
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                string szDir = Path.Combine(v.dir_victim, "WindowCapture");
                if (!Directory.Exists(szDir))
                {
                    Directory.CreateDirectory(szDir);
                    if (!Directory.Exists(szDir))
                    {
                        MessageBox.Show("Create directory failed, process terminated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                foreach (TabPage page in tabControl1.TabPages)
                {
                    string szFileName = page.Text.Contains("?") ? clsTools.GenerateFileName("jpg") : $"{page.Text}.jpg";
                    Image img = ((PictureBox)page.Controls[0]).Image;
                    if (img == null)
                    {
                        MessageBox.Show($"Image at: {page.Text} is null", "Null image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    img.Save(szFileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Remove selected
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 0)
                tabControl1.TabPages.Remove(tabControl1.SelectedTab);
        }
        //Remove all
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            foreach (TabPage page in tabControl1.TabPages)
                tabControl1.TabPages.Remove(page);
        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.W)
                {
                    if (tabControl1.TabPages.Count > 0)
                        tabControl1.TabPages.Remove(tabControl1.SelectedTab);
                }
            }
            else
            {

            }
        }
    }
}
