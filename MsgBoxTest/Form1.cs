using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MsgBoxTest
{
    public partial class Form1 : Form
    {
        // Define necessary WinAPI functions
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MessageBox(IntPtr hWnd, string text, string caption, uint type);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int Width, int Height, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetActiveWindow();

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // Constants for the MessageBox type
        const uint MB_OK = 0x00000000;

        // Flags for SetWindowPos
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_NOSIZE = 0x0001;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Display MessageBox
            IntPtr hWnd = IntPtr.Zero;

            new Thread(() =>
            {
                hWnd = MessageBox(IntPtr.Zero, "This is a custom-positioned message box.", "MessageBox Example", MB_OK);
            }).Start();

            // Move the MessageBox window to a custom location (e.g., 500, 300)
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOZORDER | SWP_NOSIZE);

            // Optionally, we can retrieve the current position of the MessageBox
            RECT rect;
            GetWindowRect(hWnd, out rect);
        }
    }
}
