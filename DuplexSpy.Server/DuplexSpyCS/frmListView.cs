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
    public partial class frmListView : Form
    {
        public frmListView()
        {
            InitializeComponent();
        }
        public frmListView(string[] aColumns)
        {
            InitializeComponent();

            listView1.Columns.Clear();
            foreach (string szColumn in aColumns)
            {
                listView1.Columns.Add(new ColumnHeader()
                {
                    Text = szColumn,
                    Width = 200,
                });
            }
        }

        public void ShowInfo(List<Tuple<string, string>> lsInfo)
        {
            foreach (var x in lsInfo)
            {
                ListViewItem item = new ListViewItem(x.Item1);
                item.SubItems.Add(x.Item2);

                Invoke(new Action(() => listView1.Items.Add(item)));
            }
        }
        public void ShowInfo(List<string[]> lsInfo)
        {
            foreach (string[] x in lsInfo)
            {
                ListViewItem item = new ListViewItem(x[0]);
                for (int i = 1; i < x.Length; i++)
                {
                    item.SubItems.Add(x[i]);
                }

                Invoke(new Action(() => listView1.Items.Add(item)));
            }
        }

        void setup()
        {
            listView1.FullRowSelect = true;
        }

        private void frmDeviceInfo_Load(object sender, EventArgs e)
        {
            setup();
        }
    }
}
