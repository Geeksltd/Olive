using System;
using System.Threading;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Invokes the specified action for the specified number of times.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="retries">The number of times to try running the action.</param>
        /// <param name="waitBeforeRetries">The time to wait before every two retries.</param>
        /// <param name="onEveryError">The action to run every time the method invokation fails. You can use this to log the error.</param>
        public static void Invoke(this Action action, int retries, TimeSpan waitBeforeRetries, Action<Exception> onEveryError = null)
        {
            var asFunction = new Func<object>(() => { action?.Invoke(); return null; });
            asFunction.Invoke(retries, waitBeforeRetries, onEveryError);
        }

        /// <summary>
        /// Invokes the specified function for the specified number of times.
        /// </summary>
        /// <param name="function">The function to evaluate.</param>
        /// <param name="retries">The number of times to try running the action.</param>
        /// <param name="waitBeforeRetries">The time to wait before every two retries.</param>
        /// <param name="onEveryError">The action to run every time the method invokation fails. You can use this to log the error.</param>
        public static T Invoke<T>(this Func<T> function, int retries, TimeSpan waitBeforeRetries, Action<Exception> onEveryError = null)
        {
            if (retries < 2)
                throw new Exception("retries should be greater than 2.");

            var count = 0;

            while (true)
            {
                try
                {
                    return function();
                }
                catch (Exception ex)
                {
                    count++;

                    onEveryError?.Invoke(ex);

                    if (count == retries)
                    {
                        // Give up:
                        throw;
                    }

                    Thread.Sleep(waitBeforeRetries);
                }
            }
        }
    }
}