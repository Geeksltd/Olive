using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// It works similar to calling .Result property, but it forces a context switch to prevent deadlocks in UI and ASP.NET context.
        /// </summary>
        public static TResult AwaitResult<TResult>(this Task<TResult> task) => Task.Run(async () => await task).Result;

        /// <summary>
        /// Runs a specified task in a new thread to prevent deadlock (context switch race).
        /// </summary> 
        public static void RunSync(this TaskFactory factory, Func<Task> task)
            => factory.StartNew(task, TaskCreationOptions.LongRunning)
            .Wait();

        /// <summary>
        /// Runs a specified task in a new thread to prevent deadlock (context switch race).
        /// </summary> 
        public static TResult RunSync<TResult>(this TaskFactory factory, Func<Task<TResult>> task)
            => factory.StartNew(task, TaskCreationOptions.LongRunning).Result.Result;

        public static async Task WithTimeout(this Task task, TimeSpan timeout, Action success = null, Action timeoutAction = null)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task) success?.Invoke();

            else
            {
                if (timeoutAction == null) throw new TimeoutException("The task didn't complete within " + timeout + "ms");
                else timeoutAction();
            }
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout, Action success = null, Func<T> timeoutAction = null)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                success?.Invoke();
                await task;
                return task.GetAwaiter().GetResult();
            }

            else
            {
                if (timeoutAction == null) throw new TimeoutException("The task didn't complete within " + timeout + "ms");
                else { return timeoutAction(); }
            }
        }

        public static async Task<List<T>> ToList<T>(this Task<IEnumerable<T>> list) => (await list).ToList();

        public static async Task<IEnumerable<TResult>> Select<TSource, TResult>(
        this Task<IEnumerable<TSource>> list,
        Func<TSource, TResult> func)
        {
            var awaited = await list;
            return awaited.Select(func);
        }

        public static async Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IEnumerable<TSource>> list,
          Func<TSource, Task<TResult>> func)
        {
            var awaited = await list;
            var resultTasks = awaited.Select(func);
            return await Task.WhenAll(resultTasks);
        }

        public static async Task<IEnumerable<T>> AwaitAll<T>(this IEnumerable<Task<T>> list)
            => await Task.WhenAll(list);
    }
}