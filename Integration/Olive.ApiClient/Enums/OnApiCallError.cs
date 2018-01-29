using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Olive
{
    public enum OnApiCallError { Throw, Ignore, IgnoreAndNotify }

    public static class EnumExtensions
    {
        public static Task Apply(this OnApiCallError strategy, string error) => Apply(strategy, new Exception(error));

        public static Task Apply(this OnApiCallError strategy, Exception error, string friendlyMessage = null)
        {
            if (error == null) return Task.CompletedTask;

            Debug.WriteLine(friendlyMessage.WithSuffix(": " + error));

            switch (strategy)
            {
                case OnApiCallError.IgnoreAndNotify:
                    // TODO: Log.NotifyByEmail...()
                    return Task.CompletedTask;
                case OnApiCallError.Ignore: return Task.CompletedTask;
                case OnApiCallError.Throw: throw error;
                default: throw new NotSupportedException(strategy + " is not implemented.");
            }
        }
    }
}