using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Abstractions48
{
    public interface IPluginContext
    {
        void Log(string message);
        IDictionary<string, object> SharedData { get; }
    }
}
