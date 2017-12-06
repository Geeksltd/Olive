namespace Olive
{
    partial class OliveExtensions
    {
        public static void FromXmlString(this RSA rsa, string xmlString)
        {
            var parameters = new RSAParameters();

            var xmlReader = xmlString.ToXmlReader();

            Func<byte[]> getBytes = () => xmlReader.ReadContentAsString().ToBytes();

            if (xmlReader.Name.Equals("RSAKeyValue"))
            {
                while (xmlReader.Read())
                {
                    switch (xmlReader.Name)
                    {
                        case "Modulus":
                            parameters.Modulus = getBytes();
                            break;

                        case "Exponent":
                            parameters.Exponent = getBytes();
                            break;

                        case "P":
                            parameters.P = getBytes();
                            break;

                        case "Q":
                            parameters.Q = getBytes();
                            break;

                        case "DP":
                            parameters.DP = getBytes();
                            break;

                        case "DQ":
                            parameters.DQ = getBytes();
                            break;

                        case "InverseQ":
                            parameters.InverseQ = getBytes();
                            break;

                        case "D":
                            parameters.D = getBytes();
                            break;

                        default:
                            throw new ArgumentException("Invalid XML RSA key.");
                    }
                }
            }
            else
            {
                throw new ArgumentException("Invalid XML RSA key.");
            }

            rsa.ImportParameters(parameters);
        }

        public static string ToXmlString(this RSA rsa, bool includePrivateParameters)
        {
            var parameters = rsa.ExportParameters(includePrivateParameters);

            return
                "<RSAKeyValue>" +
                    $"<Modulus>{parameters.Modulus.ToBase64String()}</Modulus>" +
                    $"<Exponent>{parameters.Exponent.ToBase64String()}</Exponent>" +
                    $"<P>{parameters.P.ToBase64String()}</P>" +
                    $"<Q>{parameters.Q.ToBase64String()}</Q>" +
                    $"<DP>{parameters.DP.ToBase64String()}</DP>" +
                    $"<DQ>{parameters.DQ.ToBase64String()}</DQ>" +
                    $"<InverseQ>{parameters.InverseQ.ToBase64String()}</InverseQ>" +
                    $"<D>{parameters.D.ToBase64String()}</D>" +
                "</RSAKeyValue>";
        }
    }
}
