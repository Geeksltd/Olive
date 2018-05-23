using System;
using System.Threading.Tasks;

namespace Olive
{
    public interface IDevCommandsConfig
    {
        bool AddDefaultHandlers { get; set; }

        /// <summary>Registers a web test command handler.</summary>
        /// <param name="handler">A handler that is called when the command is received.
        /// It should return whether the request should exit.</param>
        void Add(string command, Func<Task<bool>> handler, string userCommandText = null);

        /// <summary>Registers a web test command handler.</summary>
        /// <param name="handler">A handler that is called when the command is received.</param>
        void Add(string command, Func<Task> handler, string userCommandText = null);
    }
}