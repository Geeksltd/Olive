using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Olive
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
		/// <param name="padding">The default padding value is OaepSHA512.</param>
        public static string DecryptAsymmetric(string encodedText, string publicPrivateKeyXml, RSAEncryptionPadding padding = null)
        {
            if (encodedText.IsEmpty()) throw new ArgumentNullException(nameof(encodedText));

            if (publicPrivateKeyXml.IsEmpty()) throw new ArgumentNullException(nameof(publicPrivateKeyXml));

            if (encodedText.Contains("|"))
            {
                return encodedText.Split('|').Select(p => DecryptAsymmetric(p, publicPrivateKeyXml)).ToString(string.Empty);
            }
            else
            {
                var rsa = CreateRSACryptoServiceProvider();
                rsa.FromXmlString(publicPrivateKeyXml);
                return rsa.Decrypt(encodedText.ToBytes(), padding ?? RSAEncryptionPadding.OaepSHA512).ToString(Encoding.UTF8);
            }
        }

        /// <summary>
        /// Encrypts the specified text with the specified password.
        /// </summary>
        public static string Encrypt(string text, string password)
        {
            if (text.IsEmpty())
                throw new ArgumentNullException(nameof(text));

            if (password.IsEmpty())
                throw new ArgumentNullException(nameof(password));

            using (var key = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(password.Length.ToString())))
            {
                var aes = Aes.Create();
                aes.Padding = PaddingMode.PKCS7;
                var encryptor = aes.CreateEncryptor(key.GetBytes(THIRTY_TWO), key.GetBytes(SIXTEEN));

                var textData = Encoding.Unicode.GetBytes(text);
                using (var encrypted = new MemoryStream())
                using (var cryptoStream = new CryptoStream(encrypted, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(textData, 0, textData.Length);
                    cryptoStream.FlushFinalBlock();

                    return Convert.ToBase64String(encrypted.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypts the specified encrypted text with the specified password.
        /// </summary>
        public static string Decrypt(string encryptedText, string password)
        {
            using (var key = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(password.Length.ToString())))
            {
                var encryptedData = encryptedText.ToBytes();

                using (var aes = Aes.Create())
                {
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor(key.GetBytes(THIRTY_TWO), key.GetBytes(SIXTEEN)))
                    using (var memoryStream = new MemoryStream(encryptedData))
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        // The size of decrypted data is unknown, so we allocate a buffer big enough to store encrypted data.
                        // decrypted data is always the same or smaller than encrypted data.
                        var plainText = new byte[encryptedData.Length];
                        var decryptedSize = cryptoStream.Read(plainText, 0, plainText.Length);
                        return Encoding.Unicode.GetString(plainText, 0, decryptedSize);
                    }
                }
            }
        }
    }
}