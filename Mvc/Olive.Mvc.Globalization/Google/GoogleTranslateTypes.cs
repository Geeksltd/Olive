using System.Runtime.Serialization;

namespace Olive.Globalization
{
    [DataContract]
    internal class GoogleTranslateJsonResponseRootObject
    {
        [DataMember]
        public GoogleTranslateJsonResponseData data { get; set; }
    }

    [DataContract]
    internal class GoogleTranslateJsonResponseData
    {
        [DataMember]
        public GoogleTranslateJsonResponseTranslation[] translations { get; set; }
    }

    [DataContract]
    internal class GoogleTranslateJsonResponseTranslation
    {
        [DataMember]
        public string translatedText { get; set; }
    }
}
