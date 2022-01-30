using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    class Dynamo
    {
        public static IAmazonDynamoDB Client => Context.Current.GetService<IAmazonDynamoDB>();
        public static IDynamoDBContext Db => new DynamoDBContext(Client);

        public static DbIndex<T> Index<T>(string name) => new(Db, name);

        public static DbTable<T> Table<T>() => new(Db);

        public class DbIndex<T>
        {
            readonly IDynamoDBContext Db;
            readonly DynamoDBOperationConfig Config;

            public DbIndex(IDynamoDBContext db, string indexName) => (Db, Config) = (db, new DynamoDBOperationConfig { IndexName = indexName });

            public Task<T> FirstOrDefault(object hashKey) => All(hashKey).FirstOrDefault(x => true);

            public async Task<T[]> All(object hashKey)
            {
                return (await Db.QueryAsync<T>(hashKey, Config).GetRemainingAsync())?.ToArray() ?? new T[0];
            }
        }

        public class DbTable<T>
        {
            readonly IDynamoDBContext Db;

            public DbTable(IDynamoDBContext db) => Db = db;

            public Task<T> Get(object obj) => Db.LoadAsync<T>(obj);

            public Task<List<T>> All(params ScanCondition[] conditions) => Db.ScanAsync<T>(conditions).GetRemainingAsync();

            public Task<int> ForAll(ScanCondition condition, int degreeOfParallelism, Func<T, Task> action)
            {
                return ForAll(new[] { condition }.ExceptNull().ToArray(), degreeOfParallelism, action);
            }

            public async Task<int> ForAll(ScanCondition[] conditions, int degreeOfParallelism, Func<T, Task> action)
            {
                var pages = 0;
                var result = 0;
                var scan = Db.ScanAsync<T>(conditions.OrEmpty());

                while (true)
                {
                    var batch = await scan.GetNextSetAsync();
                    pages++;

                    await batch.AsParallel().ForEachAsync(degreeOfParallelism, action);
                    result += batch.Count;

                    if (scan.IsDone) return result;
                }
            }
        }
    }
}