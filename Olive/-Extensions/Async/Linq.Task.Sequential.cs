using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        public static async Task<IEnumerable<TResult>> SequentialSelect<TSource, TResult>(
            this IEnumerable<TSource> @this, Func<TSource, Task<TResult>> selector)
        {
            var result = new List<TResult>();
            foreach (var item in @this)
            {
                var sel = selector(item);
                if (sel == null) continue;
                result.Add(await sel.ConfigureAwait(false));
            }

            return result;
        }

        public static async Task<IEnumerable<TResult>> SequentialSelectMany<TSource, TResult>(
           this IEnumerable<TSource> @this, Func<TSource, Task<IEnumerable<TResult>>> selector)
        {
            var result = new List<TResult>();
            foreach (var item in @this)
            {
                var sel = selector(item);
                if (sel is null) continue;

                var awaited = await sel.ConfigureAwait(false);
                if (awaited != null) result.AddRange(awaited);
            }

            return result;
        }

        public static Task<IEnumerable<TResult>> SequentialSelect<TSource, TResult>(
           this Task<IEnumerable<TSource>> @this, Func<TSource, Task<TResult>> selector)
            => @this.Get(x => x.SequentialSelect(selector));

        public static Task<IEnumerable<TResult>> SequentialSelectMany<TSource, TResult>(
           this Task<IEnumerable<TSource>> @this, Func<TSource, Task<IEnumerable<TResult>>> selector)
            => @this.Get(v => v.SequentialSelectMany(selector));
    }
}