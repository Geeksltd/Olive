using System;

namespace Olive.Entities.Data.Replication.QueueUrlProvider
{
    public class UrlProvider
    {
        public static string GetUrl(Type type)
        {
            var url = Config.Get($"DataReplication:{type.FullName.Replace(".", "_")}:Url");

            if (url.IsEmpty())
                url = Config.Get($"DataReplication:{type.FullName}:Url");

            if (url.IsEmpty())
                url = $"FOR_DEVELOPMENT_ONLY_DataReplication_{type.FullName}";

            return url;
        }
    }
}
