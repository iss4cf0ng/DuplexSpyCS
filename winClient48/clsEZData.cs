using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winClient48
{
    internal class clsEZData
    {
        public static byte[] abGzipCompress(byte[] abRaw)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    gzip.Write(abRaw, 0, abRaw.Length);
                }

                return ms.ToArray();
            }
        }
        public static byte[] abGzipDecompress(byte[] abCompressed)
        {
            using (MemoryStream msIn = new MemoryStream(abCompressed))
            {
                using (GZipStream gzip = new GZipStream(msIn, CompressionMode.Decompress, true))
                {
                    using (MemoryStream msOut = new MemoryStream())
                    {
                        gzip.CopyTo(msOut);
                        return msOut.ToArray();
                    }
                }
            }
        }

        public static string fnGenerateRandomStr(int nLength = 10)
        {
            const string szPattern = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();
            for (int i = 0; i < nLength; i++)
                sb.Append(szPattern[rand.Next(0, szPattern.Length)]);

            return sb.ToString();
        }
    }
}
