using System;
using Olive;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive
{
    /// <summary>
    /// To limit the concurrent async process with a specific number.
    /// </summary>
    public class Limiter
    {
        readonly System.Timers.Timer Timer;
        readonly ConcurrentDictionary<long, int> Cache = new ConcurrentDictionary<long, int>();

        public int Limit { get; }

        public Limiter(int limitTo)
        {
            Limit = limitTo;
            Timer = new System.Timers.Timer(500.Milliseconds().TotalMilliseconds);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
        }

        public async Task Add(int count)
        {
            if (count > Limit) throw new ArgumentException("Provided argument is larger that the limition.");

            var effectiveSince = LocalTime.UtcNow.AddSeconds(-1).Ticks;
            var effective = Cache.Where(i => i.Key >= effectiveSince).ToArray();

            var current = effective.Sum(i => i.Value);

            if (current + count > Limit)
            {
                var oldest = effective.First().Key;
                var delay = TimeSpan.FromTicks((oldest - effectiveSince).LimitMin(0) + 1);
                await Task.Delay(delay);
                await Add(count);
            }

            if (!Cache.TryAdd(LocalTime.UtcNow.Ticks, count))
                await Add(count);
        }

        void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var expiryPoint = LocalTime.UtcNow.AddSeconds(-1).Ticks;
            Cache.RemoveWhereKey(key => key <= expiryPoint);
        }
    }
}
