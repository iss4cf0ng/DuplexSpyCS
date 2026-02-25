using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace winClient48
{
    public class clsfnTask
    {
        public string GetProcess()
        {
            return null;
        }

        public (int, string) Kill(string name)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                var procs = Process.GetProcessesByName(name);
                foreach (Process p in procs)
                    p.Kill();
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) Kill(int id)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                var proc = Process.GetProcessById(id);
                proc.Kill();
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) KillDelete(int id)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                Process proc = Process.GetProcessById(id);
                string filename = proc.MainModule.FileName;
                proc.Kill();

                File.Delete(filename);
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) Start(string filename, string argv, string work_dir)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo()
                {
                    FileName = filename,
                    Arguments = argv,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WorkingDirectory = work_dir,
                };

                new Thread(() =>
                {
                    p.Start();
                    p.WaitForExit();
                });
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) Resume(int nProcessID)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                Process proc = Process.GetProcessById(nProcessID);
                foreach (ProcessThread tProc in proc.Threads)
                {
                    IntPtr hThread = WinAPI.OpenThread(WinAPI.THREAD_SUSPEND_RESUME, false, (uint)tProc.Id);
                    if (IntPtr.Zero == hThread)
                        throw new Exception("hThread is null handle.");

                    WinAPI.ResumeThread(hThread);
                    WinAPI.CloseHandle(hThread);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }

        public (int, string) Suspend(int nProcessID)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                Process proc = Process.GetProcessById(nProcessID);
                foreach (ProcessThread tProc in proc.Threads)
                {
                    IntPtr hThread = WinAPI.OpenThread(WinAPI.THREAD_SUSPEND_RESUME, false, (uint)tProc.Id);
                    if (IntPtr.Zero == hThread)
                        throw new Exception("tThread is null handle.");

                    WinAPI.SuspendThread(hThread);
                    WinAPI.CloseHandle(hThread);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }
    }
}
