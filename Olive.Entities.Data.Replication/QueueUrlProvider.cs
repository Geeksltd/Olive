using Olive;
using System;
using System.Collections.Generic;
using System.Text;


internal class QueueUrlProvider
{
    internal static string GetUrl(Type type)
    {
        var url = Config.Get($"DataReplication:{type.FullName.Replace(".", "_")}:Url");

        if (url.IsEmpty())
            url = Config.Get($"DataReplication:{type.FullName}:Url");

        if (url.IsEmpty())
            url = $"FOR_DEVELOPMENT_ONLY_DataReplication_{type.FullName}";

        return url;
    }
}

