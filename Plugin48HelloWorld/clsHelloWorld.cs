using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugin.Abstractions48;

namespace Plugin48HelloWorld
{
    public class clsHelloWorld : IPlugin
    {
        private IPluginContext _ctx;
        private DataTable _table;

        public string Name => "InfoSpyder";
        public Version Version => new Version(1, 0, 0);
        public PluginAttribute Attribute { get; set; }

        public DataTable HelpTable => _table;

        public clsHelloWorld()
        {
            _table = new DataTable();
            _table.Columns.Add("Command");
            _table.Columns.Add("Description");

            _table.Rows.Add("help", "Print help message.");
        }

        public void Init(IPluginContext context)
        {
            _ctx = context;
            _ctx.Log("Plugin Init");


        }

        public object Execute(IDictionary<string, object> args)
        {
            /*
             * Your customized functions.
             */
            clsTools.fnLogOK("Hello World!");

            if (args.Count == 0)
                return string.Empty;
            else
            {
                object szInput = string.Empty;
                args.TryGetValue("input", out szInput);

                clsTools.fnLogInfo($"Your input: {szInput}");
            }

            return string.Empty;
        }

        public void Dispose()
        {
            _ctx.Log("Plugin Dispose");
        }
    }
}
