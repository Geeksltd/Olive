using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Olive
{
    public class DevCommandsOptions
    {
        public DevCommandsOptions(IServiceCollection services) => Services = services;

        public IServiceCollection Services { get; }
    }

    /// <summary>
    /// A command that can be sent to the application during development time.
    /// </summary>
    public interface IDevCommand
    {
        /// <summary>
        /// Programmatic name of the command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A text or title for this command, shown to the developer on the UI.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Determines whether this command is usable in the current context.
        /// </summary>
        bool IsEnabled();

        /// <summary>
        /// Invokes the command.
        /// After the command execution, if it returns null or empty, the user will be redirected to the http url referrer, or the root of the application.
        /// Otherwise the returned string value will be rendered in the http response.
        /// </summary>
        Task<string> Run();
    }
}