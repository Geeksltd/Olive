namespace Olive.Entities
{
    using System;

    public class ValidationException : Exception
    {
        public ValidationException() { }
        public ValidationException(string messageFormat, params object[] arguments) : base(string.Format(messageFormat, arguments)) { }
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception inner) : base(message, inner) { }

        public string InvalidPropertyName { get; set; }

        public bool IsMessageTranslated { get; set; }
    }
}