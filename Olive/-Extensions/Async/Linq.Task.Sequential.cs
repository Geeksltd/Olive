using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        public static async Task<IEnumerable<TResult>> SequentialSelect<TSource, TResult>(
            this IEnumerable<TSource> @this, Func<TSource, Task<TResult>> selector)
        {
            var result = new List<TResult>();
            foreach (var item in @this) result.Add(await selector(item));
            return result;
        }

        public static async Task<IEnumerable<TResult>> SequentialSelectMany<TSource, TResult>(
           this IEnumerable<TSource> @this, Func<TSource, Task<IEnumerable<TResult>>> selector)
        {
            var result = new List<TResult>();
            foreach (var item in @this) result.AddRange(await selector(item));
            return result;
        }

        public static async Task<IEnumerable<TResult>> SequentialSelect<TSource, TResult>(
           this Task<IEnumerable<TSource>> @this, Func<TSource, Task<TResult>> selector)
            => await (await @this).SequentialSelect(selector);

        public static async Task<IEnumerable<TResult>> SequentialSelectMany<TSource, TResult>(
           this Task<IEnumerable<TSource>> @this, Func<TSource, Task<IEnumerable<TResult>>> selector)
            => await (await @this).SequentialSelectMany(selector);
    }
}