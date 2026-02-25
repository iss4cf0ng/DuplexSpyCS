using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    public class clsPlugin
    {
        public clsPlugin()
        {

        }

        public struct stPluginCommand
        {
            public string name { get; set; }
            public List<string> args { get; set; }
            public string desc { get; set; }
        }

        public struct stPluginMeta
        {
            public bool bIsNull { get { return string.IsNullOrEmpty(Name); } }

            public string Name { get; set; }
            public string Version { get; set; }
            public string Description { get; set; }
            public string Author { get; set; }
            public string Entry { get; set; }

            public List<stPluginCommand> command { get; set; }
        }

        public struct stPluginInfo
        {
            public bool bIsValid { get { return !string.IsNullOrEmpty(szFileName) && !Meta.bIsNull; } }
            public byte[] fnabPluginBuffer() => File.ReadAllBytes(szFileName);

            public string szFileName;
            public stPluginMeta Meta;
        }

        public struct stCommandSpec
        {
            public string PluginName;
            public string Entry;
            public string Command;
            public List<string> Args;
            public string Description;
        }
    }
}
