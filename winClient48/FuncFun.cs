using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winClient48
{
    internal class FuncFun
    {
        public FuncFun()
        {

        }

        /// <summary>
        /// Return current wallpaper image object.
        /// </summary>
        /// <returns></returns>
        public (int, string, Image) ExportWallpaper()
        {
            int code = 1;
            string msg = string.Empty;
            Image img = null;

            try
            {

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg, img);
        }

        /// <summary>
        /// Change wallpaper from input image object.
        /// </summary>
        /// <returns></returns>
        public (int, string) ChangeWallpaper(Image img)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }

        public (int, string) LockScreen(Image img)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }

        public (int, string) UnlockScreen()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }


    }
}
