namespace Olive
{
    public static class ApiClientExtensions
    {
        public static IDevCommandsConfig AddClearApiCache(this IDevCommandsConfig config)
        {
            config.Add("clear-api-cache", () => ApiClient.DisposeCache(), "Clear Api Cache");
            return config;
        }
    }
}