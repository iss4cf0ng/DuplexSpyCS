using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    internal class clsStore
    {
        public static clsTcpListener listener;

        public static DateTime dtStartUp;
        public static SettingConfig settingConfig { get { return clsTools.GetConfigFromINI(); } }

        public static int sent_bytes;
        public static int recv_bytes;

        public static clsSqlConn sql_conn;
        public static clsIniManager ini_manager;

        public static ImageList il_extension = new ImageList();
        public static string[] imgs =
        {
            "png",
            "jpg",
            "ico",
            "svg",
            "bmp",
        };
    }
}
