using Plugin.Abstractions48;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plugin48InfoSpyder
{
    internal class clsSpyGame : clsSpy
    {
        private string[] m_asApp = 
        {
            // Gaming / Game Platforms
            "steam",            // Steam game client
            "epic games",       // Epic Games Launcher
            "origin",           // EA Origin Launcher
            "ubisoft connect",  // Ubisoft Connect / Uplay
            "battle.net",       // Blizzard Battle.net client
            "gog galaxy",       // GOG Galaxy client
            "riot client"       // Riot Games client (League of Legends, Valorant)
        };

        private DataTable dtHelp = new DataTable();

        public clsSpyGame()
        {
            szName = "InfoSpyder.Game";
            szEntry = "game";
            szDescription = "Show installed games.";

            dtHelp.Columns.Add("Command");
            dtHelp.Columns.Add("Description");

            dtHelp.Rows.Add("help", "Print help message.");
            dtHelp.Rows.Add("ls", "Show information.");
        }

        public override void fnRun(string szModule, List<string> lsArgs)
        {
            if (lsArgs.Count == 0)
            {
                Console.WriteLine("<...> game <help | ls>");
                clsTools.fnPrintTable(dtHelp);
                return;
            }

            if (lsArgs[0] == "help")
            {
                Console.WriteLine("<...> game <help | ls>");
                clsTools.fnPrintTable(dtHelp);
            }
            else if (lsArgs[0] == "ls")
            {
                clsTools.fnLogInfo(new string('-', 50));

                foreach (string szPattern in m_asApp)
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
