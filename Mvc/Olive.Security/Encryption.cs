using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Olive.Security
{
    public class Encryption
    {
        public class AsymmetricKeyPair
        {
            public string EncryptionKey { get; set; }
            public string DecryptionKey { get; set; }

            /// <summary>
            /// Invoke this method once and save the generated result in a file.
            /// Then deploy the encryption key in your encryption app's config file (e.g. Auth\appSettings.json) 
            /// and publish the descryption key to your decryptor apps' config files.
            /// </summary>
            public static AsymmetricKeyPair Generate()
            {
                var cspParams = new CspParameters(1)
                {
                    KeyContainerName = "KeyContainer",
                    Flags = CspProviderFlags.UseMachineKeyStore,
                    ProviderName = "Microsoft Strong Cryptographic Provider"
                };

                var rsa = new RSACryptoServiceProvider(cspParams);

                return new AsymmetricKeyPair
                {
                    DecryptionKey = rsa.ToXmlString(true),
                    EncryptionKey = rsa.ToXmlString(false)
                };
            }
        }

        static RSACryptoServiceProvider CreateRSAProvider(string key)
        {
            var result = new RSACryptoServiceProvider(256);
            result.FromXmlString(key);
            return result;
        }

        /// <summary>
        /// Encrypts the specified text with the specified public key.
        /// </summary>
        /// <param name="raw">The raw data to encrypt</param>
        /// <param name="encryptKey">The public key for encryption.</param> 
        public static byte[] EncryptAsymmetric(byte[] raw, string encryptKey)
        {
            if (raw == null) throw new ArgumentNullException(nameof(raw));
            if (encryptKey.IsEmpty()) throw new ArgumentNullException(nameof(encryptKey));

            using (var provider = CreateRSAProvider(encryptKey))
                return provider.Decrypt(raw, fOAEP: false);
        }

        /// <summary>
        /// Decrypts the specified text with the specified public/private key pair.
        /// </summary>
        public static byte[] DecryptAsymmetric(byte[] cipher, string decryptKey)
        {
            if (cipher == null) throw new ArgumentNullException(nameof(cipher));
            if (decryptKey.IsEmpty()) throw new ArgumentNullException(nameof(decryptKey));

            using (var provider = CreateRSAProvider(decryptKey))
                return provider.Decrypt(cipher, fOAEP: false);
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