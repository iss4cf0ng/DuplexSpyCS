using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugin.Abstractions48;

namespace winClient48
{
    public class clsPluginContext : IPluginContext
    {
        public IDictionary<string, object> SharedData { get; } = new Dictionary<string, object>();

        public clsPluginContext()
        {

        }

        public void Log(string message)
        {
            Console.WriteLine("[PLUGIN] " + message);
        }
    }
}
