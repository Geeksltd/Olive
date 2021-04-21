using System;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.BlobAws
{
    public static class AsyncHelper
    {
        static readonly TaskFactory TaskFactory = new
            TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

        /// <summary>
        /// Make Async method Sync
        /// </summary>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return TaskFactory
                           .StartNew(func)
                           .Unwrap()
                           .GetAwaiter()
                           .GetResult();
        }

        /// <summary>
        /// Make Async method Sync
        /// </summary>
        public static void RunSync(Func<Task> func)
        {
            TaskFactory
                   .StartNew(func)
                   .Unwrap()
                   .GetAwaiter()
                   .GetResult();
        }
    }
}
