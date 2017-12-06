namespace Olive
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public struct DisposableAwaitable<T> where T : IDisposable
    {
        public readonly Task<T> Task;

        internal DisposableAwaitable(Task<T> task) { Task = task; }

        public TaskAwaiter<T> GetAwaiter() => Task.GetAwaiter();
    }
}