using System;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.BlobAws
{
    /// <summary>
    /// helper class borrowed from Microsoft
    /// </summary>
    public static class AsyncHelper
    {
        static readonly TaskFactory taskFactory = new
            TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return taskFactory
                           .StartNew(func)
                           .Unwrap()
                           .GetAwaiter()
                           .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            taskFactory
                   .StartNew(func)
                   .Unwrap()
                   .GetAwaiter()
                   .GetResult();
        }
    }
}
