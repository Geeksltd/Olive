namespace Olive
{
    using System;

    public class AsyncEventHandlingException : Exception
    {
        public AsyncEventHandlingException(string message, Exception innerException) : base(message, innerException) { }
    }
}