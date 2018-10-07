// namespace Olive.Security
// {
//    using System;
//    using System.Security.Cryptography;
//    using System.Text;
//    using System.Xml.Serialization;

//    public class SignedData
//    {
//        internal static XmlSerializer Serializer = new XmlSerializer(typeof(SignedData));

//        public byte[] Signature { get; set; }

//        public byte[] EncryptedData { get; set; }

//        byte[] Hash
//        {
//            get
//            {
//                using (var sha = new SHA1Managed()) return sha.ComputeHash(EncryptedData);
//            }
//        }

//        static string SHA1 => CryptoConfig.MapNameToOID("SHA1");

//        public SignedData() { }

//        public SignedData(byte[] data, byte[] signature)
//        {
//            EncryptedData = data;
//            Signature = signature;
//        }

//        public SignedData(byte[] data, RSACryptoServiceProvider rsa)
//        {
//            EncryptedData = data;
//            Signature = rsa.SignHash(Hash, SHA1);
//        }

//        public byte[] ToBytes() => Serializer.Serialize(this).ToBytes(Encoding.UTF8);

//        internal void VerifySignature(RSACryptoServiceProvider rsa)
//        {
//            if (rsa.VerifyHash(Hash, SHA1, Signature)) return;
//            else throw new Exception("signature is not valid.");
//        }
//    }
// }