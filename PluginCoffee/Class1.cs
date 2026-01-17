using Plugin.Abstractions;

namespace PluginCoffee
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
            return $"Hello {coffee}";
        }

        public void Dispose()
        {
            _ctx.Log("Plugin Dispose");
        }
    }
}
