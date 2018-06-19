namespace Olive
{
    public class ApiClientEventArg
    {
        public ApiClientEventArg(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}