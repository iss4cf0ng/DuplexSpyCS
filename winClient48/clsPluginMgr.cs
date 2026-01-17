using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Plugin.Abstractions48;

namespace winClient48
{
    public class clsPluginMgr
    {
        private readonly Dictionary<string, IPlugin> _plugins =
        new Dictionary<string, IPlugin>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, IPlugin> Plugins { get { return _plugins; } }

        private readonly IPluginContext _context;

        public clsPluginMgr(IPluginContext context)
        {
            _context = context;
        }

        public void Load(byte[] abRaw)
        {
            Assembly asm = Assembly.Load(abRaw);
            var type = asm.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

            if (type == null)
                throw new Exception("No valid plugin found!");

            var plugin = (IPlugin)Activator.CreateInstance(type);

            if (_plugins.ContainsKey(plugin.Name))
                throw new Exception("This plugin was already loaded.");

            plugin.Init(_context);

            _plugins.Add(plugin.Name, plugin);
        }

        public object Execute(string pluginName, IDictionary<string, object> args)
        {
            if (!_plugins.TryGetValue(pluginName, out var plugin))
                throw new KeyNotFoundException($"Plugin '{pluginName}' not loaded");

            return plugin.Execute(args);
        }

        public void Unload(string pluginName)
        {
            if (_plugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.Dispose();
                _plugins.Remove(pluginName);
            }
        }

        public void UnloadAll()
        {
            foreach (var p in _plugins.Values)
                p.Dispose();

            _plugins.Clear();
        }
    }
}
