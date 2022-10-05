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

        static byte[] GetValidAesKey(string keyString, int aesLeyLength)
        {
            var key = Encoding.UTF8.GetBytes(keyString);
            if (key.Length == aesLeyLength) return key;
            return key = key.Take(aesLeyLength).ToArray().PadRight(aesLeyLength, (byte)0);
        }

        /// <summary>Encrypts the specified text with the specified password.</summary>
        /// <param name="encoding">If not specified, UTF8 will be used.</param>
        public static string Encrypt(string raw, string password, Encoding encoding = null) => Encrypt(raw, password, AES_KEY_LENGTH, AES_IV_LENGTH, encoding);

        /// <summary>Encrypts the specified text with the specified password.</summary>
        /// <param name="encoding">If not specified, UTF8 will be used.</param>
        public static string Encrypt(string raw, string password, int aesKeyLength, int aesIvLength, Encoding encoding = null)
        {
            if (raw.IsEmpty()) throw new ArgumentNullException(nameof(raw));
            if (encoding == null) encoding = Encoding.UTF8;

            return Encrypt(encoding.GetBytes(raw), password, aesKeyLength, aesIvLength).ToBase64String();
        }

        public static byte[] Encrypt(byte[] raw, string password) => Encrypt(raw, password, AES_KEY_LENGTH, AES_IV_LENGTH);

        public static byte[] Encrypt(byte[] raw, string password, int aesKeyLength, int aesIvLength)
        {
            if (raw == null) throw new ArgumentNullException(nameof(raw));

            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = GetValidAesKey(password, aesKeyLength);
                aesAlg.IV = aesAlg.Key.Take(aesIvLength).ToArray();
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
        public static string Decrypt(string cipher, string password, Encoding encoding = null) => Decrypt(cipher, password, AES_KEY_LENGTH, AES_IV_LENGTH, encoding);

        /// <summary>Decrypts the specified text with the specified password.</summary>
        /// <param name="encoding">If not specified, UTF8 will be used.</param>
        public static string Decrypt(string cipher, string password, int aesKeyLength, int aesIvLength, Encoding encoding = null)
        {
            if (cipher.IsEmpty()) throw new ArgumentNullException(nameof(cipher));
            var plainBytes = Decrypt(cipher.ToBytesFromBase64(), password, aesKeyLength, aesIvLength);

            if (encoding == null) encoding = Encoding.UTF8;
            return encoding.GetString(plainBytes);
        }

        public static byte[] Decrypt(byte[] cipher, string password) => Decrypt(cipher, password, AES_KEY_LENGTH, AES_IV_LENGTH);

        public static byte[] Decrypt(byte[] cipher, string password, int aesKeyLength, int aesIvLength)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = GetValidAesKey(password, aesKeyLength);
                aesAlg.IV = aesAlg.Key.Take(aesIvLength).ToArray();

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(cipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new BinaryReader(csDecrypt))
                    return srDecrypt.ReadAllBytes();
            }
        }
    }
}
