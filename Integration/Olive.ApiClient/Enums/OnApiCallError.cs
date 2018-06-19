using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Olive
{
    public enum OnApiCallError { Throw, Ignore, IgnoreAndNotify }

    public static class EnumExtensions
    {
        public static Task Apply(this OnApiCallError strategy, string errorMessage)
        {
            var error = new Exception(errorMessage);

            Debug.WriteLine(errorMessage);

            switch (strategy)
            {
                case OnApiCallError.IgnoreAndNotify:
                    {
                        return Task.CompletedTask;
                    }
                case OnApiCallError.Ignore: return Task.CompletedTask;
                case OnApiCallError.Throw: return Task.FromException(error);
                default: throw new NotSupportedException(strategy + " is not implemented.");
            }
        }
    }
}
