using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Olive
{
    public enum OnApiCallError { Throw, Ignore, IgnoreAndNotify }

    public static class EnumExtensions
    {
        public static Task Apply(this OnApiCallError strategy, string error, string url, int age) => Apply(strategy, new Exception(error), url, age);

        public static Task Apply(this OnApiCallError strategy, Exception error, string url, int age, string friendlyMessage = null)
        {
            if (error == null) return Task.CompletedTask;

            Debug.WriteLine(friendlyMessage.WithSuffix(": " + error));

            switch (strategy)
            {
                case OnApiCallError.IgnoreAndNotify:
                    {
                        ApiClient.UsingCacheInsteadOfFresh.Raise(new CachedApiUsageArgs
                        {
                            Message = $"Failed to get fresh results from {url} Using the latest cache from {age} days ago.",
                            OriginalErrorMessage = error.Message,
                            FailedUrl = url,
                            CacheAge = age
                        });

                        return Task.CompletedTask;
                    }
                case OnApiCallError.Ignore: return Task.CompletedTask;
                case OnApiCallError.Throw: return Task.FromException(error);
                default: throw new NotSupportedException(strategy + " is not implemented.");
            }
        }
    }
}