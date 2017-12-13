using System.Collections.Generic;
using System.Xml.Linq;
using Olive.Entities.Data;
using Olive.Web;

namespace Olive.Services.Testing
{
    class DatabaseChangeWatcher
    {
        static List<XElement> Changes = new List<XElement>();

        static DatabaseChangeWatcher()
        {
            DatabaseStateChangeCommand.ExecutedChangeCommand += DatabaseStateChangeCommand_ExecutedChangeCommand;
        }

        static void DatabaseStateChangeCommand_ExecutedChangeCommand(DatabaseStateChangeCommand change)
        {
            var node = new XElement("Change");
            if (change.CommandType != System.Data.CommandType.Text)
                node.Add(new XAttribute("Type", change.CommandType.ToString()));

            node.Add(new XAttribute("Command", change.CommandText));

            foreach (var p in change.Params)
                node.Add(new XElement("Param",
                    new XAttribute("Name", p.ParameterName),
                    new XAttribute("Value", p.Value),
                    new XAttribute("Type", p.DbType)));

            Changes.Add(node);
        }

        internal static void Restart() => Changes.Clear();

        internal static void DispatchChanges()
        {
            var response = new XElement("Changes", Changes).ToString();
            Changes.Clear();
            Context.Response.EndWith(response, "text/xml");
        }
    }
}