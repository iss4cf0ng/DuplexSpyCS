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
    public partial class frmMultiFile : Form
    {
        public delegate void UploadFile(List<string[]> l_path, string tgt_dir);
        public Dictionary<Victim, UploadFile> dic_uf = new Dictionary<Victim, UploadFile>();

        public frmMultiFile()
        {
            InitializeComponent();
        }

        void SendFile(string files, string tgt)
        {
            
        }
        List<string[]> DirRecurrence(string dir, List<string[]> l_files)
        {
            foreach (string d in Directory.GetDirectories(dir))
            {
                DirRecurrence(d, l_files);
            }
            foreach (string f in Directory.GetFiles(dir))
            {
                l_files.Add(new string[]
                {
                    string.Empty,
                    f,
                });
            }

            return l_files;
        }
        void SendDir(string dir, string tgt)
        {
            List<string[]> l_files = DirRecurrence(dir, new List<string[]>());
            
        }

        void setup()
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            foreach (Victim v in dic_uf.Keys)
            {
                ListViewItem item = new ListViewItem(v.ID);
                item.SubItems.Add("?");
                listView1.Items.Add(item);
            }
        }

        private void frmMultiFile_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0) //FILE
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = ofd.FileName;
                }
            }
            else //FOLDER
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = fbd.SelectedPath;
                }
            }
        }

        //SEND FILE
        private void button2_Click(object sender, EventArgs e)
        {
            string path = textBox1.Text;
            string dir = textBox2.Text;
            switch (comboBox1.SelectedIndex)
            {
                case 0: //FILE
                    new Thread(() => SendFile(path, dir)).Start();
                    break;
                case 1: //FOLDER
                    new Thread(() => SendDir(path, dir)).Start();
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:

                    break;
                case 1:

                    break;
            }
        }
    }
}
