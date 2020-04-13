namespace Olive
{
    using System;
    using System.Threading.Tasks;

    public class AwaitableEvent
    {
        internal Func<Task> AsyncHandler;

        /// <summary>
        /// Invokes an async handler. This should be called only once.
        /// </summary>
        public void Do(Func<Task> work)
        {
            if (AsyncHandler is null) AsyncHandler = work;
            else throw new Exception("Do() is already called once.");
        }
    }
}