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
    public partial class frmFilePaste : Form
    {
        public Victim v;
        public frmManager f_mgr;

        public frmFilePaste()
        {
            InitializeComponent();
        }

        public void ShowResult(List<string[]> entries)
        {
            Invoke(new Action(() =>
            {
                listView1.Items.Clear();

                ListViewGroup gFolder = new ListViewGroup("Folder");
                ListViewGroup gFile = new ListViewGroup("File");

                foreach (string[] entry in entries)
                {
                    string name = entry[2];
                    if (string.IsNullOrEmpty(name))
                        continue;

                    string type = entry[1];
                    string msg = entry[3];
                    ListViewItem item = new ListViewItem(name);
                    item.Group = type == "d" ? gFolder : gFile;
                    item.SubItems.Add(msg);
                    if (Path.GetPathRoot(msg) == null) //NOT VALID PATH -> ERROR MESSAGE
                        item.SubItems.Add("ERROR");
                    else
                        item.SubItems.Add("OK");
                    listView1.Items.Add(item);
                }

                f_mgr.fileLV_Refresh();
            }));
        }

        private void frmFilePaste_Load(object sender, EventArgs e)
        {

        }
    }
}
