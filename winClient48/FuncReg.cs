using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winClient48
{
    public class FuncReg
    {
        private RegistryHive StringToRegistryHive(string hiveString)
        {
            // Convert the hive string to an enumeration value
            switch (hiveString.ToUpper())
            {
                case "HKEY_CLASSES_ROOT":
                    return RegistryHive.ClassesRoot;
                case "HKEY_CURRENT_USER":
                    return RegistryHive.CurrentUser;
                case "HKEY_LOCAL_MACHINE":
                    return RegistryHive.LocalMachine;
                case "HKEY_USERS":
                    return RegistryHive.Users;
                case "HKEY_CURRENT_CONFIG":
                    return RegistryHive.CurrentConfig;
                default:
                    return 0;
            }
        }
        private string RegistryHiveToString(RegistryHive hive)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot:
                    return "HKEY_CLASSES_ROOT";
                case RegistryHive.CurrentUser:
                    return "HKEY_CURRENT_USER";
                case RegistryHive.LocalMachine:
                    return "HKEY_LOCAL_MACHINE";
                case RegistryHive.Users:
                    return "HKEY_USERS";
                case RegistryHive.CurrentConfig:
                    return "HKEY_CURRENT_CONFIG";
                default:
                    return null;
            }
        }
        private string GetValueTypeString(int type)
        {
            switch (type)
            {
                case 1:
                    return "REG_SZ";
                case 2:
                    return "REG_EXPAND_SZ";
                case 3:
                    return "REG_BINARY";
                case 4:
                    return "REG_DWORD";
                case 7:
                    return "REG_MULTI_SZ";
                default:
                    return "Unknown";
            }
        }
        private string GetRegKeyPathFromFullPath(string regFullPath)
        {
            string[] pathSplit = regFullPath.Split('\\');

            if (pathSplit.Length < 2)
                throw new Exception("Invalid Path");

            if (pathSplit.Length == 2)
                return ".";
            else
                return string.Join("\\", pathSplit.Skip(2).Take(pathSplit.Length - 2).ToArray());
        }
        private RegistryHive GetHiveFromFullPath(string regFullPath)
        {
            string[] pathSplit = regFullPath.Split('\\');
            if (pathSplit.Length < 2)
                throw new Exception("Invalid Path");

            return StringToRegistryHive(pathSplit[1]);
        }
        private bool RegKeyPathExist(string regFullPath)
        {
            RegistryHive hive = GetHiveFromFullPath(regFullPath);
            string regPath = GetRegKeyPathFromFullPath(regFullPath);

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
            {
                using (RegistryKey subKey = string.Equals(regPath, ".") ? baseKey : baseKey.OpenSubKey(regPath))
                {
                    return subKey == null;
                }
            }
        }
        private bool ValueExist(string regFullPath, string valName)
        {
            if (!RegKeyPathExist(regFullPath))
                return false;

            RegistryHive hive = GetHiveFromFullPath(regFullPath);
            string regPath = GetRegKeyPathFromFullPath(regFullPath);

            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
            {
                using (RegistryKey subKey = regPath == "." ? baseKey : baseKey.OpenSubKey(regPath))
                {
                    return subKey.GetValue(valName) == null;
                }
            }
        }

        private string ConvertByteArrayToString(byte[] buffer)
        {
            return string.Join(string.Empty, Array.ConvertAll(buffer, b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }
        private string ConvertIntTo0xStr(int value)
        {
            return value.ToString("X");
        }

        public string GetRootKeys()
        {
            List<string> keys = new List<string>();
            foreach (var root_key in Enum.GetValues(typeof(RegistryHive)))
            {
                try
                {
                    RegistryKey key = RegistryKey.OpenBaseKey((RegistryHive)root_key, RegistryView.Default);
                    if (key.GetSubKeyNames().Length > 0)
                        keys.Add(key.Name);
                }
                catch (Exception ex)
                {

                }
            }

            return string.Join(",", keys.ToArray());
        }
        public string GetItems(string str_hive, string path)
        {
            List<string> l_subkeys = new List<string>();
            List<string> l_values = new List<string>();

            RegistryHive hive = StringToRegistryHive(str_hive);
            RegistryKey base_key = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            RegistryKey key = base_key.OpenSubKey(path, true);

            if (key != null)
            {
                l_subkeys.AddRange(key.GetSubKeyNames());
                foreach (string value_name in key.GetValueNames())
                {
                    RegistryValueKind value_kind = key.GetValueKind(value_name);
                    string value = null;
                    switch (value_kind)
                    {
                        case RegistryValueKind.Binary:
                            value = ConvertByteArrayToString((byte[])key.GetValue(value_name));
                            break;
                        case RegistryValueKind.QWord:
                            value = ConvertIntTo0xStr((int)key.GetValue(value_name));
                            break;
                        case RegistryValueKind.DWord:
                            value = ConvertIntTo0xStr((int)key.GetValue(value_name));
                            break;
                        default:
                            value = key.GetValue(value_name).ToString();
                            break;
                    }
                    string type = GetValueTypeString((int)value_kind);
                    l_values.Add($"{value_name},{type},{Crypto.b64E2Str(value)}");
                }

            }

            return string.Join(",", l_subkeys.ToArray()) + "|" + string.Join(";", l_values);
        }
        public bool Goto(string str_hive, string path)
        {
            RegistryHive hive = StringToRegistryHive(str_hive);
            RegistryKey base_key = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            RegistryKey key = base_key.OpenSubKey(path, true);

            return key != null;
        }

        public (int, string) AddNewKey(string regFullPath, string keyName)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                string[] pathSplit = regFullPath.Split('\\');

                if (pathSplit.Length < 2)
                    throw new Exception("InvalidPath: " + regFullPath);

                RegistryHive hive = StringToRegistryHive(pathSplit[1]);
                string regPath = string.Join("\\", pathSplit.Skip(pathSplit.Length == 2 ? 1 : 2).ToArray());

                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                {
                    if (pathSplit.Length == 2) //Add new key at BaseKey
                    {
                        baseKey.CreateSubKey(keyName, true);
                    }
                    else
                    {
                        using (RegistryKey subKey = baseKey.OpenSubKey(regPath))
                        {
                            subKey.CreateSubKey(keyName, true);
                        }
                    }
                }

                msg = "OK";
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            msg = Crypto.b64E2Str(msg);

            return (code, msg);
        }
        public (int, string) RenameKey(string srcFullPath, string dstFullPath)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                CopyKey(srcFullPath, dstFullPath);
                DeleteKey(srcFullPath);

                msg = "OK";
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }
        public (int, string) CopyKey(string srcFullPath, string dstFullPath)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                void CopyRecurrence(string rSrcFullPath, string rDstFullPath)
                {
                    string[] srcPathSplit = rSrcFullPath.Split('\\');
                    string[] dstPathSplit = rDstFullPath.Split('\\');

                    string srcKeyPath = string.Join("\\", srcPathSplit.Skip(2).Take(srcPathSplit.Length - 2).ToArray());
                    string dstKeyPath = string.Join("\\", dstPathSplit.Skip(2).Take(dstPathSplit.Length - 2).ToArray());

                    using (RegistryKey srcBaseKey = RegistryKey.OpenBaseKey(StringToRegistryHive(srcPathSplit[1]), RegistryView.Default))
                    using (RegistryKey dstBaseKey = RegistryKey.OpenBaseKey(StringToRegistryHive(dstPathSplit[1]), RegistryView.Default))
                    using (RegistryKey srcKey = srcBaseKey.OpenSubKey(srcKeyPath, true))
                    using (RegistryKey dstKey = dstBaseKey.CreateSubKey(dstKeyPath, true))
                    {
                        if (srcKey == null)
                            throw new Exception("Source key in null.");

                        foreach (string szSrcSubKey in srcKey.GetSubKeyNames())
                        {
                            using (RegistryKey srcSubKey = srcKey.OpenSubKey(szSrcSubKey, true))
                            {
                                string relaPath = srcSubKey.Name.Replace(srcPathSplit[1] + "\\" + srcKeyPath, string.Empty);
                                string dstNewPath = rDstFullPath + relaPath;

                                CopyKey($"{srcPathSplit[0]}\\" + srcSubKey.Name, dstNewPath);
                            }
                        }

                        foreach (string szSrcValName in srcKey.GetValueNames())
                        {
                            object valData = srcKey.GetValue(szSrcValName);
                            RegistryValueKind valKind = srcKey.GetValueKind(szSrcValName);

                            dstKey.SetValue(szSrcValName, valData, valKind);
                        }
                    }
                }

                CopyRecurrence(srcFullPath, dstFullPath);

                msg = "OK";
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            msg = Crypto.b64E2Str(msg);

            return (code, msg);
        }
        public (int, string) CopyValue(string srcFullPath, string srcValName, string dstFullPath, string dstValName)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                RegistryHive srcHive = GetHiveFromFullPath(srcFullPath);
                RegistryHive dstHive = GetHiveFromFullPath(dstFullPath);
                string srcKeyPath = GetRegKeyPathFromFullPath(srcFullPath);
                string dstKeyPath = GetRegKeyPathFromFullPath(dstFullPath);

                using (RegistryKey srcBaseKey = RegistryKey.OpenBaseKey(srcHive, RegistryView.Default))
                using (RegistryKey dstBaseKey = RegistryKey.OpenBaseKey(dstHive, RegistryView.Default))
                using (RegistryKey srcSubKey = srcKeyPath == "." ? srcBaseKey : srcBaseKey.OpenSubKey(srcKeyPath, true))
                using (RegistryKey dstSubKey = dstKeyPath == "." ? dstBaseKey : dstBaseKey.OpenSubKey(dstKeyPath, true))
                {
                    object srcValData = srcSubKey.GetValue(srcValName);
                    RegistryValueKind srcValKind = srcSubKey.GetValueKind(srcValName);

                    dstSubKey.SetValue(dstValName, srcValData, srcValKind);
                }
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }
        public (int, string) AddNewValue(string regFullPath, string valName, RegistryValueKind valKind)
        {
            //No initial value.
            int code = 1;
            string msg = string.Empty;

            try
            {
                string[] pathSplit = regFullPath.Split('\\');
                if (pathSplit.Length < 2)
                    throw new Exception("InvalidPath: " + regFullPath);

                RegistryHive hive = StringToRegistryHive(pathSplit[1]);
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64))
                {
                    if (pathSplit.Length == 2)
                    {
                        baseKey.SetValue(valName, string.Empty, valKind);
                    }
                    else
                    {
                        string regPath = string.Join("\\", pathSplit.Skip(2).Take(pathSplit.Length - 2).ToArray());
                        using (RegistryKey subKey = baseKey.OpenSubKey(regPath, true))
                        {
                            switch (valKind)
                            {
                                case RegistryValueKind.String:
                                    subKey.SetValue(valName, string.Empty, valKind);
                                    break;
                                case RegistryValueKind.ExpandString:
                                    subKey.SetValue(valName, string.Empty, valKind);
                                    break;
                                case RegistryValueKind.MultiString:
                                    subKey.SetValue(valName, string.Empty, valKind);
                                    break;
                                case RegistryValueKind.Binary:
                                    subKey.SetValue(valName, Encoding.UTF8.GetBytes(string.Empty), valKind);
                                    break;
                                case RegistryValueKind.DWord:
                                    subKey.SetValue(valName, 0, valKind);
                                    break;
                                case RegistryValueKind.QWord:
                                    subKey.SetValue(valName, 0, valKind);
                                    break;
                            }
                        }
                    }
                }

                msg = "OK";
            }
            catch (Exception ex)
            {
                code = 0;
                msg = $"{ex.GetType().Name} - {ex.Message}";
            }

            msg = Crypto.b64E2Str(msg);

            return (code, msg);
        }
        public (int, string) EditValue(string regFullPath, string valName, object valData)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                string[] pathSplit = regFullPath.Split('\\');
                RegistryHive regHive = StringToRegistryHive(pathSplit[1]);
                string regPath = GetRegKeyPathFromFullPath(regFullPath);

                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(regHive, RegistryView.Default))
                {
                    using (RegistryKey subKey = regPath == "." ? baseKey : baseKey.OpenSubKey(regPath, true))
                    {
                        subKey.SetValue(valName, valData);
                    }
                }

                msg = "OK";
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            msg = Crypto.b64E2Str(msg);

            return (code, msg);
        }
        public (int, string) RenameValue(string regFullPath, string oldValName, string newValName)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                CopyValue(regFullPath, oldValName, regFullPath, newValName);
                DeleteValue(regFullPath, oldValName);

                msg = "OK";
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            msg = Crypto.b64E2Str(msg);

            return (code, msg);
        }
        public (int, string) DeleteKey(string regFullPath)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                string[] pathSplit = regFullPath.Split('\\');

                if (pathSplit.Length <= 2)
                    throw new Exception("InvalidPath: " + regFullPath);

                RegistryHive hive = StringToRegistryHive(pathSplit[1]);
                string regPath = string.Join("\\", pathSplit.Skip(2).Take(pathSplit.Length - 2).ToArray());

                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                {
                    baseKey.DeleteSubKeyTree(regPath);
                }

                msg = "OK";
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }
        public (int, string) DeleteValue(string regFullPath, string valName)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                string[] pathSplit = regFullPath.Split('\\');

                if (pathSplit.Length <= 2)
                    throw new Exception("Invalid Path: " + regFullPath);

                RegistryHive hive = StringToRegistryHive(pathSplit[1]);
                string regPath = string.Join("\\", pathSplit.Skip(2).Take(pathSplit.Length - 2).ToArray());

                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                {
                    using (RegistryKey subKey = baseKey.OpenSubKey(regPath, true))
                    {
                        subKey.DeleteValue(valName);
                    }
                }
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            return (code, msg);
        }

        public (int, string) Import(string regFile)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "regedit.exe",
                        Arguments = $"/s \"{regFile}\"",
                        UseShellExecute = true,
                        Verb = "runas",
                    }
                };

                proc.Start();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                    throw new Exception(proc.StandardError.ReadToEnd());

                msg = "OK";
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            msg = Crypto.b64E2Str(msg);

            return (code, msg);
        }
        public (int, string) Export(string regFullPath, string servPath)
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                string tempfile = Path.GetTempFileName();
                string outputFile = tempfile + ".reg";
                string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Templates), outputFile);

                string[] pathSplit = regFullPath.Split('\\');
                string regPath = string.Join("\\", pathSplit.Skip(1).Take(pathSplit.Length - 1).ToArray());

                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = $"export \"{regPath}\" \"{outputPath}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                };

                using (Process p = Process.Start(info))
                {
                    p.WaitForExit();
                    if (p.ExitCode != 0)
                        throw new Exception(p.StandardError.ReadToEnd());

                    msg = File.ReadAllText(outputPath);
                }
            }
            catch (Exception ex)
            {
                code = 0;
                msg = ex.Message;
            }

            msg = Crypto.b64E2Str(msg);

            return (code, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="patterns"></param>
        /// <param name="method"></param>
        /// <param name="bIgnoreCase"></param>
        /// <param name="itemType"></param>
        /// <param name="keyKind"></param>
        /// <returns>Item1: type(k/v, key or value), Item2: Name, Item3: Path</returns>
        public List<(string, string, string)> Find(string[] paths, string[] patterns, int method, bool bIgnoreCase, int itemType, RegistryValueKind keyKind = RegistryValueKind.None)
        {
            //method: 0: Name Only, 1: Full Path
            //itemType: 0: Key, 1: Value, 2: Bot

            string InputProcess(string keyName)
            {
                switch (method)
                {
                    case 0:
                        string[] s = keyName.Split('\\');
                        return s[s.Length - 1]; //Return last element in array.
                    case 1:
                        return keyName;
                    default:
                        return null;
                }
            }

            List<(string, string, string)> results = new List<(string, string, string)>();

            foreach (string path in paths)
            {
                string[] pathSplit = path.Split('\\');
                if (pathSplit.Length < 2)
                    continue;

                RegistryHive hive = StringToRegistryHive(pathSplit[1]);
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                {
                    using (RegistryKey subKey = pathSplit.Length == 2 ? baseKey : baseKey.OpenSubKey(string.Join("\\", pathSplit.Skip(2).Take(pathSplit.Length - 2)), true))
                    {
                        if (itemType == 0 || itemType == 2)
                        {
                            foreach (string subKeyName in subKey.GetSubKeyNames())
                            {
                                foreach (string pattern in patterns)
                                {
                                    if (Regex.IsMatch(InputProcess(subKeyName), pattern))
                                        results.Add(("k", subKeyName, subKeyName));
                                }

                                //Recurrence
                                results.AddRange(Find(new string[] { subKeyName }, patterns, method, bIgnoreCase, itemType, keyKind));
                            }
                        }

                        if (itemType == 1 || itemType == 2)
                        {
                            foreach (string valName in subKey.GetValueNames())
                            {
                                foreach (string pattern in patterns)
                                {
                                    if (Regex.IsMatch(InputProcess(valName), pattern))
                                        results.Add(("v", valName, subKey.Name));
                                }
                            }
                        }
                    }
                }
            }

            return results;
        }
    }
}
