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
        
                
        public static Task<int?> Sum(this Task<IEnumerable<int?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<int> Sum(this Task<IEnumerable<int>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<IEnumerable<int?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<IEnumerable<int>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<int?> Max(this Task<IEnumerable<int?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<int> Max(this Task<IEnumerable<int>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<int?> Min(this Task<IEnumerable<int?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<int> Min(this Task<IEnumerable<int>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<long?> Sum(this Task<IEnumerable<long?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<long> Sum(this Task<IEnumerable<long>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<IEnumerable<long?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<IEnumerable<long>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<long?> Max(this Task<IEnumerable<long?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<long> Max(this Task<IEnumerable<long>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<long?> Min(this Task<IEnumerable<long?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<long> Min(this Task<IEnumerable<long>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<decimal?> Sum(this Task<IEnumerable<decimal?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<decimal> Sum(this Task<IEnumerable<decimal>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<decimal?> Average(this Task<IEnumerable<decimal?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<decimal> Average(this Task<IEnumerable<decimal>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<decimal?> Max(this Task<IEnumerable<decimal?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<decimal> Max(this Task<IEnumerable<decimal>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<decimal?> Min(this Task<IEnumerable<decimal?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<decimal> Min(this Task<IEnumerable<decimal>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<double?> Sum(this Task<IEnumerable<double?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<double> Sum(this Task<IEnumerable<double>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<IEnumerable<double?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<IEnumerable<double>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<double?> Max(this Task<IEnumerable<double?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<double> Max(this Task<IEnumerable<double>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<double?> Min(this Task<IEnumerable<double?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<double> Min(this Task<IEnumerable<double>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<float?> Sum(this Task<IEnumerable<float?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<float> Sum(this Task<IEnumerable<float>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<float?> Average(this Task<IEnumerable<float?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<float> Average(this Task<IEnumerable<float>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<float?> Max(this Task<IEnumerable<float?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<float> Max(this Task<IEnumerable<float>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<float?> Min(this Task<IEnumerable<float?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<float> Min(this Task<IEnumerable<float>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<int?> Sum(this Task<int?[]> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<int> Sum(this Task<int[]> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<int?[]> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<int[]> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<int?> Max(this Task<int?[]> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<int> Max(this Task<int[]> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<int?> Min(this Task<int?[]> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<int> Min(this Task<int[]> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<long?> Sum(this Task<long?[]> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<long> Sum(this Task<long[]> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<long?[]> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<long[]> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<long?> Max(this Task<long?[]> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<long> Max(this Task<long[]> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<long?> Min(this Task<long?[]> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<long> Min(this Task<long[]> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<decimal?> Sum(this Task<decimal?[]> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<decimal> Sum(this Task<decimal[]> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<decimal?> Average(this Task<decimal?[]> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<decimal> Average(this Task<decimal[]> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<decimal?> Max(this Task<decimal?[]> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<decimal> Max(this Task<decimal[]> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<decimal?> Min(this Task<decimal?[]> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<decimal> Min(this Task<decimal[]> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<double?> Sum(this Task<double?[]> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<double> Sum(this Task<double[]> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<double?[]> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<double[]> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<double?> Max(this Task<double?[]> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<double> Max(this Task<double[]> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<double?> Min(this Task<double?[]> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<double> Min(this Task<double[]> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<float?> Sum(this Task<float?[]> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<float> Sum(this Task<float[]> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<float?> Average(this Task<float?[]> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<float> Average(this Task<float[]> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<float?> Max(this Task<float?[]> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<float> Max(this Task<float[]> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<float?> Min(this Task<float?[]> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<float> Min(this Task<float[]> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<int?> Sum(this Task<IOrderedEnumerable<int?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<int> Sum(this Task<IOrderedEnumerable<int>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<IOrderedEnumerable<int?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<IOrderedEnumerable<int>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<int?> Max(this Task<IOrderedEnumerable<int?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<int> Max(this Task<IOrderedEnumerable<int>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<int?> Min(this Task<IOrderedEnumerable<int?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<int> Min(this Task<IOrderedEnumerable<int>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<long?> Sum(this Task<IOrderedEnumerable<long?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<long> Sum(this Task<IOrderedEnumerable<long>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<IOrderedEnumerable<long?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<IOrderedEnumerable<long>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<long?> Max(this Task<IOrderedEnumerable<long?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<long> Max(this Task<IOrderedEnumerable<long>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<long?> Min(this Task<IOrderedEnumerable<long?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<long> Min(this Task<IOrderedEnumerable<long>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<decimal?> Sum(this Task<IOrderedEnumerable<decimal?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<decimal> Sum(this Task<IOrderedEnumerable<decimal>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<decimal?> Average(this Task<IOrderedEnumerable<decimal?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<decimal> Average(this Task<IOrderedEnumerable<decimal>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<decimal?> Max(this Task<IOrderedEnumerable<decimal?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<decimal> Max(this Task<IOrderedEnumerable<decimal>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<decimal?> Min(this Task<IOrderedEnumerable<decimal?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<decimal> Min(this Task<IOrderedEnumerable<decimal>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<double?> Sum(this Task<IOrderedEnumerable<double?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<double> Sum(this Task<IOrderedEnumerable<double>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<IOrderedEnumerable<double?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<IOrderedEnumerable<double>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<double?> Max(this Task<IOrderedEnumerable<double?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<double> Max(this Task<IOrderedEnumerable<double>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<double?> Min(this Task<IOrderedEnumerable<double?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<double> Min(this Task<IOrderedEnumerable<double>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<float?> Sum(this Task<IOrderedEnumerable<float?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<float> Sum(this Task<IOrderedEnumerable<float>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<float?> Average(this Task<IOrderedEnumerable<float?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<float> Average(this Task<IOrderedEnumerable<float>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<float?> Max(this Task<IOrderedEnumerable<float?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<float> Max(this Task<IOrderedEnumerable<float>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<float?> Min(this Task<IOrderedEnumerable<float?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<float> Min(this Task<IOrderedEnumerable<float>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<int?> Sum(this Task<List<int?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<int> Sum(this Task<List<int>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<List<int?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<List<int>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<int?> Max(this Task<List<int?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<int> Max(this Task<List<int>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<int?> Min(this Task<List<int?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<int> Min(this Task<List<int>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<long?> Sum(this Task<List<long?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<long> Sum(this Task<List<long>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<List<long?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<List<long>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<long?> Max(this Task<List<long?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<long> Max(this Task<List<long>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<long?> Min(this Task<List<long?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<long> Min(this Task<List<long>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<decimal?> Sum(this Task<List<decimal?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<decimal> Sum(this Task<List<decimal>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<decimal?> Average(this Task<List<decimal?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<decimal> Average(this Task<List<decimal>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<decimal?> Max(this Task<List<decimal?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<decimal> Max(this Task<List<decimal>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<decimal?> Min(this Task<List<decimal?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<decimal> Min(this Task<List<decimal>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<double?> Sum(this Task<List<double?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<double> Sum(this Task<List<double>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<double?> Average(this Task<List<double?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<double> Average(this Task<List<double>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<double?> Max(this Task<List<double?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<double> Max(this Task<List<double>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<double?> Min(this Task<List<double?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<double> Min(this Task<List<double>> @this) => @this.Get(v=> v.Min(x => x));
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
        
                
        public static Task<float?> Sum(this Task<List<float?>> @this) => @this.Get(v=> v.Sum(x => x));
        public static Task<float> Sum(this Task<List<float>> @this) => @this.Get(v=> v.Sum(x => x));
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
        
                
        public static Task<float?> Average(this Task<List<float?>> @this) => @this.Get(v=> v.Average(x => x));
        public static Task<float> Average(this Task<List<float>> @this) => @this.Get(v=> v.Average(x => x));
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
        
                
        public static Task<float?> Max(this Task<List<float?>> @this) => @this.Get(v=> v.Max(x => x));
        public static Task<float> Max(this Task<List<float>> @this) => @this.Get(v=> v.Max(x => x));
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
        
                
        public static Task<float?> Min(this Task<List<float?>> @this) => @this.Get(v=> v.Min(x => x));
        public static Task<float> Min(this Task<List<float>> @this) => @this.Get(v=> v.Min(x => x));
    }
}