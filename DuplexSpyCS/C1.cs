using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    internal class C1
    {
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
        public static Form GetFormByVictim(Victim v, Function func)
        {
            Form form = null;
            foreach (Form f in Application.OpenForms)
            {
                FieldInfo field = f.GetType().GetField("v", BindingFlags.Public | BindingFlags.Instance);
                bool has_victim_field = field != null && field.FieldType == typeof(Victim);
                if (has_victim_field && f.Tag != null)
                {
                    Victim _v = (Victim)field.GetValue(f);
                    if ((Function)f.Tag == func && _v == v)
                    {
                        form = f;
                        break;
                    }
                }
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
                IniManager ini_manager = C2.ini_manager;
                string Read(string section, string key)
                {
                    return ini_manager.Read(section, key);
                }

                SettingConfig config = new SettingConfig()
                {
                    //General
                    dwListenPort = int.Parse(Read("General", "listen_port")),
                    dwTimeout = int.Parse(Read("General", "listen_timeout")),
                    szDbFile = Read("General", "db_file"),

                    //Crypto
                    crypto_szChallengeText = Read("Crypto", "challenge"),
                    crypto_bRandomChallenge = Read("Crypto", "bRandom") == "1",
                    crypto_dwChallengeLength = int.Parse(Read("Crypto", "length")),

                    //FileMgr
                    fileMgr_dwThread = int.Parse(Read("FileMgr", "thread")),
                    fileMgr_dwMaxFolder = int.Parse(Read("FileMgr", "max_folder")),
                    fileMgr_dwMaxFile = int.Parse(Read("FileMgr", "max_file")),
                    fileMgr_dwTimeout = int.Parse(Read("FileMgr", "timeout")),
                    fileMgr_dwUfChunkSize = int.Parse(Read("FileMgr", "uf_size")),
                    fileMgr_dwDfChunkSize = int.Parse(Read("FileMgr", "df_size")),

                    fileMgr_bMaxFolder = Read("FileMgr", "bMaxFolder") == "1",
                    fileMgr_bMaxFile = Read("FileMgr", "bMaxFile") == "1",

                    //TaskMgr
                    taskMgr_szAVjson = Read("TaskMgr", "AVjson"),
                    bGetChildProcess = Read("TaskMgr", "childproc") == "1",

                    //ServMgr
                    servMgr_szAVjson = Read("ServMgr", "AVjson"),

                    //RegEdit
                    reg_dwMaxKey = int.Parse(Read("RegEdit", "max_key")),
                    reg_dwMaxValue = int.Parse(Read("RegEdit", "max_value")),

                    reg_bMaxKey = Read("RegEdit", "bMaxKey") == "1",
                    reg_bMaxValue = Read("RegEdit", "bMaxValue") == "1",

                    //Shell
                    shell_szExec = Read("Shell", "exec"),
                    shell_bHistory = Read("Shell", "bHistory") == "1",
                    shell_szHistory = Read("Shell", "file_history"),

                    //Monitor
                    monitor_bShowDate = Read("Desktop", "record_showdatetime") == "1",
                    monitor_dwQuality = int.Parse(Read("Desktop", "quality")),

                    //Webcam
                    webcam_bShowDate = Read("Webcam", "record_showdatetime") == "1",
                    webcam_dwQuality = int.Parse(Read("Webcam", "quality")),

                    //FunStuff
                    msgbox_szCaption = Read("FunStuff", "msgbox_caption"),
                    msgbox_szText = Read("FunStuff", "msgbox_text"),

                    szLockScreenDir = Read("FunStuff", "dir_lockscreen"),
                    szWallpaperDir = Read("FunStuff", "wallpaper"),
                    szSpeakText = Read("FunStuff", "speak_text"),
                };

                return config;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetConfigFromINI()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new SettingConfig();
            }
        }
    }
}
