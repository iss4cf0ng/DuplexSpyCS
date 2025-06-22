using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    internal class C2
    {
        public static Listener listener;

        public static DateTime dtStartUp;
        public static SettingConfig settingConfig { get { return C1.GetConfigFromINI(); } }

        public static int sent_bytes;
        public static int recv_bytes;

        public static SqlConn sql_conn;
        public static IniManager ini_manager;

        public static ImageList il_extension = new ImageList();
        public static string[] imgs =
        {
            "png",
            "jpg",
            "ico",
            "svg",
            "bmp",
        };

        public static string BytesNormalize(long bytes_size)
        {
            if (bytes_size < 1024)
                return $"{bytes_size} Bytes";
            else if (bytes_size < 1024 * 1024)
                return $"{bytes_size / 1024.0:F2} KB";
            else if (bytes_size < 1024 * 1024 * 1024)
                return $"{bytes_size / (1024.0 * 1024):F2} MB";
            else if (bytes_size < 1024L * 1024 * 1024 * 1024)
                return $"{bytes_size / (1024.0 * 1024 * 1024):F2} GB";
            else
                return $"{bytes_size / (1024.0 * 1024 * 1024 * 1024):F2} TB";
        }
        public static string ImageToBase64(string file)
        {
            return ImageToBase64(Image.FromFile(file));
        }
        public static string ImageToBase64(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, img.RawFormat);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
        public static bool FileIsImage(string filename)
        {
            string name = Path.GetExtension(filename).Replace(".", string.Empty);
            return imgs.Contains(name);
        }
    }
}
