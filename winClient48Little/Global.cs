using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace winClient48Small
{
    public class Global
    {
        public static string GetActiveWindowTitle()
        {
            const int nChars = 512;
            StringBuilder sb = new StringBuilder(nChars);
            IntPtr handle = WinAPI.GetForegroundWindow();

            if (WinAPI.GetWindowText(handle, sb, nChars) > 0)
                return sb.ToString();
            else
                return "";
        }
    }
}
