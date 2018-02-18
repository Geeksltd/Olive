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
        [Obsolete("Use Task.Factory.RunSync() instead.", error: true)]
        public static TResult AwaitResult<TResult>(this Task<TResult> task) => Task.Run(async () => await task).Result;

        /// <summary>
        /// If the task is not completed already it throws an exception warning you to await the task.
        /// If the task wraps an exception, the wrapped exception will be thrown.
        /// Otherwise the result will be returned.
        /// Use this instead of calling the Result property when you know that the result is ready to avoid deadlocks.
        /// </summary>
        public static TResult GetAlreadyCompletedResult<TResult>(this Task<TResult> task)
        {
            if (task == null) return default(TResult);

            if (!task.IsCompleted)
                throw new InvalidOperationException("This task is not completed yet. Do you need to await it?");

            if (task.Exception != null)
                throw task.Exception.InnerException;

            return task.Result;
        }

        /// <summary>
        /// Runs a specified task in a new thread to prevent deadlock (context switch race).
        /// </summary> 
        public static void RunSync(this TaskFactory factory, Func<Task> task)
        {
            try
            {
                var actualTask = new Task<object>(task);
                actualTask.RunSynchronously();
                actualTask.Wait(); // To get the exception                
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Runs a specified task in a new thread to prevent deadlock (context switch race).
        /// </summary> 
        public static TResult RunSync<TResult>(this TaskFactory factory, Func<Task<TResult>> task)
        {
            try
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
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
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
            where TOriginal : TTarget => task.ContinueWith(t => (TTarget)t.GetAlreadyCompletedResult());

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

        /// <summary>
        /// A shorter more readable alternative to ContinueWith().
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TResult> Get<TSource, TResult>(this Task<TSource> sourceTask, Func<TSource, TResult> expression)
            => expression(await sourceTask.ConfigureAwait(continueOnCapturedContext: false));

        /// <summary>
        /// A shorter more readable alternative to nested ContinueWith() methods.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<TResult> Get<TSource, TResult>(this Task<TSource> sourceTask, Func<TSource, Task<TResult>> expression)
        {
            var item = await sourceTask.ConfigureAwait(continueOnCapturedContext: false);
            return await expression(item).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// A shorter more readable alternative to nested ContinueWith() methods.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task Then<TSource, TResult>(this Task<TSource> sourceTask, Action<TSource> action)
        {
            var item = await sourceTask.ConfigureAwait(continueOnCapturedContext: false);
            action(item);
        }
    }
}