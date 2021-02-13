namespace Olive
{
    using System.Collections.Concurrent;
    using System.Threading;

    public static class CallContext<T>
    {
        static readonly ConcurrentDictionary<string, AsyncLocal<T>> State
            = new ConcurrentDictionary<string, AsyncLocal<T>>();

        /// <summary> 
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param> 
        public static void SetData(string name, T data)
            => State.GetOrAdd(name, _ => new AsyncLocal<T>()).Value = data;

        /// <summary>
        /// Retrieves an object with the specified name from the current call context/>.
        /// </summary>        
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name,
        /// or a default value for <typeparamref name="T"/> if none is found.</returns>
        public static T GetData(string name) =>
            State.TryGetValue(name, out var data) ? data.Value : default;
    }
}