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
    public partial class frmKillProc : Form
    {
        public List<string> lsProc = new List<string>();

        public frmKillProc()
        {
            InitializeComponent();
        }

        void setup()
        {
            foreach (string szProc in lsProc)
                listView1.Items.Add(szProc);

            toolStripStatusLabel1.Text = $"Process[{listView1.Items.Count}] Selected[{listView1.SelectedItems.Count}]";
        }

        private void frmKillProc_Load(object sender, EventArgs e)
        {
            setup();
        }

        //Add
        private void button1_Click(object sender, EventArgs e)
        {
            string[] aProc = textBox1.Lines.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            listView1.Items.AddRange(aProc.Select(x => new ListViewItem(x)).ToArray());
        }
        //Remove selected
        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
                listView1.Items.Remove(item);
        }
        //Select all
        private void button4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Selected = true;
        }
        //Unselect All
        private void button3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Selected = false;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = $"Process[{listView1.Items.Count}] Selected[{listView1.SelectedItems.Count}]";
        }

        //OK
        private void button5_Click(object sender, EventArgs e)
        {
            lsProc = listView1.Items.Cast<ListViewItem>().Select(x => x.Text).ToList();
            ActiveForm.Close();
        }
    }
}
