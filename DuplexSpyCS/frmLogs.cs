using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmLogs : Form
    {
        private DateTime dateLastUpdate;
        private string szDateTime => dateLastUpdate.ToString("yyyy-MM-dd HH:mm:ss");
        private SqlConn sql_conn = C2.sql_conn;

        private bool bSearchWhenTextChanged = false;

        public frmLogs()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Display systme log.
        /// </summary>
        private void ShowSystemLogs()
        {
            string sql = $"SELECT * FROM Logs WHERE CreateDate >= \"{dateLastUpdate.ToString("yyyy-MM-dd HH:mm:ss")}\" AND Type == \"System\";";

            DataTable dt = C2.sql_conn.GetDataTable(sql);

            foreach (DataRow dr in dt.Rows)
            {
                Invoke(new Action(() =>
                {
                    richTextBox1.AppendText($"[{dr[6]}]: {dr[5]}");
                    richTextBox1.AppendText(Environment.NewLine);
                }));
            }
        }

        /// <summary>
        /// Display key exchange log.
        /// </summary>
        private void ShowKeyExchange()
        {
            //Sent RSA public key
            string sql = $"SELECT * FROM Logs WHERE CreateDate >= \"{dateLastUpdate.ToString("yyyy-MM-dd HH:mm:ss")}\" AND Type == \"KeyExchange\" AND Message ==\"OK\";";

            DataTable dt = C2.sql_conn.GetDataTable(sql);

            foreach (DataRow dr in dt.Rows)
            {
                string rhost = dr[2].ToString(); //Remote Host

                /* Warning: the OnlineID maybe be duplicated if same machine connect to this server over thousands times.
                 * Becasue in this stage the online id is determined by remote host. */
                DataTable tmp = C2.sql_conn.GetDataTable($"SELECT * FROM Logs WHERE OnlineID == \"{rhost}\" AND Type == \"KeyExchange\" AND Message ==\"Sent RSA public key\";");
                string szCreateDate = tmp.Rows[0][6].ToString();

                ListViewItem item = new ListViewItem(rhost);
                item.SubItems.Add(dr[5].ToString());
                item.SubItems.Add(szCreateDate);
                item.SubItems.Add(dr[6].ToString());

                Invoke(new Action(() => listView1.Items.Add(item)));
            }
        }

        /// <summary>
        /// Diaplay sent command.
        /// </summary>
        private void ShowFunction()
        {
            string sql = $"SELECT * FROM Logs WHERE CreateDate >= \"{dateLastUpdate.ToString("yyyy-MM-dd HH:mm:ss")}\" AND Type == \"Function\";";

            DataTable dt = C2.sql_conn.GetDataTable(sql);

            foreach (DataRow dr in dt.Rows)
            {
                Invoke(new Action(() =>
                {
                    richTextBox2.AppendText($"[{dr[6]}]: {dr[5]}");
                    richTextBox2.AppendText(Environment.NewLine);
                }));
            }
        }

        /// <summary>
        /// Display error log.
        /// </summary>
        private void ShowError()
        {
            string sql = $"SELECT * FROM Logs WHERE CreateDate >= \"{dateLastUpdate.ToString("yyyy-MM-dd HH:mm:ss")}\" AND Type == \"Error\";";

            DataTable dt = C2.sql_conn.GetDataTable(sql);

            foreach (DataRow dr in dt.Rows)
            {
                Invoke(new Action(() =>
                {
                    richTextBox3.AppendText($"[{dr[6]}]: {dr[5]}");
                    richTextBox3.AppendText(Environment.NewLine);
                }));
            }
        }

        /// <summary>
        /// Append new system log message into text box.
        /// </summary>
        /// <param name="msg"></param>
        private void NewSystemLogs(string msg)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.AppendText(msg);
                richTextBox1.AppendText(Environment.NewLine);
            }));
        }

        /// <summary>
        /// Append new key exchange message into list view.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="msg"></param>
        private void NewKeyExchange(Victim v, string msg)
        {
            string rhost = v.socket.RemoteEndPoint.ToString();
            Invoke(new Action(() =>
            {
                ListViewItem item = listView1.FindItemWithText(rhost);
                string szDate = C1.DateTimeStrEnglish();

                if (item == null)
                {
                    item = new ListViewItem(rhost);
                    item.SubItems.Add(msg);
                    item.SubItems.Add(szDate);
                    item.SubItems.Add(szDate);
                    listView1.Items.Add(item);
                }
                else
                {
                    item.SubItems[1].Text = msg;
                    item.SubItems[3].Text = szDate;
                }
            }));
        }

        /// <summary>
        /// Append new function log message into text box.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="msg"></param>
        private void NewFunction(Victim v, string msg)
        {
            Invoke(new Action(() =>
            {
                richTextBox2.AppendText($"[{v.ID}]: {msg}");
                richTextBox2.AppendText(Environment.NewLine);
            }));
        }

        /// <summary>
        /// Append new error log message into textbox.
        /// </summary>
        /// <param name="msg"></param>
        private void NewError(string msg)
        {
            Invoke(new Action(() =>
            {
                richTextBox3.AppendText(msg);
                richTextBox3.AppendText(Environment.NewLine);
            }));
        }

        void setup()
        {
            dateLastUpdate = C2.dtStartUp;

            ShowSystemLogs();
            ShowKeyExchange();
            ShowFunction();
            ShowError();

            sql_conn.NewSystemLogs += NewSystemLogs;
            sql_conn.NewKeyExchangeLogs += NewKeyExchange;
            sql_conn.NewSendFunctionLogs += NewFunction;
            sql_conn.NewErrorLogs += NewError;
        }

        private void frmLogs_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dateLastUpdate = DateTime.Now;
        }

        private void frmLogs_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        //File - Save
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File(*.txt)|*.txt|CSV File(*.csv)|*.csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                switch (sfd.FilterIndex)
                {
                    case 0: //txt
                        using (StreamWriter sw = new StreamWriter(sfd.FileName))
                        {
                            sw.WriteLine($"--------------------[ System ]--------------------");
                            sw.WriteLine(richTextBox1.Text);

                            sw.WriteLine($"--------------------[ Key Exchange ]--------------------");
                            sw.WriteLine($"{string.Join(",", listView1.Columns.Cast<ColumnHeader>().Select(x => x.Text).ToArray())}");
                            foreach (ListViewItem item in listView1.Items)
                                sw.WriteLine($"{string.Join(",", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(x => x.Text).ToArray())}");

                            sw.WriteLine($"--------------------[ Run Payload ]--------------------");
                            sw.WriteLine(richTextBox2.Text);

                            sw.WriteLine($"--------------------[ Error ]--------------------");
                            sw.WriteLine(richTextBox3.Text);
                        }

                        break;
                    case 1: //csv

                        break;
                }
            }
        }

        private void frmLogs_FormClosed(object sender, FormClosedEventArgs e)
        {
            sql_conn.NewSystemLogs -= NewSystemLogs;
            sql_conn.NewKeyExchangeLogs -= NewKeyExchange;
            sql_conn.NewSendFunctionLogs -= NewFunction;
            sql_conn.NewErrorLogs -= NewError;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\Log").Show();
        }

        #region System

        void SysLogSearch(string pattern)
        {
            richTextBox1.Clear();

            DataTable dt = C2.sql_conn.GetDataTable($"SELECT * FROM Logs WHERE Type = \"System\" AND CreateDate >= \"{szDateTime}\"");
            foreach (DataRow dr in dt.Rows)
            {
                string msg = (string)dr[5];
                string szDate = (string)dr[6];

                if (Regex.IsMatch(msg, pattern, RegexOptions.IgnoreCase))
                {
                    richTextBox1.AppendText($"[{szDate}]: {msg}\n");
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (bSearchWhenTextChanged)
                SysLogSearch(textBox1.Text);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SysLogSearch(textBox1.Text);
        }

        #endregion

        #region Key exchange

        void KxLogSearch(string pattern)
        {
            listView1.Items.Clear();

            DataTable dt = C2.sql_conn.GetDataTable($"SELECT * FROM Logs WHERE Type = \"KeyExchange\" AND CreateDate >= \"{szDateTime}\"");
            foreach (DataRow dr in dt.Rows)
            {
                string online_id = (string)dr[2];
                string msg = (string)dr[5];
                string szDate = (string)dr[6];

                if (Regex.IsMatch(msg, pattern, RegexOptions.IgnoreCase) || Regex.IsMatch(online_id, pattern, RegexOptions.IgnoreCase))
                {
                    ListViewItem item = new ListViewItem(online_id);
                    item.SubItems.Add(msg);
                    item.SubItems.Add(szDate);

                    listView1.Items.Add(item);
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (bSearchWhenTextChanged)
                KxLogSearch(textBox2.Text);
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                KxLogSearch(textBox2.Text);
        }

        #endregion

        #region Payload

        void rpLogSearch(string pattern)
        {
            richTextBox2.Clear();

            DataTable dt = C2.sql_conn.GetDataTable($"SELECT * FROM Logs WHERE Type = \"Function\" AND CreateDate >= \"{szDateTime}\"");
            foreach (DataRow dr in dt.Rows)
            {
                string online_id = (string)dr[2];
                string msg = (string)dr[5];
                string szDate = (string)dr[6];

                if (Regex.IsMatch(msg, pattern, RegexOptions.IgnoreCase))
                {
                    richTextBox2.AppendText($"[{online_id}]: {msg}\n");
                }
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (bSearchWhenTextChanged)
                rpLogSearch(textBox3.Text);
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                rpLogSearch(textBox3.Text);
        }

        #endregion

        #region Error

        void errLogSearch(string pattern)
        {
            richTextBox3.Clear();

            DataTable dt = C2.sql_conn.GetDataTable($"SELECT * FROM Logs WHERE Type = \"Error\" AND CreateDate >= \"{szDateTime}\"");
            foreach (DataRow dr in dt.Rows)
            {
                string msg = (string)dr[5];
                string szDate = (string)dr[6];

                if (Regex.IsMatch(msg, pattern, RegexOptions.IgnoreCase))
                {
                    richTextBox3.AppendText($"[{szDate}]: {msg}\n");
                }
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (bSearchWhenTextChanged)
                errLogSearch(textBox4.Text);
        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                errLogSearch(textBox4.Text);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            new frmSetting("General").ShowDialog();
        }

        #endregion

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure to delete all logs?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes)
                return;

            if (!sql_conn.ClearLogs())
            {
                MessageBox.Show("Delete logs error.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Delete logs successfully", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}