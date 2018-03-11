using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Olive.Security
{
    partial class Encryption
    {
        static byte[] GetValidAesKey(string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);
            if (key.Length == 32) return key;
            return key = key.Take(32).ToArray().PadRight(32, (byte)0);
        }

        public static byte[] Encrypt(byte[] raw, string password)
        {
            if (raw == null) throw new ArgumentNullException(nameof(raw));

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = GetValidAesKey(password);
                aesAlg.IV = aesAlg.Key.Take(16).ToArray();
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new BinaryWriter(csEncrypt))
                        swEncrypt.Write(raw);

                    return msEncrypt.ToArray();
                }
            }
        }

        public static byte[] Decrypt(byte[] cipher, string password)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = GetValidAesKey(password);
                aesAlg.IV = aesAlg.Key.Take(16).ToArray();

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(cipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new BinaryReader(csDecrypt))
                    return srDecrypt.ReadAllBytes();
            }
        }
    }
}