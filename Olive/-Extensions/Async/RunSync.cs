using System;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Runs a specified task synchronously.
        /// </summary> 
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static void RunSync(this TaskFactory @this, Func<Task> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            @this.StartNew(task).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs a specified task synchronously.
        /// </summary> 
        [EscapeGCop("I AM the solution to the GCop warning itself!")]
        public static TResult RunSync<TResult>(this TaskFactory @this, Func<Task<TResult>> task)
        {
            return @this.StartNew(task).Unwrap().GetAwaiter().GetResult();
        }
    }
}