/* 
 * Linq methods that are:
 *       defined on Task<IEnumerable<T>>
 *       and take lambdas that either return the result or a task of result
 *       
 * Note: These methods do not end with "Async".
 * They are intended to enable chaining of async linq calls.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        public static async Task<IEnumerable<TResult>> Select<TSource, TResult>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TResult> func)
        {
            var awaited = await list;
            return awaited.Select(func);
        }

        public static async Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, Task<TResult>> func)
        {
            var awaited = await list;
            var resultTasks = awaited.Select(func);
            return await Task.WhenAll(resultTasks);
        }

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, IEnumerable<TResult>> func)
        {
            var awaited = await list;
            return awaited.SelectMany(func);
        }

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, IEnumerable<Task<TResult>>> func)
        {
            var awaited = await list;
            var resultTasks = awaited.SelectMany(func);
            return await Task.WhenAll(resultTasks);
        }

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
         this Task<IEnumerable<TSource>> list, Func<TSource, Task<IEnumerable<TResult>>> func)
        {
            var awaited = await list;
            var resultTasks = awaited.Select(func);
            var awaitedResults = await Task.WhenAll(resultTasks);
            return awaitedResults.SelectMany(x => x);
        }

      
    }
}