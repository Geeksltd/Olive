using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Olive.Mvc.Testing
{
    class DatabaseChangeWatcher
    {
        static List<XElement> Changes = new List<XElement>();
        const string NullObject = "¦¦nil¦¦";

        static DatabaseChangeWatcher()
        {
            DatabaseStateChangeCommand.ExecutedChangeCommand += DatabaseStateChangeCommand_ExecutedChangeCommand;
        }

        static void DatabaseStateChangeCommand_ExecutedChangeCommand(DatabaseStateChangeCommand change)
        {
            var node = new XElement("Change");
            if (change.CommandType != CommandType.Text)
                node.Add(new XAttribute("Type", change.CommandType.ToString()));

            node.Add(new XAttribute("Command", change.CommandText));

            foreach (var p in change.Params)
                node.Add(new XElement("Param",
                    new XAttribute("Name", p.ParameterName),
                    new XAttribute("Value", p.Value == DBNull.Value ? NullObject : p.Value),
                    new XAttribute("Type", p.DbType)));

            Changes.Add(node);
        }

        internal static void Restart() => Changes.Clear();

        internal static string DispatchChanges()
        {
            var response = new XElement("Changes", Changes).ToString();
            Changes.Clear();
            return response;
        }

        public static async Task RunChanges()
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(Context.Current.Request().Param("Data"))));

                var connectionStringKey = xmlDocument.GetElementsByTagName("ConnectionStringKey")[0].FirstChild.Value;
                var dataProviderType = xmlDocument.GetElementsByTagName("DataProviderType")[0].FirstChild.Value;
                var changesNodeList = xmlDocument.GetElementsByTagName("Changes")[0];
                var access = DataAccess.Create(dataProviderType);

                foreach (var xmlElement in changesNodeList.ChildNodes.OfType<XmlElement>())
                {
                    var command = xmlElement.GetAttribute("Command").Replace("&#xD;&#xA;", Environment.NewLine);
                    var commandType = CommandType.Text;
                    if (!xmlElement.GetAttribute("Type").IsEmpty())
                        commandType = xmlElement.GetAttribute("Type").To<CommandType>();

                    var dataParameters = new List<IDataParameter>();

                    foreach (var innerXmlElement in xmlElement.ChildNodes.OfType<XmlElement>())
                    {
                        var value = innerXmlElement.GetAttribute("Value");
                        var sqlDbType = innerXmlElement.GetAttribute("Type").To<DbType>();
                        var sqlParameter = access.CreateParameter(innerXmlElement.GetAttribute("Name"), value == NullObject ? (object)DBNull.Value : value);

                        switch (sqlDbType)
                        {
                            case DbType.DateTime:
                                sqlParameter.DbType = DbType.DateTime;
                                sqlParameter.Value = (value.IsEmpty() || value == NullObject) ? sqlParameter.Value : XmlConvert.ToDateTimeOffset(sqlParameter.Value.ToString()).DateTime;
                                break;
                            case DbType.Guid:
                                sqlParameter.DbType = DbType.Guid;
                                sqlParameter.Value = (value.IsEmpty() || value == NullObject) ? sqlParameter.Value : sqlParameter.Value?.ToString().To<Guid>();
                                break;
                            case DbType.DateTime2:
                                sqlParameter.DbType = DbType.DateTime2;
                                sqlParameter.Value = (value.IsEmpty() || value == NullObject) ? sqlParameter.Value : XmlConvert.ToDateTimeOffset(sqlParameter.Value.ToString()).DateTime;
                                break;
                            case DbType.Time:
                                sqlParameter.Value = (value.IsEmpty() || value == NullObject) ? sqlParameter.Value : XmlConvert.ToTimeSpan(sqlParameter.Value.ToString());
                                break;
                            case DbType.Boolean:
                                sqlParameter.DbType = DbType.Boolean;
                                sqlParameter.Value = (value.IsEmpty() || value == NullObject) ? sqlParameter.Value : sqlParameter.Value?.ToString().To<bool>();
                                break;
                        }

                        dataParameters.Add(sqlParameter);
                    }

                    using (new DatabaseContext(Context.Current.GetService<IConnectionStringProvider>()
                        .GetConnectionString(connectionStringKey)))
                        await access.ExecuteNonQuery(command, commandType, dataParameters.ToArray());
                }

                Context.Current.Database().Cache.ClearAll();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}