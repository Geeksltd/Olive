using StackExchange.Redis;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Olive.Entities.Data
{
    public partial class RedisCache : Cache
    {
        string RedisConfig;
        ConnectionMultiplexer Redis;
        IServer Server;

        public RedisCache()
        {
            RedisConfig = Config.Get("Database:Cache:RedisConfig", defaultValue: "localhost:6379");
            Redis = ConnectionMultiplexer.Connect(RedisConfig + ",allowAdmin=true");
            Server = Redis.GetServer(RedisConfig);
        }

        StackExchange.Redis.IDatabase Db => Redis.GetDatabase();


        string GetKey(IEntity entity) => entity.GetType().FullName + "|" + entity.GetId();

        protected override void DoAdd(IEntity entity) => Set(GetKey(entity), entity);

        static string ListPrefix(Type type) => type.FullName + ">List>";

        protected override void DoAddList(Type type, string key, IEnumerable list)
        {
            var data = list.Cast<IEntity>().ToArray();
            Set(ListPrefix(type) + key, data);
        }

        public override void ClearAll() => Server.FlushDatabase();

        void RemoveWithPattern(string pattern)
        {
            var keys = Server.Keys(pattern: pattern);
            Db.KeyDelete(keys.ToArray());
        }

        protected override void DoExpireLists(Type type) => RemoveWithPattern(ListPrefix(type) + "*");

        protected override IEntity DoGet(Type entityType, string id)
            => (IEntity)Get(entityType.FullName + "|" + id);

        protected override IEnumerable DoGetList(Type type, string key)
            => (IEnumerable)Get(ListPrefix(type) + key);

        protected override void DoRemove(IEntity entity) => Db.KeyDelete(GetKey(entity));

        protected override void DoRemove(Type type, bool invalidateCachedReferences = false)
        {
            RemoveWithPattern(type.FullName + "|*");
        }

        #region Row version is not supported in Web Farm

        public override bool IsUpdatedSince(IEntity instance, DateTime since) => false;

        public override void UpdateRowVersion(IEntity entity) { }

        #endregion
    }
}
