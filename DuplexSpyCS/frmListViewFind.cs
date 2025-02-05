using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace DuplexSpyCS
{
    public partial class frmListViewFind : Form
    {
        public ListView lv;
        private List<ListViewItem> l_items = new List<ListViewItem>();
        private int idx_finditem = 0;
        private List<int> l_idx = new List<int>();
        private int idx_lIdx = 0;

        public frmListViewFind()
        {
            InitializeComponent();
        }

        private int FindListViewItemWithTextInRange(string pattern, int idx_start, int idx_end, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            int idx = -1;
            for (int i = idx_start; i < idx_end; i++)
            {
                ListViewItem item = l_items[i];
                if (item.Text.Contains(pattern, comparison))
                {
                    idx = i;
                    l_idx.Add(idx);
                    break;
                }
            }

            return idx;
        }

        private void FindItem(bool previous = false)
        {
            int idx = FindListViewItemWithTextInRange(textBox1.Text, idx_finditem, l_items.Count, radioButton1.Checked ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
            if (idx == -1)
            {
                MessageBox.Show("Cannot find any item", idx_finditem == 0 ? "Cannot find any item" : "No more", MessageBoxButtons.OK, MessageBoxIcon.Information);
                idx_finditem = 0;
                l_idx.Clear();
                return;
            }

            ListViewItem item = l_items[idx];
            l_items[idx_finditem == 0 ? 0 : idx_finditem - 1].Selected = false;
            item.Selected = true;
            item.EnsureVisible();
            idx_finditem = idx + 1;
        }

        void setup()
        {
            if (lv == null)
            {
                MessageBox.Show("Unexpected null form", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ActiveForm.Close();
            }

            foreach (ListViewItem item in lv.Items)
                l_items.Add(item);
        }

        private void frmListViewFind_Load(object sender, EventArgs e)
        {
            setup();
        }

        //PREVIOUS
        private void button1_Click(object sender, EventArgs e)
        {
            FindItem(true);
        }

        //NEXT
        private void button2_Click(object sender, EventArgs e)
        {
            FindItem();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FindItem(checkBox1.Checked);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            idx_finditem = 0;
            l_idx.Clear();
        }
    }
}
