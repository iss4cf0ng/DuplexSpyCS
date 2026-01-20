using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin48InfoSpyder
{
    internal class clsSpyBrowser : clsSpy
    {
        private string[] m_asTool =
        {
            // Browsers
            "chrome",           // Google Chrome
            "firefox",          // Mozilla Firefox
            "edge",             // Microsoft Edge
            "opera",            // Opera browser
            "brave",            // Brave browser
        };

        public clsSpyBrowser()
        {
            szName = "InfoSpyder.Browser";
            szEntry = "browser";
            szDescription = "Browsers";
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

                            clsTools.fnLogInfo(new string('-', 50));
                        }
                    }
                }
            }
        }
    }
}
