using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmSetting : Form
    {
        private IniManager ini_manager = C2.ini_manager;

        public frmSetting(string szInitPageName = null)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(szInitPageName))
            {
                TabPage _page = null;
                foreach (TabPage page in tabControl1.TabPages)
                {
                    if (page.Name == szInitPageName)
                    {
                        _page = page;
                        break;
                    }
                }

                if (_page != null)
                    tabControl1.SelectedTab = _page;
                else
                    throw new Exception("Cannot find tab page: " + szInitPageName);
            }
        }

        string Read(string section, string key)
        {
            return ini_manager.Read(section, key);
        }

        bool Write(string section, string key, string value)
        {
            try
            {
                ini_manager.Write(section, key, value);

                string szCheck = ini_manager.Read(section, key);
                if (string.IsNullOrEmpty(szCheck) || szCheck != value)
                    throw new Exception($"Write: [{section}] {key} = {value} failed");

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        bool Write(string section, string key, int value) => Write(section, key, value.ToString());
        bool Write(string section, string key, bool value) => Write(section, key, value ? 1 : 0);

        bool ValidateSettingConfig(SettingConfig config)
        {
            try
            {
                //todo: validate member of config.
                if (!File.Exists(config.szDbFile))
                    throw new Exception("Cannot find database file: " + config.szDbFile);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        SettingConfig GetConfigFromSetting()
        {
            try
            {
                SettingConfig config = new SettingConfig()
                {
                    //General
                    dwListenPort = (int)numericUpDown1.Value,
                    szDbFile = textBox2.Text,

                    dwTimeout = (int)numericUpDown5.Value,

                    //Crypto
                    crypto_szChallengeText = textBox11.Text,
                    crypto_bRandomChallenge = checkBox5.Checked,
                    crypto_dwChallengeLength = (int)numericUpDown13.Value,

                    //FileMgr
                    fileMgr_dwThread = (int)numericUpDown2.Value,
                    fileMgr_dwMaxFolder = (int)numericUpDown3.Value,
                    fileMgr_dwMaxFile = (int)numericUpDown4.Value,
                    fileMgr_dwTimeout = (int)numericUpDown5.Value,
                    fileMgr_dwUfChunkSize = (int)numericUpDown6.Value,
                    fileMgr_dwDfChunkSize = (int)numericUpDown7.Value,

                    //TaskMgr

                    //ServMgr

                    //RegEdit
                    reg_dwMaxKey = (int)numericUpDown14.Value,
                    reg_dwMaxValue = (int)numericUpDown15.Value,

                    //Shell
                    shell_szExec = textBox7.Text,
                    shell_bHistory = checkBox3.Checked,
                    shell_szHistory = textBox8.Text,

                    //Monitor
                    monitor_bShowDate = checkBox2.Checked,
                    monitor_dwQuality = (int)numericUpDown12.Value,

                    //Webcam
                    webcam_bShowDate = checkBox1.Checked,
                    webcam_dwQuality = (int)numericUpDown11.Value,

                    //FunStuff
                    msgbox_szCaption = textBox1.Text,
                    msgbox_szText = textBox3.Text,

                    szLockScreenDir = textBox4.Text,
                    szWallpaperDir = textBox5.Text,
                    szSpeakText = textBox6.Text,
                };

                return config;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GetConfigFromSetting()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new SettingConfig();
            }
        }

        void setup()
        {
            //Check setting window form is call by ShowDialog() (Modal)
            if (!Modal)
                throw new Exception("frmSetting cannot be called by Show()");

            SettingConfig config = C1.GetConfigFromINI();

            try
            {
                if (C2.ini_manager == null)
                    ini_manager = new IniManager("config.ini");

                //General
                numericUpDown1.Value = config.dwListenPort;
                textBox2.Text = config.szDbFile;

                //Crypto
                textBox11.Text = config.crypto_szChallengeText;
                checkBox5.Checked = config.crypto_bRandomChallenge;
                numericUpDown13.Value = config.crypto_dwChallengeLength;

                //FileMgr
                numericUpDown2.Value = config.fileMgr_dwThread;
                numericUpDown3.Value = config.fileMgr_dwMaxFolder;
                numericUpDown4.Value = config.fileMgr_dwMaxFile;
                numericUpDown5.Value = config.fileMgr_dwTimeout;

                numericUpDown6.Value = config.fileMgr_dwUfChunkSize;
                numericUpDown7.Value = config.fileMgr_dwDfChunkSize;

                checkBox8.Checked = config.fileMgr_bMaxFolder;
                checkBox9.Checked = config.fileMgr_bMaxFile;

                //TaskMgr
                textBox9.Text = config.taskMgr_szAVjson;
                checkBox4.Checked = config.bGetChildProcess;

                //ServMgr
                textBox10.Text = config.servMgr_szAVjson;

                //RegEdit
                numericUpDown14.Value = config.reg_dwMaxKey;
                numericUpDown15.Value = config.reg_dwMaxValue;

                checkBox6.Checked = config.reg_bMaxKey;
                checkBox7.Checked = config.reg_bMaxValue;

                //Shell
                textBox7.Text = config.shell_szExec;
                checkBox3.Checked = config.shell_bHistory;
                textBox8.Text = config.shell_szHistory;

                //Monitor
                numericUpDown12.Value = config.monitor_dwQuality;
                checkBox2.Checked = config.monitor_bShowDate;

                //Webcam
                numericUpDown11.Value = config.webcam_dwQuality;
                checkBox1.Checked = config.webcam_bShowDate;

                //Plugin

                //Build

                //FunStuff
                textBox1.Text = config.msgbox_szCaption;
                textBox3.Text = config.msgbox_szText;

                textBox4.Text = config.szLockScreenDir;
                textBox5.Text = config.szWallpaperDir;

                textBox6.Text = config.szSpeakText;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void Save()
        {
            try
            {
                SettingConfig config = GetConfigFromSetting();
                if (!ValidateSettingConfig(config))
                    throw new Exception("Validation failed.");

                //General
                if (!Write("General", "listen_port", config.dwListenPort))
                    throw new Exception("Write listen_port error.");

                //todo: listen_limit

                if (!Write("General", "db_file", config.szDbFile))
                    throw new Exception("Write db_file error.");

                //todo: show_screen

                //Crypto
                if (!Write("Crypto", "challenge", config.crypto_szChallengeText))
                    throw new Exception("Write challenge error.");

                if (!Write("Crypto", "bRandom", config.crypto_bRandomChallenge))
                    throw new Exception("Write bRandom error.");

                if (!Write("Crypto", "length", config.crypto_dwChallengeLength))
                    throw new Exception("Write length error.");

                //FileMgr
                if (!Write("FileMgr", "max_folder", config.fileMgr_dwMaxFolder))
                    throw new Exception("Write max_folder error.");

                if (!Write("FileMgr", "max_file", config.fileMgr_dwMaxFile))
                    throw new Exception("Write max_file error.");

                if (!Write("FileMgr", "timeout", config.fileMgr_dwTimeout))
                    throw new Exception("Write timeout error.");

                if (!Write("FileMgr", "thread", config.fileMgr_dwThread))
                    throw new Exception("Write thread error.");

                if (!Write("FileMgr", "uf_size", config.fileMgr_dwUfChunkSize))
                    throw new Exception("Write uf_size error.");

                if (!Write("FileMgr", "df_size", config.fileMgr_dwDfChunkSize))
                    throw new Exception("Write df_size error.");

                //ServMgr

                //todo: regedit

                //FunStuff
                if (!Write("FunStuff", "msgbox_caption", config.msgbox_szCaption))
                    throw new Exception("Write msgbox_caption error.");

                if (!Write("FunStuff", "msgbox_text", config.msgbox_szText))
                    throw new Exception("Write msgbox_text error.");

                if (!Write("FunStuff", "dir_lockscreen", config.szLockScreenDir))
                    throw new Exception("Write dir_lockscreen error.");

                if (!Write("FunStuff", "dir_wallpaper", config.szWallpaperDir))
                    throw new Exception("Write dir_wallpaper error.");

                if (!Write("FunStuff", "speak_text", config.szSpeakText))
                    throw new Exception("Write speak_text error.");

                //TaskMgr

                //Monitor
                if (!Write("Desktop", "quality", config.monitor_dwQuality))
                    throw new Exception("Write quality error.");

                if (!Write("Desktop", "record_showdatetime", config.monitor_bShowDate))
                    throw new Exception("Write record_showdatetime error.");

                //Webcam
                if (!Write("Webcam", "quality", config.webcam_dwQuality))
                    throw new Exception("Write quality error.");

                if (!Write("Webcam", "record_showdatetime", config.webcam_bShowDate))
                    throw new Exception("Write record_showdatetime error.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Write INI error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmSetting_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Save();
        }

        //FunStuff - Image - Lock Screen
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = fbd.SelectedPath;
            }
        }
        //FunStuff - Image - Wallpaper
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                textBox5.Text = fbd.SelectedPath;
            }
        }

        //TaskMgr - Select anti-virus json file
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox9.Text = ofd.FileName;
            }
        }

        //Refresh
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            setup();
        }
    }
}
