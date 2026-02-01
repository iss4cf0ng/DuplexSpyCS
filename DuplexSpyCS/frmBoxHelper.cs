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
    public partial class frmBoxHelper : Form
    {
        public string readFile;

        public frmBoxHelper(string filename)
        {
            InitializeComponent();

            readFile = filename;
        }

        void setup()
        {
            Text = readFile;

            string path = Path.Combine(new string[] { Application.StartupPath, "Doc", "RTF", readFile + ".rtf" });

            if (File.Exists(path))
            {
                textBox1.Text = path;
                textBox1.Tag = path;

                richTextBox1.LoadFile(path);
            }
            else
            {
                MessageBox.Show("File not found:\n" + path, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void frmBoxHelper_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void frmBoxHelper_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.O:
                        string path = (string)textBox1.Tag;
                        if (!string.IsNullOrEmpty(path))
                            Process.Start("explorer.exe", @Path.GetDirectoryName(path));
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.F5:
                        setup();
                        break;
                }
            }
        }

        //Open Folder
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string path = (string)textBox1.Tag;
            if (!string.IsNullOrEmpty(path))
                Process.Start("explorer.exe", @Path.GetDirectoryName(path));
        }
        //Refresh RTF File
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            setup();
        }
    }
}
