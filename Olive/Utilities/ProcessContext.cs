using System;

namespace Olive
{
    /// <summary>
    /// Provides process context data sharing mechanism to pass arguments and data around execution in a shared pipeline.
    /// It supports context nesting.
    /// </summary>
    public class ProcessContext<T> : IDisposable
    {
        readonly string Key;

        /// <summary>Gets or sets the Data of this ProcessContext.</summary>
        public T Data { get; }

        /// <summary> 
        /// Creates a new Process Context.
        /// </summary>
        public ProcessContext(T data) : this(null, data) { }

        /// <summary>
        /// Gets the data of the current context with default key (null).
        /// </summary>
        public static T Current => CallContext<T>.GetData(GetKey());

        /// <summary>
        /// Creates a new Process Context with the specified key and data.
        /// </summary>
        public ProcessContext(string key, T data)
        {
            Data = data;
            Key = GetKey(key);
            CallContext<T>.SetData(Key, data);
        }

        static string GetKey(string key = null) => $"ProcessContext:{typeof(T).FullName}|K:{key}";

        /// <summary>
        /// Gets the data of the current context with the specified key.
        /// </summary>
        public static T GetCurrent(string key) => CallContext<T>.GetData(GetKey(key));

        /// <summary>
        /// Disposes the current process context and switches the actual context to the containing process context.
        /// </summary>
        public void Dispose()
        {
            try { CallContext<T>.SetData(GetKey(Key), default); }
            catch
            {
                // No logging is needed
            }
        }
    }

    /// <summary>
    /// Provides a facade for easiper creation of a Process Context.
    /// </summary>
    public static class ProcessContext
    {
        /// <summary>
        /// Create a process context for the specified object.
        /// To access the context object, you can use ProcessContext&lt;Your Type&gt;.Current.
        /// </summary>
        public static ProcessContext<T> Create<T>(T contextObject) => new ProcessContext<T>(contextObject);

        /// <summary>
        /// Create a process context for the specified object with the specified key.
        /// To access the context object, you can use ProcessContext&lt;Your Type&gt;.GetCurrent(key).
        /// </summary>
        public static ProcessContext<T> Create<T>(string key, T contextObject) => new ProcessContext<T>(key, contextObject);
    }
}