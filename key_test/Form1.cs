using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace key_test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Import GetAsyncKeyState from user32.dll
        [DllImport("user32.dll", SetLastError = true)]
        public static extern short GetAsyncKeyState(int vKey);

        // Virtual Key Codes
        const int VK_A = 0x41;   // 'A' Key
        const int VK_B = 0x42;   // 'B' Key
        const int VK_ENTER = 0x0D; // Enter key

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine($"Key Pressed: {e.KeyCode}, Virtual Key Code: {e.KeyValue}");

            // Alternatively, you can use GetAsyncKeyState to detect the key press
            short keyState = GetAsyncKeyState(e.KeyValue);
            Console.WriteLine($"GetAsyncKeyState for {e.KeyCode}: {keyState}");

            // Optionally, check if the key is down (the high-order bit is set to 1)
            if ((keyState & 0x8000) != 0)
            {
                Console.WriteLine($"The key {e.KeyCode} is currently being pressed.");
            }
        }
    }
}
