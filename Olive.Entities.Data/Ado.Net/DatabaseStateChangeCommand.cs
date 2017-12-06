using System;
using System.Data;

namespace Olive.Entities.Data
{
    public class DatabaseStateChangeCommand
    {
        public static event Action<DatabaseStateChangeCommand> ExecutedChangeCommand;

        public string CommandText { get; private set; }
        public CommandType CommandType { get; private set; }
        public IDataParameter[] Params { get; private set; }

        internal static void Raise(string command, CommandType type, IDataParameter[] @params)
        {
            if (ExecutedChangeCommand == null) return;

            var item = new DatabaseStateChangeCommand
            {
                CommandText = command,
                CommandType = type,
                Params = @params
            };

            ExecutedChangeCommand?.Invoke(item);
        }
    }
}
