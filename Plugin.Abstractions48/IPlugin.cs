using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Abstractions48
{
    public interface IPlugin : IDisposable
    {
        string Name { get; }
        Version Version { get; }

        void Init(IPluginContext context);
        object Execute(IDictionary<string, object> args);
    }
}
