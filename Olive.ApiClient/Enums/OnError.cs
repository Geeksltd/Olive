using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Olive.ApiClient
{
    public enum OnError { Throw, Ignore, IgnoreAndNotify }

    public static class EnumExtensions
    {
        public static Task Apply(this OnError strategy, string error) => Apply(strategy, new Exception(error));

        public static Task Apply(this OnError strategy, Exception error, string friendlyMessage = null)
        {
            if (error == null) return Task.CompletedTask;

            Debug.WriteLine(friendlyMessage.WithSuffix(": " + error));

            switch (strategy)
            {
                case OnError.IgnoreAndNotify:
                    // TODO: Log.NotifyByEmail...()
                    return Task.CompletedTask;
                case OnError.Ignore: return Task.CompletedTask;
                case OnError.Throw: throw error;
                default: throw new NotSupportedException(strategy + " is not implemented.");
            }
        }
    }
}