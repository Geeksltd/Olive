using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {   /// <summary>
        /// Runs a specified task synchronously.
        /// </summary> 
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static void RunSync(this TaskFactory @this, Func<Task> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            @this.StartNew(task).Unwrap().GetAwaiter().GetResult();

            // using (var actualTask = new Task<Task>(task))
            // {
            //    try
            //    {
            //        actualTask.RunSynchronously();
            //        actualTask.WaitAndThrow();
            //        actualTask.Result.WaitAndThrow();
            //    }
            //    catch (AggregateException ex)
            //    {
            //        Log.For(typeof(TaskExtensions)).Error(ex);
            //        throw ex.InnerException;
            //    }
            // }
        }

        /// <summary>
        /// Runs a specified task synchronously.
        /// </summary> 
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static TResult RunSync<TResult>(this TaskFactory @this, Func<Task<TResult>> task)
        {
            return @this.StartNew(task).Unwrap().GetAwaiter().GetResult();

            // try
            // {
            //    return @this.StartNew(task, TaskCreationOptions.LongRunning)
            //         .ContinueWith(t =>
            //         {
            //             if (t.Exception == null) return t.Result;

            //             System.Diagnostics.Debug.Fail("Error in calling TaskFactory.RunSync: " + t.Exception.InnerException.ToLogString());
            //             throw t.Exception.InnerException;
            //         })
            //         .RiskDeadlockAndAwaitResult()
            //         .RiskDeadlockAndAwaitResult();
            // }
            // catch (AggregateException ex)
            // {
            //    throw ex.InnerException;
            // }
        }
    }
}