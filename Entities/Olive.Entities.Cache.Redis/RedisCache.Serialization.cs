using StackExchange.Redis;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Olive.Entities.Data
{
    partial class RedisCache
    {
        object Get(string key)
        {
            var result = Db.StringGet(key);
            if (result.IsNullOrEmpty) return null;

            using (var mem = new MemoryStream((byte[])result))
            {
                var deserialized = new BinaryFormatter().Deserialize(mem);
                return deserialized;
                //if (deserialized is Serialized ser) return ser.Extract();
                //if (deserialized is Serialized[] sers)
                //    return sers.Select(x => x.Extract()).ToArray();
                //throw new NotSupportedException();
            }
        }

        void Set(string key, object value)
        {
            using (var mem = new MemoryStream())
            {
                new BinaryFormatter().Serialize(mem, value);
                Db.StringSet(key, mem.ToArray());
            }
        }

    }
}
