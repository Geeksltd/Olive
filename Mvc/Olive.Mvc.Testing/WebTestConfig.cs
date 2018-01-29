using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Olive.Web;

namespace Olive.Mvc.Testing
{
    public interface IWebTestConfig
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

    public class WebTestConfig : IWebTestConfig
    {
        internal static Dictionary<string, Func<Task<bool>>> Handlers = new Dictionary<string, Func<Task<bool>>>();
        internal static Dictionary<string, string> UserCommands = new Dictionary<string, string>();
        static bool? isActive;

        internal static Func<Task> ReferenceDataCreator;

        public static bool IsAutoExecMode { get; internal set; }

        internal static void SetRunner()
        {
            if (Context.Request.Has("runner")) IsAutoExecMode = true;
        }

        /// <summary>
        /// Determines whether the application is running under Temp database mode.
        /// </summary>
        public static bool IsActive()
        {
            if (isActive.HasValue) return isActive.Value;

            var db = Config.GetConnectionString("AppDatabase")
                .Get(c => new System.Data.SqlClient.SqlConnectionStringBuilder(c).InitialCatalog);

            db = db.Or("").ToLower().TrimStart("[").TrimEnd("]").Trim('`');

            isActive = db.EndsWith(".temp");

            return isActive.Value;
        }

        public bool AddDefaultHandlers { get; set; } = true;

        public void Add(string command, Func<Task<bool>> handler, string userCommandText = null)
        {
            Handlers[command] = handler;

            if (userCommandText.HasValue())
                UserCommands.Add(command, userCommandText);
        }

        public void Add(string command, Func<Task> handler, string userCommandText = null)
        {
            Add(command, async () => { await handler(); return false; }, userCommandText);
        }

        public static async Task<bool> Run(string command)
        {
            if (command.IsEmpty())
                throw new Exception("No command is specified.");

            if (!IsActive())
                throw new Exception("Invalid command in non-TDD execution mode. Database name must end with .Temp.");

            var action = Handlers.GetValueOrDefault(command);
            if (action == null)
                throw new Exception("No Web test handler is registered for command: " + command);

            return await action();
        }
    }
}