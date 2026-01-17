using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Init(IPluginContext context)
        {
            _ctx = context;
            _ctx.Log("Plugin Init");
        }

        public object Execute(IDictionary<string, object> args)
        {
            List<string> lsKey = args.Keys.ToList();
            string szCmd = lsKey[0];

            if (szCmd == "ls")
            {
                //Detect and show exploitable method.
                return string.Empty;
            }
            else
            {
                return "Invalid parameter: " + szCmd;
            }
        }

        public void Dispose()
        {
            _ctx.Log("Plugin Dispose");
        }
    }
}
