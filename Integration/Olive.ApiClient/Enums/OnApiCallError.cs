using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Olive
{
    public enum OnApiCallError { Throw, Ignore, IgnoreAndNotify }

    public static class EnumExtensions
    {
        public static Task Apply(this OnApiCallError strategy, string error) => Apply(strategy, new Exception(error));

        public static async Task Apply(this OnApiCallError strategy, Exception error, string friendlyMessage = null)
        {
            if (error == null) return Task.CompletedTask;

            Debug.WriteLine(friendlyMessage.WithSuffix(": " + error));

            switch (strategy)
            {
                case OnApiCallError.IgnoreAndNotify:                    
                    await ApiClient.PublishEvent.Raise(new ApiClientEventArg("Failed to get fresh results. Using the latest available cache."));
                    break;                    
                case OnApiCallError.Ignore: return;
                case OnApiCallError.Throw: throw new Exception(error);
                default: throw new NotSupportedException(strategy + " is not implemented.");
            }
        }
    }
}
