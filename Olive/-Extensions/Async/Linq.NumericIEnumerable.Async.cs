using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Olive
{
    partial class OliveExtensions
    {
        public static async Task<int> Sum<T>(this IEnumerable<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Sum<T>(this IEnumerable<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this IEnumerable<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this IEnumerable<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Max<T>(this IEnumerable<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Max<T>(this IEnumerable<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Min<T>(this IEnumerable<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Min<T>(this IEnumerable<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Sum<T>(this IEnumerable<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Sum<T>(this IEnumerable<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this IEnumerable<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this IEnumerable<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Max<T>(this IEnumerable<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Max<T>(this IEnumerable<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Min<T>(this IEnumerable<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Min<T>(this IEnumerable<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Sum<T>(this IEnumerable<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Sum<T>(this IEnumerable<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Average<T>(this IEnumerable<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Average<T>(this IEnumerable<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Max<T>(this IEnumerable<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Max<T>(this IEnumerable<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Min<T>(this IEnumerable<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Min<T>(this IEnumerable<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Sum<T>(this IEnumerable<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Sum<T>(this IEnumerable<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this IEnumerable<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this IEnumerable<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Max<T>(this IEnumerable<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Max<T>(this IEnumerable<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Min<T>(this IEnumerable<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Min<T>(this IEnumerable<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Sum<T>(this IEnumerable<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Sum<T>(this IEnumerable<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Average<T>(this IEnumerable<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Average<T>(this IEnumerable<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Max<T>(this IEnumerable<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Max<T>(this IEnumerable<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Min<T>(this IEnumerable<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Min<T>(this IEnumerable<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Sum<T>(this T[] @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Sum<T>(this T[] @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this T[] @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this T[] @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Max<T>(this T[] @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Max<T>(this T[] @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Min<T>(this T[] @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Min<T>(this T[] @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Sum<T>(this T[] @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Sum<T>(this T[] @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this T[] @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this T[] @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Max<T>(this T[] @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Max<T>(this T[] @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Min<T>(this T[] @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Min<T>(this T[] @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Sum<T>(this T[] @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Sum<T>(this T[] @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Average<T>(this T[] @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Average<T>(this T[] @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Max<T>(this T[] @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Max<T>(this T[] @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Min<T>(this T[] @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Min<T>(this T[] @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Sum<T>(this T[] @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Sum<T>(this T[] @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this T[] @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this T[] @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Max<T>(this T[] @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Max<T>(this T[] @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Min<T>(this T[] @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Min<T>(this T[] @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Sum<T>(this T[] @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Sum<T>(this T[] @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Average<T>(this T[] @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Average<T>(this T[] @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Max<T>(this T[] @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Max<T>(this T[] @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Min<T>(this T[] @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Min<T>(this T[] @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Sum<T>(this IOrderedEnumerable<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Sum<T>(this IOrderedEnumerable<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this IOrderedEnumerable<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this IOrderedEnumerable<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Max<T>(this IOrderedEnumerable<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Max<T>(this IOrderedEnumerable<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Min<T>(this IOrderedEnumerable<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Min<T>(this IOrderedEnumerable<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Sum<T>(this IOrderedEnumerable<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Sum<T>(this IOrderedEnumerable<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this IOrderedEnumerable<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this IOrderedEnumerable<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Max<T>(this IOrderedEnumerable<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Max<T>(this IOrderedEnumerable<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Min<T>(this IOrderedEnumerable<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Min<T>(this IOrderedEnumerable<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Sum<T>(this IOrderedEnumerable<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Sum<T>(this IOrderedEnumerable<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Average<T>(this IOrderedEnumerable<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Average<T>(this IOrderedEnumerable<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Max<T>(this IOrderedEnumerable<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Max<T>(this IOrderedEnumerable<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Min<T>(this IOrderedEnumerable<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Min<T>(this IOrderedEnumerable<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Sum<T>(this IOrderedEnumerable<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Sum<T>(this IOrderedEnumerable<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this IOrderedEnumerable<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this IOrderedEnumerable<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Max<T>(this IOrderedEnumerable<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Max<T>(this IOrderedEnumerable<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Min<T>(this IOrderedEnumerable<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Min<T>(this IOrderedEnumerable<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Sum<T>(this IOrderedEnumerable<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Sum<T>(this IOrderedEnumerable<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Average<T>(this IOrderedEnumerable<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Average<T>(this IOrderedEnumerable<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Max<T>(this IOrderedEnumerable<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Max<T>(this IOrderedEnumerable<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Min<T>(this IOrderedEnumerable<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Min<T>(this IOrderedEnumerable<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Sum<T>(this List<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Sum<T>(this List<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this List<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this List<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Max<T>(this List<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Max<T>(this List<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int> Min<T>(this List<T> @this, Func<T, Task<int>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<int?> Min<T>(this List<T> @this, Func<T, Task<int?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Sum<T>(this List<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Sum<T>(this List<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this List<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this List<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Max<T>(this List<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Max<T>(this List<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long> Min<T>(this List<T> @this, Func<T, Task<long>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<long?> Min<T>(this List<T> @this, Func<T, Task<long?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Sum<T>(this List<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Sum<T>(this List<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Average<T>(this List<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Average<T>(this List<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Max<T>(this List<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Max<T>(this List<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal> Min<T>(this List<T> @this, Func<T, Task<decimal>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<decimal?> Min<T>(this List<T> @this, Func<T, Task<decimal?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Sum<T>(this List<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Sum<T>(this List<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Average<T>(this List<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Average<T>(this List<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Max<T>(this List<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Max<T>(this List<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double> Min<T>(this List<T> @this, Func<T, Task<double>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<double?> Min<T>(this List<T> @this, Func<T, Task<double?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Sum<T>(this List<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Sum<T>(this List<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Average<T>(this List<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Average<T>(this List<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Max<T>(this List<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Max<T>(this List<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float> Min<T>(this List<T> @this, Func<T, Task<float>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
        public static async Task<float?> Min<T>(this List<T> @this, Func<T, Task<float?>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();
            
            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);
            
            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }
        
    }
}