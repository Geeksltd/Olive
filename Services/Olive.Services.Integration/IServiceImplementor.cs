namespace Olive.Services.Integration
{
    // /// <summary>
    // /// All integration services should implement this interface.
    // /// Each service should be registered At application start-up time by calling:
    // /// IntegrationManager.Register[]
    // /// </summary>
    // public interface IIntegrationService : IIntegrationService<string, string>
    // {
    //    /// <summary>
    //    /// It will process the specified request, send it to the remote service, and return the response.
    //    /// </summary>
    //    string GetResponse(string request);
    // }

    public interface IServiceImplementor<in TRequest, out TResponse>
    {
        /// <summary>
        /// It will process the specified request, send it to the remote service, and return the response.
        /// </summary>
        TResponse GetResponse(TRequest request);
    }
}