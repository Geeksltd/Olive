using Olive.Entities;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Olive.Globalization
{
    [DataContract]
    internal class GoogleAutoDetectJsonResponseRootObject
    {
        [DataMember]
        public GoogleAutoDetectJsonResponseData data { get; set; }
    }

    [DataContract]
    internal class GoogleAutoDetectJsonResponseData
    {
        [DataMember]
        public GoogleAutoDetectJsonResponseDetection[][] detections { get; set; }
    }

    [DataContract]
    internal class GoogleAutoDetectJsonResponseDetection
    {
        [DataMember]
        public string language { get; set; }
        [DataMember]
        public bool isReliable { get; set; }
        [DataMember]
        public float confidence { get; set; }
    }

    /// <summary>
    /// Response returned by Google API for each auto-detect language request
    /// </summary>
    public class GoogleAutodetectResponse
    {
        static IDatabase Database => Context.Current.Database();

        /// <summary>ISO Code</summary>
        public string ISOCode { get; private set; }
        /// <summary>Confidence [0;1] about the detection</summary>
        public double? Confidence { get; private set; }
        // public bool IsReliable { get; set; }    // Deprecated

        /// <summary>
        /// Initialize a new Google auto-detect response
        /// </summary>
        public GoogleAutodetectResponse(string isoCode, double? confidence)
        {
            ISOCode = isoCode;
            Confidence = confidence;
        }

        /// <summary>Language detected based on iso639-1</summary>
        public async Task<ILanguage> GetLanguage()
        {
            var iso6391Code = ISOCode.Substring(0, 2).ToLowerInvariant(); // ISO639-1 are two letters code, but for Chinese Google returns 2 different codes (zh-CN for simplified and zh-TW for traditional)
            return await Database.FirstOrDefault<ILanguage>(l => l.IsoCode.ToLowerInvariant() == iso6391Code);
        }
    }
}
