using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Abstractions48
{
    public interface IPlugin : IDisposable
    {
        string Name { get; }
        Version Version { get; }
        DataTable HelpTable { get; }

        PluginAttribute Attribute { get; set; }

        void Init(IPluginContext context);
        object Execute(IDictionary<string, object> args);
    }
}
