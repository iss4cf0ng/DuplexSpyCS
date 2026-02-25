using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmShowImgSave : Form
    {
        public int img_cnt;
        private clsVictim v;

        public frmShowImgSave(clsVictim v)
        {
            InitializeComponent();
            
            this.v = v;
        }

        public void UpdateProgress(string msg)
        {
            richTextBox1.AppendText(msg);
            richTextBox1.AppendText(Environment.NewLine);
            progressBar1.Increment(1);
        }

        void setup()
        {
            progressBar1.Maximum = img_cnt;
            progressBar1.Value = 0;
        }

        private void frmShowImgSave_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string szDirPath = Path.Combine(v.dir_victim, "Images");
            if (Directory.Exists(szDirPath))
                Process.Start("explorer.exe", $"\"{szDirPath}\"");
        }
    }
}
