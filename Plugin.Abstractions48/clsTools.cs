using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Abstractions48
{
    public class clsTools
    {
        public static void fnPrintTable(DataTable dt)
        {
            if (dt == null || dt.Columns.Count == 0)
                return;

            int[] colWidths = new int[dt.Columns.Count];

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                colWidths[i] = dt.Columns[i].ColumnName.Length;

                foreach (DataRow row in dt.Rows)
                {
                    int len = row[i]?.ToString().Length ?? 0;
                    if (len > colWidths[i])
                        colWidths[i] = len;
                }
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sb.Append(dt.Columns[i].ColumnName.PadRight(colWidths[i] + 2));
            }
            sb.AppendLine();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sb.Append(new string('-', colWidths[i]) + "  ");
            }
            sb.AppendLine();

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string text = row[i]?.ToString() ?? "";
                    sb.Append(text.PadRight(colWidths[i] + 2));
                }
                sb.AppendLine();
            }

            Console.WriteLine(sb.ToString());
        }

        public static void fnLogOK(string szMsg) => Console.WriteLine($"[+] {szMsg}");
        public static void fnLogInfo(string szMsg) => Console.WriteLine($"[*] {szMsg}");
        public static void fnLogError(string szMsg) => Console.WriteLine($"[-] {szMsg}");
        public static void fnLogWarning(string szMsg) => Console.WriteLine($"[!] {szMsg}");
    }
}
