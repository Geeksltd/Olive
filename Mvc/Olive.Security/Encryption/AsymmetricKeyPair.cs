using System;
using System.Security.Cryptography;
using System.Text;

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

            internal static string KeyToXml(string key) => key.ToBytes().GZip().ToString(Encoding.ASCII);

            internal static string XmlToKey(string xml) => xml.ToBytes(Encoding.ASCII).UnGZip().ToBase64String();

            /// <summary>
            /// Invoke this method once and save the generated result in a file.
            /// Then deploy the encryption key in your encryption app's config file (e.g. Auth\appSettings.json) 
            /// and publish the descryption key to your decryptor apps' config files.
            /// </summary>
            public static AsymmetricKeyPair Generate()
            {
                var rsaCSP = new RSACryptoServiceProvider();
                var privateParams = rsaCSP.ExportParameters(includePrivateParameters: true);
                var publicKeyXml = rsaCSP.ToXmlString(includePrivateParameters: false);
                var privateKeyXml = rsaCSP.ToXmlString(includePrivateParameters: true);

                return new AsymmetricKeyPair
                {
                    DecryptionKey = XmlToKey(publicKeyXml),
                    EncryptionKey = XmlToKey(privateKeyXml)
                };
            }
        }
    }
}