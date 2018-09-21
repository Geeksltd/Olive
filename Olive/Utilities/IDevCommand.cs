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
        /// Invokes the command, and returns whether the command was executed successfully.
        /// </summary>
        Task<bool> Run();

        /// <summary>
        /// Determines whether this command is usable in the current context.
        /// </summary>
        bool IsEnabled();
    }
}