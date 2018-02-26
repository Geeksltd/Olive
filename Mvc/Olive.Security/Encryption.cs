using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Olive.Security
{
    public class Encryption
    {
        public class AssymetricKeyPair
        {
            public string EncryptionKey { get; set; }
            public string DecryptionKey { get; set; }

            /// <summary>
            /// Invoke this method once and save the generated result in a file.
            /// Then deploy the encryption key in your encryption app's config file (e.g. Auth\appSettings.json) 
            /// and publish the descryption key to your decryptor apps' config files.
            /// </summary>
            public static AssymetricKeyPair Generate()
            {
                throw new NotImplementedException();

                //var rsa = CreateRSACryptoServiceProvider();
                //return new KeyValuePair<string, string>(rsa.ToXmlString(includePrivateParameters: false), rsa.ToXmlString(includePrivateParameters: true));
            }
        }

        /// <summary>
        /// Encrypts the specified text with the specified public key.
        /// </summary>
        /// <param name="raw">The clear text value to encrypt</param>
        /// <param name="encryptKey">The public key XML for encryption.</param>
        /// <param name="padding">The default padding value is OaepSHA512.</param>
        public static byte[] EncryptAsymmetric(byte[] raw, string encryptKey)
        {
            if (raw == null) throw new ArgumentNullException(nameof(raw));
            if (encryptKey.IsEmpty()) throw new ArgumentNullException(nameof(encryptKey));

            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypts the specified text with the specified public/private key pair.
        /// </summary>
        public static byte[] DecryptAsymmetric(byte[] cipher, string decryptKey)
        {
            if (cipher == null) throw new ArgumentNullException(nameof(cipher));
            if (decryptKey.IsEmpty()) throw new ArgumentNullException(nameof(decryptKey));
            throw new NotImplementedException();
        }

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
