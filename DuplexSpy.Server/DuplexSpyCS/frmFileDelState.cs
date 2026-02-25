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
    public partial class frmFileDelState : Form
    {
        public clsVictim v;
        public frmManager f_mgr;

        public List<string> l_folder;
        public List<string> l_file;

        public frmFileDelState()
        {
            InitializeComponent();
        }

        public void ShowDelState(List<string[]> l_d, List<string[]> l_f)
        {
            Invoke(new Action(() =>
            {
                List<ListViewItem> l_folder = listView1.Items.Cast<ListViewItem>().Where(x => x.SubItems[1].Text == "Directory").ToList();
                List<ListViewItem> l_file = listView1.Items.Cast<ListViewItem>().Where(x => x.SubItems[1].Text == "File").ToList();

                bool lvItemMatch(ListViewItem item, string pattern)
                {
                    return string.Equals(item.Tag.ToString(), pattern, StringComparison.InvariantCultureIgnoreCase);
                }

                void func(string[] entry, List<ListViewItem> entries)
                {
                    foreach (ListViewItem item in entries)
                    {
                        if (lvItemMatch(item, entry[1]))
                        {
                            item.SubItems[2].Text = entry[2] == string.Empty ? "OK" : clsCrypto.b64D2Str(entry[2]);
                            break;
                        }
                    }
                }

                foreach (string[] folder in l_d)
                    func(folder, l_folder);
                foreach (string[] file in l_f)
                    func(file, l_file);

                f_mgr.fileLV_Refresh();
            }));
        }

        void setup()
        {
            foreach (string folder in l_folder)
            {
                ListViewItem item = new ListViewItem(Path.GetFileName(folder));
                item.SubItems.Add("Directory");
                item.SubItems.Add("?");
                item.Tag = folder;
                listView1.Items.Add(item);
            }
            foreach (string file in l_file)
            {
                ListViewItem item = new ListViewItem(Path.GetFileName(file));
                item.SubItems.Add("File");
                item.SubItems.Add("?");
                item.Tag = file;
                listView1.Items.Add(item);
            }
        }

        private void frmFileDelState_Load(object sender, EventArgs e)
        {
            setup();
        }
    }
}
