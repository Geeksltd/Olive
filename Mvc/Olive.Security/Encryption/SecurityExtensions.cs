namespace Olive.Security
{
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml.Serialization;

    public static class SecurityExtensions
    {
        static XmlSerializer RSAParametersSerializer = new XmlSerializer(typeof(RSAParameters));

        public static string ToKey(this RSAParameters parameters)
        {
            var xml = RSAParametersSerializer.Serialize(parameters);
            return xml.ToBytes(Encoding.ASCII).GZip().ToBase64String();
        }

        public static RSACryptoServiceProvider FromKey(this RSACryptoServiceProvider provider, string key)
        {
            var xml = key.ToBytesFromBase64().UnGZip().ToString(Encoding.ASCII);
            var parameters = RSAParametersSerializer.Deserialize<RSAParameters>(xml);
            provider.ImportParameters(parameters);

            return provider;
        }
    }
}