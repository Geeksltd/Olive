namespace Olive.Security
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml.Serialization;

    class SignedData
    {
        internal static XmlSerializer Serializer = new XmlSerializer(typeof(SignedData));

        public byte[] Signature { get; set; }

        public byte[] EncryptedData { get; set; }

        byte[] Hash
        {
            get
            {
                using (var sha = new SHA1Managed()) return sha.ComputeHash(EncryptedData);
            }
        }

        static string SHA1 => CryptoConfig.MapNameToOID("SHA1");

        public SignedData() { }

        public SignedData(byte[] data, byte[] signature)
        {
            EncryptedData = data;
            Signature = signature;
        }

        public SignedData(byte[] data, RSACryptoServiceProvider rsa)
        {
            EncryptedData = data;
            Signature = rsa.SignHash(Hash, SHA1);
        }

        public static SignedData FromBytes(byte[] cipher)
        {
            var base64Data = cipher.ToString(Encoding.ASCII);

            using (var reader = new StringReader(base64Data))
                return (SignedData)Serializer.Deserialize(reader);
        }

        public byte[] ToBytes()
        {
            using (var textWriter = new StringWriter())
            {
                Serializer.Serialize(textWriter, this);
                return textWriter.ToString().ToBytes(Encoding.UTF8);
            }
        }

        internal void VerifySignature(RSACryptoServiceProvider rsa)
        {
            if (rsa.VerifyHash(Hash, SHA1, Signature)) return;
            else throw new Exception("signature is not valid.");
        }
    }
}
