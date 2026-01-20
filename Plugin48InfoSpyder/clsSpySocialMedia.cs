using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin48InfoSpyder
{
    internal class clsSpySocialMedia : clsSpy
    {
        private string[] m_asTool =
        {
            // Social / Communication Tools
            "slack",            // Team collaboration software
            "teams",            // Microsoft Teams
            "discord",          // Communication software
            "skype",            // Communication software
            "zoom",             // Video conferencing software
            "wechat",           // Instant messaging software
            "line",             // Instant messaging software
            "telegram",         // Instant messaging software
        };

        public clsSpySocialMedia()
        {
            szName = "InfoSpyder.SocialMedia";
            szEntry = "social";
            szDescription = "Social medial applications.";
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
