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
                result.Add(await sel);
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
                if (sel == null) continue;

                try
                {
                    var awaited = await sel;
                    if (awaited != null)
                        result.AddRange(awaited);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

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