using System.Collections.Generic;
using System.Linq;

namespace Olive.Entities.Data
{
    public class SubqueryMapping
    {
        public string Path, Subquery;
        public Dictionary<string, string> Details;

        public SubqueryMapping(string path, string prefix, Dictionary<string, string> destinationPropertyMappings)
        {
            Path = path;
            Details = destinationPropertyMappings.ToDictionary(x => x.Key, x =>
                {
                    if (x.Value.StartsWith("["))
                        return x.Value.Insert(1, prefix);
                    else return prefix + x.Value;
                });
        }
    }
}