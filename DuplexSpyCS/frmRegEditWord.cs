using Microsoft.Win32;
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
    public partial class frmRegEditWord : Form
    {
        public frmManager f_mgr;

        public string regFullPath;
        public string valName;
        public int valData;

        public bool bDword = true;
        private bool bInit = true;

        public frmRegEditWord()
        {
            InitializeComponent();
        }

        void setup()
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            textBox1.Text = valName;
            textBox1.ReadOnly = true;

            textBox2.Text = valData.ToString();
        }

        private void frmRegEditWord_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                RegistryValueKind kind = comboBox1.SelectedIndex == 0 ? RegistryValueKind.DWord : RegistryValueKind.QWord;

                string strInt = textBox2.Text;
                strInt = strInt.Replace("0x", string.Empty);

                switch (comboBox2.SelectedIndex)
                {
                    case 0:
                        valData = int.Parse(strInt);
                        break;
                    case 1:
                        valData = int.Parse(strInt, System.Globalization.NumberStyles.HexNumber);
                        break;
                }

                new Thread(() => f_mgr.Reg_ReqEditValue(regFullPath, valName, kind, valData)).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "RegEditWord", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string szOriginal = textBox2.Text;

            try
            {
                switch (comboBox2.SelectedIndex)
                {
                    case 0: //decimal
                        if (!bInit)
                        {
                            int nHex = Convert.ToInt32(textBox2.Text, 16);
                            textBox2.Text = nHex.ToString();
                        }
                        else
                        {
                            bInit = false;
                        }
                        break;
                    case 1: //hexadecimal
                        int nDec = int.Parse(textBox2.Text);
                        textBox2.Text = nDec.ToString("X");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox2.Text = szOriginal;
            }
        }
    }
}
