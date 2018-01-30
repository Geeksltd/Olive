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
        const int MAX_ENCRYPTION_PART_LENGTH = 117, SIXTEEN = 16, THIRTY_TWO = 32;

        /// <summary>
        /// Generates a public/private key for asymmetric encryption.
        /// </summary>
        public static KeyValuePair<string, string> GenerateAsymmetricKeys()
        {
            var rsa = CreateRSACryptoServiceProvider();

            return new KeyValuePair<string, string>(rsa.ToXmlString(includePrivateParameters: false), rsa.ToXmlString(includePrivateParameters: true));
        }

        static RSA CreateRSACryptoServiceProvider() => RSA.Create();

        /// <summary>
        /// Encrypts the specified text with the specified public key.
        /// </summary>
        /// <param name="text">The clear text value to encrypt</param>
        /// <param name="publicKeyXml">The public key XML for encryption.</param>
        /// <param name="padding">The default padding value is OaepSHA512.</param>
        public static string EncryptAsymmetric(string text, string publicKeyXml, RSAEncryptionPadding padding = null)
        {
            if (text.IsEmpty()) throw new ArgumentNullException(nameof(text));

            if (publicKeyXml.IsEmpty()) throw new ArgumentNullException(nameof(publicKeyXml));

            if (text.Length > MAX_ENCRYPTION_PART_LENGTH)
            {
                return text.Split(MAX_ENCRYPTION_PART_LENGTH).Select(p => EncryptAsymmetric(p, publicKeyXml)).ToString("|");
            }
            else
            {
                var rsa = CreateRSACryptoServiceProvider();
                rsa.FromXmlString(publicKeyXml);
                return rsa.Encrypt(text.ToBytes(Encoding.UTF8), padding ?? RSAEncryptionPadding.OaepSHA512).ToBase64String();
            }
        }

        /// <summary>
        /// Decrypts the specified text with the specified public/private key pair.
        /// </summary>
        /// <param name="encryptedText">The encrypted text to decode.</param>
        /// <param name="publicPrivateKeyXml">The private key used to decrype it.</param>
        /// <param name="padding">The default padding value is OaepSHA512.</param>
        public static string DecryptAsymmetric(string encryptedText
            , string publicPrivateKeyXml, RSAEncryptionPadding padding = null)
        {
            if (encryptedText.IsEmpty()) throw new ArgumentNullException(nameof(encryptedText));

            if (publicPrivateKeyXml.IsEmpty()) throw new ArgumentNullException(nameof(publicPrivateKeyXml));

            if (encryptedText.Contains("|"))
            {
                return encryptedText.Split('|').Select(p => DecryptAsymmetric(p, publicPrivateKeyXml)).ToString(string.Empty);
            }
            else
            {
                var rsa = CreateRSACryptoServiceProvider();
                rsa.FromXmlString(publicPrivateKeyXml);
                return rsa.Decrypt(encryptedText.ToBytes(), padding ?? RSAEncryptionPadding.OaepSHA512).ToString(Encoding.UTF8);
            }
        }

        static byte[] GetUsableKey(string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            if (key.Length == 32) return key;

            return key = key.Take(32).ToArray().PadRight(32, (byte)0);
        }

        public static byte[] Encrypt(byte[] input, string password)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = GetUsableKey(password);
                aesAlg.IV = aesAlg.Key.Take(16).ToArray();

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new BinaryWriter(csEncrypt))
                        swEncrypt.Write(input);

                    return msEncrypt.ToArray();
                }
            }
        }

        public static byte[] Decrypt(byte[] cipher, string password)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = GetUsableKey(password);
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
