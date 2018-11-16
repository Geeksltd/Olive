using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Olive.Security
{
    partial class Encryption
    {
        const int AES_KEY_LENGTH = 32, AES_IV_LENGTH = 16;

        static byte[] GetValidAesKey(string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);
            if (key.Length == AES_KEY_LENGTH) return key;
            return key = key.Take(AES_KEY_LENGTH).ToArray().PadRight(AES_KEY_LENGTH, (byte)0);
        }

        /// <summary>Encrypts the specified text with the specified password.</summary>
        /// <param name="encoding">If not specified, UTF8 will be used.</param>
        public static string Encrypt(string raw, string password, Encoding encoding = null)
        {
            if (raw.IsEmpty()) throw new ArgumentNullException(nameof(raw));
            if (encoding == null) encoding = Encoding.UTF8;

            return Encrypt(encoding.GetBytes(raw), password).ToBase64String();
        }

        public static byte[] Encrypt(byte[] raw, string password)
        {
            if (raw == null) throw new ArgumentNullException(nameof(raw));

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = GetValidAesKey(password);
                aesAlg.IV = aesAlg.Key.Take(AES_IV_LENGTH).ToArray();
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

        /// <summary>Decrypts the specified text with the specified password.</summary>
        /// <param name="encoding">If not specified, UTF8 will be used.</param>
        public static string Decrypt(string cipher, string password, Encoding encoding = null)
        {
            if (cipher.IsEmpty()) throw new ArgumentNullException(nameof(cipher));
            var plainBytes = Decrypt(cipher.ToBytesFromBase64(), password);

            if (encoding == null) encoding = Encoding.UTF8;
            return encoding.GetString(plainBytes);
        }

        public static byte[] Decrypt(byte[] cipher, string password)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = GetValidAesKey(password);
                aesAlg.IV = aesAlg.Key.Take(AES_IV_LENGTH).ToArray();

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(cipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new BinaryReader(csDecrypt))
                    return srDecrypt.ReadAllBytes();
            }
        }
    }
}