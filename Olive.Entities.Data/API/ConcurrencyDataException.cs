namespace Olive.Entities.Data
{
    public class ConcurrencyException : ValidationException
    {
        public ConcurrencyException() { }

        public ConcurrencyException(string message) : base(message) { }
    }
}
