using Newtonsoft.Json;
using System;

namespace Olive.Aws
{
    public class SnsMessage<T> : SnsMessage
    {
        public T ParseMessage() => JsonConvert.DeserializeObject<T>(Message);
    }

    public class SnsMessage : EventBusMessage
    {
        public string Type { get; set; }
        public string MessageId { get; set; }
        public string TopicArn { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string SignatureVersion { get; set; }
        public string Signature { get; set; }
        public string SigningCertURL { get; set; }
        public string UnsubscribeURL { get; set; }
    }
}