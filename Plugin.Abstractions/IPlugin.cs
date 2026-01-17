using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Abstractions
{
    public interface IPlugin : IDisposable
    {
        string Name { get; }
        Version Version { get; }

        void Init(IPluginContext context);
        object Execute(IDictionary<string, object> args);
    }
}
