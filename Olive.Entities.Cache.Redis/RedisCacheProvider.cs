using StackExchange.Redis;
using System;
using System.Collections;
using System.Linq;

namespace Olive.Entities.Data
{
    public partial class RedisCacheProvider : ICacheProvider
    {
        string RedisConfig;
        ConnectionMultiplexer Redis;
        IServer Server;

        public RedisCacheProvider()
        {
            RedisConfig = Config.Get("Database:Cache:RedisConfig", defaultValue: "localhost:6379");
            Redis = ConnectionMultiplexer.Connect(RedisConfig + ",allowAdmin=true");
            Server = Redis.GetServer(RedisConfig);
        }

        StackExchange.Redis.IDatabase Db => Redis.GetDatabase();

        string GetKey(IEntity entity) => entity.GetType().FullName + "|" + entity.GetId();

        public void Add(IEntity entity) => Set(GetKey(entity), entity);

        static string ListPrefix(Type type) => type.FullName + ">List>";

        public void AddList(Type type, string key, IEnumerable list)
        {
            var data = list.Cast<IEntity>().ToArray();
            Set(ListPrefix(type) + key, data);
        }

        public void ClearAll() => Server.FlushDatabase();

        void RemoveWithPattern(string pattern)
        {
            var keys = Server.Keys(pattern: pattern);
            Db.KeyDelete(keys.ToArray());
        }

        public void RemoveList(Type type) => RemoveWithPattern(ListPrefix(type) + "*");

        public IEntity Get(Type entityType, string id)
            => (IEntity)Get(entityType.FullName + "|" + id);

        public IEnumerable GetList(Type type, string key)
            => (IEnumerable)Get(ListPrefix(type) + key);

        public void Remove(IEntity entity) => Db.KeyDelete(GetKey(entity));

        public void Remove(Type type, bool invalidateCachedReferences = false)
        {
            RemoveWithPattern(type.FullName + "|*");
        }

        #region Row version is not supported in Web Farm

        public bool IsUpdatedSince(IEntity instance, DateTime since) => false;

        void ICacheProvider.UpdateRowVersion(IEntity entity) { }

        #endregion
    }
}