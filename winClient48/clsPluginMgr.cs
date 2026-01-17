using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        private string fnPrintHelp(string szDescription, string szUsage, DataTable dt)
        {
            if (dt == null || dt.Columns.Count == 0)
                return string.Empty;

            int[] colWidths = new int[dt.Columns.Count];

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                colWidths[i] = dt.Columns[i].ColumnName.Length;

                foreach (DataRow row in dt.Rows)
                {
                    int len = row[i]?.ToString().Length ?? 0;
                    if (len > colWidths[i])
                        colWidths[i] = len;
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(szDescription);
            sb.AppendLine(szUsage);

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sb.Append(dt.Columns[i].ColumnName.PadRight(colWidths[i] + 2));
            }
            sb.AppendLine();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sb.Append(new string('-', colWidths[i]) + "  ");
            }
            sb.AppendLine();

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string text = row[i]?.ToString() ?? "";
                    sb.Append(text.PadRight(colWidths[i] + 2));
                }
                sb.AppendLine();
            }

            return sb.ToString();
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

            if (args.TryGetValue("help", out _))
                return fnPrintHelp(plugin.Attribute.Description, plugin.Attribute.Usage, plugin.HelpTable);

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
