using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Olive.Entities.Data
{
    partial class RedisCacheProvider
    {
        object Get(string key)
        {
            var result = Db.StringGet(key);
            if (result.IsNullOrEmpty) return null;

            using (var mem = new MemoryStream((byte[])result))
                return new BinaryFormatter().Deserialize(mem);
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
