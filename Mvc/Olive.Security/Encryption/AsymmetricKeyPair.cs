using System.Security.Cryptography;

namespace Olive.Security
{
    partial class Encryption
    {
        public class AsymmetricKeyPair
        {
            /// <summary>Contains the base64 string for the public key Xml.</summary>
            public string EncryptionKey { get; set; }

            /// <summary>Contains the base64 string for the private key Xml.</summary>
            public string DecryptionKey { get; set; }

            AsymmetricKeyPair() { }

            /// <summary>
            /// Invoke this method once and save the generated result in a file.
            /// Then deploy the encryption key in your encryption app's config file (e.g. Auth\appSettings.json) 
            /// and publish the descryption key to your decryptor apps' config files.
            /// </summary>
            public static AsymmetricKeyPair Generate()
            {
                var rsa = new RSACryptoServiceProvider();

                var publicParams = rsa.ExportParameters(includePrivateParameters: false);
                var privateParams = rsa.ExportParameters(includePrivateParameters: true);

                return new AsymmetricKeyPair
                {
                    DecryptionKey = privateParams.ToKey(),
                    EncryptionKey = publicParams.ToKey()
                };
            }
        }
    }
}