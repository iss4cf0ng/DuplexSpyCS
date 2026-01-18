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
                //string szTarget = args["target"].ToString();
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
                    string szAction = lsKey[2];
                    clsTools.fnLogInfo("Action => " + szAction);

                    if (szAction == "history")
                    {
                        var module = (clsChromeDumper)m_dicModule[szTarget];
                        var ls = module.fnlsDumpHistory(nCount);

                        foreach (var history in ls)
                        {
                            clsTools.fnLogOK(new string('=', 50));

                            clsTools.fnLogOK($"[Title]: {history.Title}");
                            clsTools.fnLogOK($"[URL]: {history.URL}");
                            clsTools.fnLogOK($"[Last Used]: {history.szLastUsed}");
                        }

                        clsTools.fnLogOK(new string('=', 50));
                        clsTools.fnLogOK("\t\t\t\t[Summary]");
                        clsTools.fnLogOK($"Total: {ls.Count} records");

                    }
                    else if (szAction == "cred")
                    {

                    }
                    else
                    {
                        clsTools.fnLogError("Invalid command: " + szAction);
                    }
                }
            }
            else
            {
                
            }

            return string.Empty;
        }

        public void Dispose()
        {
            _ctx.Log("Plugin Dispose");
        }
    }
}
