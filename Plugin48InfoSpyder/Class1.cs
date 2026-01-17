using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        public void Init(IPluginContext context)
        {
            _ctx = context;
            _ctx.Log("Plugin Init");
        }

        public object Execute(IDictionary<string, object> args)
        {

            return string.Empty;
        }

        public void Dispose()
        {
            _ctx.Log("Plugin Dispose");
        }
    }
}
