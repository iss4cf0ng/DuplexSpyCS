using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace winClient48Small
{
    public class Crypto
    {
        static int rsa_keySize = 4096;
        static int aes_keySize = 256;
        static int block_size = 128;

        //RSA CRYPTO METHOD
        public static string[] CreateRSAKey()
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = rsa_keySize;

            string publicKey = rsa.ToXmlString(false);
            string privateKey = rsa.ToXmlString(true);

            return new string[] { publicKey, privateKey };
        }
        public static byte[] RSAEncrypt(string data, string publicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = rsa_keySize;
            rsa.FromXmlString(publicKey);

            byte[] eVal = rsa.Encrypt(Encoding.UTF8.GetBytes(data), false);

            return eVal;
        }
        public static byte[] RSADecrypt(byte[] data, string privateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = rsa_keySize;
            rsa.FromXmlString(privateKey);

            byte[] dVal = rsa.Decrypt(data, false);
            return dVal;
        }

        //AES CRYPTO METHOD
        public static string AESEncrypt(string plain_text, byte[] key, byte[] iv)
        {
            byte[] cipher_bytes = null;
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = aes_keySize;
                aes.BlockSize = block_size;
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plain_text);
                        }
                        cipher_bytes = ms.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(cipher_bytes);
        }
        public static string AESDecrypt(byte[] cipher_bytes, byte[] key, byte[] iv)
        {
            string plain_text = null;
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = aes_keySize;
                aes.BlockSize = block_size;
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(cipher_bytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            plain_text = sr.ReadToEnd();
                        }
                    }
                }
            }


            return plain_text;
        }
        public static (string key, string iv) AES_GenerateKeyAndIV()
        {
            string key = null;
            string iv = null;
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();

                key = b64E2Str(aes.Key);
                iv = b64E2Str(aes.IV);
            }

            return (key, iv);
        }

        //BASE-64 FUNCTION
        public static string b64E2Str(string data) { return Convert.ToBase64String(Encoding.UTF8.GetBytes(data)); }
        public static string b64E2Str(byte[] data) { return Convert.ToBase64String(data); }
        public static byte[] b64E2Bytes(string data) { return Encoding.UTF8.GetBytes(b64E2Str(data)); }
        public static byte[] b64E2Bytes(byte[] data) { return Encoding.UTF8.GetBytes(b64E2Str(data)); }
        public static string b64D2Str(string data) { return Encoding.UTF8.GetString(Convert.FromBase64String(data)); }
        public static string b64D2Str(byte[] data) { return Encoding.UTF8.GetString(data); }
        public static byte[] b64D2Bytes(string data) { return Convert.FromBase64String(data); }
        public static byte[] b64D2Bytes(byte[] data) { return b64D2Bytes(Encoding.UTF8.GetString(data)); }
    }
}
