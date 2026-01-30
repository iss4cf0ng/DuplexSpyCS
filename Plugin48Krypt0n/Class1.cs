using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin48Krypt0n
{
    public class PluginKrypt0n : IPlugin
    {
        private IPluginContext _ctx;
        private DataTable _table;

        public string Name => "Krypt0n";
        public Version Version => new Version(1, 0, 0);
        public PluginAttribute Attribute { get; set; }

        public DataTable HelpTable => _table;

        public void Init(IPluginContext context)
        {
            _ctx = context;
            _ctx.Log("Plugin Init");

            Attribute = new PluginAttribute()
            {
                Author = "ISSAC",
                Description = "Simple ransomware.",
                Usage = "krypt0n command",
            };

            _table = new DataTable();
            _table.Columns.Add("Command");
            _table.Columns.Add("Description");


        }

        public object Execute(IDictionary<string, object> args)
        {
            List<string> lsKey = args.Keys.ToList();
            if (lsKey.Count == 0)
                throw new Exception(Attribute.Usage);

            string szCmd = lsKey[0];

            if (szCmd == "help")
            {

            }
            else if (szCmd == "show")
            {

            }
            else if (szCmd == "set")
            {

            }
            else if (szCmd == "run")
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
