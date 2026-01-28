using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Plugin48InfoSpyder
{
    public class PluginInfoSpyder : IPlugin
    {
        private IPluginContext _ctx;
        private DataTable _table;

        public string Name => "InfoSpyder";
        public Version Version => new Version(1, 0, 0);
        public PluginAttribute Attribute { get; set; }

        public DataTable HelpTable => _table;

        private List<clsSpy> m_lsModule = new List<clsSpy>()
        {
            new clsSpyBrowser(),
            new clsSpyDevelopment(),
            new clsSpyGame(),
            new clsSpyMachine(),
            new clsSpyRemoteMgr(),
        };

        private Dictionary<string, clsSpy> m_dicModule = new Dictionary<string, clsSpy>();

        public void Init(IPluginContext context)
        {
            _ctx = context;
            _ctx.Log("Plugin Init");

            Attribute = new PluginAttribute()
            {
                Author = "ISSAC",
                Description = "Extract information from local machine.",
                Usage = "infospyder <Entry> <Command>",
            };

            _table = new DataTable();
            _table.Columns.Add("Command");
            _table.Columns.Add("Description");

            _table.Rows.Add("ls", "Print available modules.");
            _table.Rows.Add("<entry>", "Use module.");

            foreach (var module in m_lsModule)
            {
                if (m_dicModule.ContainsKey(module.szName))
                    continue;

                m_dicModule.Add(module.szEntry, module);
            }
        }

        public object Execute(IDictionary<string, object> args)
        {
            List<string> lsKey = args.Keys.ToList();
            if (lsKey.Count == 0)
                throw new Exception(Attribute.Usage);

            string szCmd = lsKey[0];

            if (szCmd == "ls")
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Module");
                dt.Columns.Add("Entry");
                dt.Columns.Add("Description");

                foreach (var module in m_lsModule)
                    dt.Rows.Add(module.szName, module.szEntry, module.szDescription);

                clsTools.fnPrintTable(dt);
            }
            else
            {
                List<string> lsModule = m_lsModule.Select(x => x.szEntry).ToList();
                string szModule = szCmd;

                if (!lsModule.Contains(szModule))
                    throw new Exception("Module does not exists: " + szModule);

                var module = m_dicModule[szModule];
                module.fnRun(szModule, lsKey.Skip(1).ToList());
            }

            return string.Empty;
        }

        public void Dispose()
        {
            _ctx.Log("Plugin Dispose");
        }
    }
}
