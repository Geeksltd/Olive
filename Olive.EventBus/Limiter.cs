using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
    public class Limiter
    {
        readonly ConcurrentDictionary<long, int> cache = new ConcurrentDictionary<long, int>();

        public int Limit { get; }

        public Limiter(int limitTo)
        {
            Limit = limitTo;
        }

        public async Task Add(int count)
        {
            if (count > Limit) throw new ArgumentException("Provided argument is larger that the limition.");

            var key = GetKey();

            if (cache.TryGetValue(key, out var value))
            {
                if (value + count > Limit)
                {
                    var delay = GetDelay(key);
                    await Task.Delay(delay);
                    if (!cache.TryAdd(key + 1, count))
                        await Add(count);
                }

                if (!cache.TryUpdate(key, value + count, value))
                    await Add(count);
            }
            else if (!cache.TryAdd(key, count))
                await Add(count);

            cache.RemoveWhereKey(x => x < key);
        }

        long GetKey() => (long)(LocalTime.UtcNow - DateTime.MinValue).TotalSeconds;

        TimeSpan GetDelay(long key) => DateTime.MinValue.AddSeconds(key + 1) - LocalTime.UtcNow;
    }
}
