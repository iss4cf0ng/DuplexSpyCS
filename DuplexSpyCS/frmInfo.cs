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
    public partial class frmInfo : Form
    {
        public clsVictim v;
        private Dictionary<string, string> dic_Screen = new Dictionary<string, string>();

        public frmInfo(clsVictim victim)
        {
            InitializeComponent();

            v = victim;
        }

        private ColumnHeader FindColumnHeaderByText(ListView lv, string text)
        {
            ColumnHeader header = null;
            Invoke(new Action(() =>
            {
                foreach (ColumnHeader h in lv.Columns)
                {
                    if (string.Equals(h.Text, text))
                    {
                        header = h;
                        break;
                    }
                }
            }));

            return header;
        }

        public void ShowInfo(string szInfo)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    richTextBox1.Text = szInfo;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                clsStore.sql_conn.WriteSysErrorLogs(ex.Message);
            }
        }

        public void ShowPatch(DataTable dt)
        {
            Invoke(new Action(() =>
            {
                foreach (DataColumn dc in dt.Columns)
                    listView1.Columns.Add(new ColumnHeader() { Text = dc.ColumnName, });

                foreach (DataRow dr in dt.Rows)
                {
                    string[] aVals = dr.ItemArray.Select(x => x.ToString()).ToArray();
                    ListViewItem item = new ListViewItem(aVals[0]);
                    for (int i = 1; i < aVals.Length; i++)
                        item.SubItems.Add(aVals[i]);

                    listView1.Items.Add(item);
                }

                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                toolStripStatusLabel1.Text = $"Item[{listView1.Items.Count}]";
            }));
        }

        void setup()
        {
            toolStripStatusLabel1.Text = string.Empty;

            v.SendCommand("detail|pc|info");
        }

        private void frmInfo_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\Info").Show();
        }
    }
}
