using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        {
            factory.StartNew(task, TaskCreationOptions.LongRunning)
              .ContinueWith(t =>
              {
                  if (t.Exception == null) return;
                  System.Diagnostics.Debug.Fail("Error in calling TaskFactory.RunSync: " + t.Exception.InnerException.ToLogString());
                  throw t.Exception.InnerException;
              })
              .Wait();
        }

        /// <summary>
        /// Runs a specified task in a new thread to prevent deadlock (context switch race).
        /// </summary> 
        public static TResult RunSync<TResult>(this TaskFactory factory, Func<Task<TResult>> task)
        {
            return factory.StartNew(task, TaskCreationOptions.LongRunning)
                 .ContinueWith(t =>
                 {
                     if (t.Exception == null) return t.Result;

                     System.Diagnostics.Debug.Fail("Error in calling TaskFactory.RunSync: " + t.Exception.InnerException.ToLogString());
                     throw t.Exception.InnerException;
                 })
                 .Result.Result;
        }

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

        public static async Task<T[]> ToArray<T>(this Task<IEnumerable<T>> list) => (await list).ToArray();

        public static async Task<IEnumerable<T>> AwaitAll<T>(this IEnumerable<Task<T>> list)
            => await Task.WhenAll(list);

        /// <summary>
        /// Casts the result type of the input task as if it were covariant.
        /// </summary>
        /// <typeparam name="TOriginal">The original result type of the task</typeparam>
        /// <typeparam name="TTarget">The covariant type to return</typeparam>
        /// <param name="task">The target task to cast</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<TTarget> AsTask<TOriginal, TTarget>(this Task<TOriginal> task)
            where TOriginal : TTarget => task.ContinueWith(t => (TTarget)t.Result);

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<T[]> task)
            => task.AsTask<T[], IEnumerable<T>>();

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<List<T>> task)
            => task.AsTask<List<T>, IEnumerable<T>>();

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<IList<T>> task)
            => task.AsTask<IList<T>, IEnumerable<T>>();

        /// <summary>
        /// Casts it into a Task of IEnumerable, so the Linq methods can be invoked on it.
        /// </summary> 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<IEnumerable<T>> ForLinq<T>(this Task<IOrderedEnumerable<T>> task)
            => task.AsTask<IOrderedEnumerable<T>, IEnumerable<T>>();
    }
}