namespace Olive
{
    public class AsyncEventHandlingException : Exception
    {
        public AsyncEventHandlingException(string message, Exception innerException) : base(message, innerException) { }
    }
}