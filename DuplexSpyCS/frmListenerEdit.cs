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
    public partial class frmListenerEdit : Form
    {
        private frmListener m_frmListener { get; init; }
        private stListenerConfig m_config { get; set; }
        private clsSqlConn m_sqlConn { get; init; }

        private List<string> m_lsUA = new List<string>()
        {
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Ubuntu Chromium/37.0.2062.94 Chrome/37.0.2062.94 Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.85 Safari/537.36",
            "Mozilla/5.0 (iPad; CPU OS 8_4_1 like Mac OS X) AppleWebKit/600.1.4 (KHTML, like Gecko) Version/8.0 Mobile/12H321 Safari/600.1.4",
            "Mozilla/5.0 (Windows NT 6.1; rv:38.0) Gecko/20100101 Firefox/38.0",
            "Mozilla/5.0 (iPhone13,2; U; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/602.1.50 (KHTML, like Gecko) Version/10.0 Mobile/15E148 Safari/602.1",
            "Mozilla/5.0 (Linux; U; en-US) AppleWebKit/528.5+ (KHTML, like Gecko, Safari/528.5+) Version/4.0 Kindle/3.0 (screen 600x800; rotate)",

            "python-requests/X.Y.Z",
        };
        private List<string> m_lsStatus = new List<string>()
        {
            "200 OK",
            "202 Accepted",

            "301 Moved Permanently",
            "302 Found",

            "400 Bad Request",
            "401 Unauthorized",
            "403 Forbidden",
            "404 Not Found",
            "406 Not Acceptable",

            "500 Internal Server Error",
            "501 Not Implemented",
            "502 Bad Gateway",
            "503 Service Unavailable",
            "505 HTTP Version Not Supported",

            "999 No Hacking", //Web Knight.
        };
        private List<string> m_lsServer = new List<string>()
        {
            "Apache/2.4.41 (Unix)",
            "nginx/1.18.0",
            "nginx/1.21.1",
            "Microsoft-IIS/10.0",
            "LiteSpeed",
            "Caddy",
            "AmazonS3",
            "WWW Server/1.1",
            "Cherokee/1.2.1",
            "Apache-Coyote/1.1",
            "cloudflare",
            "openresty/1.19.3.1",
        };
        private List<string> m_lsContentType = new List<string>()
        {
            "text/html; charset=UTF-8",
            "application/json; charset=UTF-8",
            "application/xml; charset=UTF-8",
            "text/plain; charset=UTF-8",
            "text/css; charset=UTF-8",
            "application/javascript; charset=UTF-8",
            "image/png",
            "audio/mpeg",
            "video/mp4",
            "application/pdf",
            "application/zip",
        };
        private List<string> m_lsBody = new List<string>()
        {
            "<h1>Access deinal.</h1>",
            "<h1>Server internal error.</h1>",
            "Nothing here.",
            "<script>window.location.href = 'https://www.google.com'</script>",
            "<script>while(true){};</script>",
        };

        public frmListenerEdit(frmListener frmListener, stListenerConfig config, clsSqlConn sqlConn)
        {
            InitializeComponent();

            m_frmListener = frmListener;
            m_config = config;
            m_sqlConn = sqlConn;

            Text = "Listener Editor";
        }

        void fnSetup()
        {
            //Controls
            checkBox1.Checked = true;
            checkBox1.Checked = false;

            foreach (string s in Enum.GetNames(typeof(enListenerProtocol)))
                comboBox1.Items.Add(s);

            comboBox1.SelectedIndex = 0;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; //ReadOnly.

            //tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            //HTTP
            textBox7.Text = "www.google.com";
            textBox5.Text = "/";

            foreach (string s in Enum.GetNames(typeof(enHttpMethod)))
                comboBox4.Items.Add(s);

            comboBox4.SelectedIndex = 0;
            comboBox4.DropDownStyle = ComboBoxStyle.DropDownList;

            numericUpDown1.Value = 4444;

            foreach (string s in m_lsUA)
                comboBox6.Items.Add(s);

            comboBox6.SelectedIndex = 0;

            foreach (string s in m_lsStatus)
                comboBox2.Items.Add(s);

            comboBox2.SelectedIndex = 0;

            foreach (string s in m_lsServer)
                comboBox5.Items.Add(s);

            comboBox5.SelectedIndex = 0;

            foreach (string s in m_lsContentType)
                comboBox3.Items.Add(s);

            comboBox3.SelectedIndex = 0;

            foreach (string s in m_lsBody)
                comboBox7.Items.Add(s);

            comboBox7.SelectedIndex = 0;

            if (!string.IsNullOrEmpty(m_config.szName))
            {
                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    if (string.Equals(comboBox1.Items[i].ToString(), Enum.GetName(typeof(enListenerProtocol), m_config.enProtocol)))
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }

                //TCP
                textBox1.Text = m_config.szName;
                numericUpDown1.Value = m_config.nPort;
                textBox2.Text = m_config.szDescription;

                //TLS
                textBox3.Text = m_config.szCertPath;
                textBox4.Text = m_config.szCertPassword;

                //HTTP
                textBox7.Text = m_config.szHttpHost;
                comboBox4.SelectedIndex = (int)m_config.httpMethod;
                textBox5.Text = m_config.szHttpPath;
                comboBox6.Text = m_config.szHttpUA;

                comboBox2.Text = m_config.szStatus;
                comboBox5.Text = m_config.szServer;
                comboBox3.Text = m_config.szContentType;
                comboBox7.Text = m_config.szBody;
            }
        }

        private void frmListenerEdit_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            stListenerConfig config = new stListenerConfig()
            {
                szName = textBox1.Text,
                nPort = (int)numericUpDown1.Value,
                szDescription = textBox2.Text,
                dtCreationDate = DateTime.Now,
                enProtocol = (enListenerProtocol)Enum.Parse(typeof(enListenerProtocol), comboBox1.Text),

                szCertPath = textBox3.Text,
                szCertPassword = textBox4.Text,

                szHttpHost = textBox7.Text,
                httpMethod = (enHttpMethod)Enum.Parse(typeof(enHttpMethod), comboBox4.Text),
                szHttpPath = textBox5.Text,
                szHttpUA = comboBox6.Text,

                szStatus = comboBox2.Text,
                szContentType = comboBox3.Text,
                szServer = comboBox5.Text,
                szBody = comboBox7.Text,
            };

            if (!m_sqlConn.fnbSaveListener(config))
            {
                MessageBox.Show("Save listener failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                m_frmListener.fnLoadListener();
                MessageBox.Show("Save listener successfully", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = comboBox1.SelectedIndex;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            frmSSLCert f = new frmSSLCert();
            f.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PFX file (*.pfx)|*.pfx";
            ofd.InitialDirectory = Application.StartupPath;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = ofd.FileName;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Listener\\Edit").Show();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox4.UseSystemPasswordChar = !checkBox1.Checked;
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            
        }
    }
}
