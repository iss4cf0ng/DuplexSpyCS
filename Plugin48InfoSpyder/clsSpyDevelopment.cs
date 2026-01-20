using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin48InfoSpyder
{
    public class clsSpyDevelopment : clsSpy
    {
        private string[] m_asTool =
        {
            // Virtualization Tools
            "vmware",           // Virtual machine software
            "virtualbox",       // Virtual machine software
            "hyper-v",          // Windows Hyper-V virtualization tool

            // Git / Version Control
            "git",              // Git version control
            "sourcetree",       // Git GUI tool
            "tortoisegit",      // Git GUI tool

            // IDE / Development Tools
            "visual studio",    // Microsoft development IDE
            "vs code",          // Lightweight code editor by Microsoft
            "intellij",         // Java IDE
            "pycharm",          // Python IDE
            "eclipse",          // Java IDE
            "notepad++",        // Code editor
        };

        public clsSpyDevelopment()
        {
            szName = "InfoSpyder.Development";
            szEntry = "devtool";
            szDescription = "Development tools.";
        }

        public override void fnRun(string szModule, List<string> lsArgs)
        {
            if (lsArgs[0] == "ls")
            {
                clsTools.fnLogInfo(new string('-', 50));

                foreach (string szPattern in m_asTool)
                {
                    foreach (var app in m_lsApp)
                    {
                        if (Regex.IsMatch(app.Name, szPattern, RegexOptions.IgnoreCase))
                        {
                            clsTools.fnLogInfo($"[Name]: {app.Name}");
                            clsTools.fnLogInfo($"[Path]: {app.AppFolder}");
                            clsTools.fnLogInfo($"[Version]: {app.Version}");
                            clsTools.fnLogInfo($"[Install Date]: {app.InstallDate}");
                        }
                    }
                }
            }
        }
    }
}
