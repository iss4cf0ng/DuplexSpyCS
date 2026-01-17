using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;
using Plugin.Abstractions48;

namespace Plugin48Coffee
{
    public class PluginCoffee : IPlugin
    {
        private IPluginContext _ctx;

        public string Name => "Coffee";
        public Version Version => new Version(1, 0, 0);

        public void Init(IPluginContext context)
        {
            _ctx = context;
            _ctx.Log("Plugin Init");
        }

        public object Execute(IDictionary<string, object> args)
        {
            string coffee = (string)args["test"];
            _ctx.Log($"Execute for {coffee}");

            return $"Hello {coffee}";
        }

        public void Dispose()
        {
            _ctx.Log("Plugin Dispose");
        }
    }
}
