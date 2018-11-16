using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Olive.Mvc.Testing
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
            if (change.CommandType != CommandType.Text)
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
                xmlDocument.LoadXml(Context.Current.Request().Param("Data").HtmlDecode());

                var connectionStringKey = xmlDocument.GetElementsByTagName("ConnectionStringKey")[0].FirstChild.Value;
                var dataProviderType = xmlDocument.GetElementsByTagName("DataProviderType")[0].FirstChild.Value;
                var changesNodeList = xmlDocument.GetElementsByTagName("Changes")[0];

                foreach (XmlElement xmlElement in changesNodeList.ChildNodes)
                {
                    var command = xmlElement.GetAttribute("Command").Replace("&#xD;&#xA;", Environment.NewLine);
                    var commandType = CommandType.Text;
                    if (!xmlElement.GetAttribute("Type").IsEmpty())
                    {
                        commandType = xmlElement.GetAttribute("Type").To<CommandType>();
                    }

                    var dataParameters = new List<SqlParameter>();

                    foreach (XmlElement innerXmlElement in xmlElement.ChildNodes)
                    {
                        var value = innerXmlElement.GetAttribute("Value");
                        var sqlDbType = innerXmlElement.GetAttribute("Type").To<DbType>();

                        var sqlParameter = new SqlParameter
                        {
                            DbType = sqlDbType,
                            Value = value.IsEmpty() ? (object)DBNull.Value : value,
                            ParameterName = innerXmlElement.GetAttribute("Name"),
                        };

                        switch (sqlDbType)
                        {
                            case DbType.DateTime:
                                sqlParameter.DbType = DbType.DateTime;
                                sqlParameter.SqlDbType = SqlDbType.DateTime;
                                sqlParameter.Value = value.IsEmpty() ? sqlParameter.Value : sqlParameter.Value?.ToString().To<DateTime>();
                                break;
                            case DbType.Guid:
                                sqlParameter.DbType = DbType.Guid;
                                sqlParameter.SqlDbType = SqlDbType.UniqueIdentifier;
                                sqlParameter.Value = value.IsEmpty() ? sqlParameter.Value : sqlParameter.Value?.ToString().To<Guid>();
                                break;
                            case DbType.DateTime2:
                                sqlParameter.DbType = DbType.DateTime2;
                                sqlParameter.SqlDbType = SqlDbType.DateTime2;
                                sqlParameter.Value = value.IsEmpty() ? sqlParameter.Value : sqlParameter.Value?.ToString().To<DateTime>();
                                break;
                            case DbType.Time:
                                sqlParameter.DbType = DbType.Time;
                                sqlParameter.SqlDbType = SqlDbType.Time;
                                sqlParameter.Value = value.IsEmpty() ? sqlParameter.Value : XmlConvert.ToTimeSpan(sqlParameter.Value.ToString());
                                break;
                        }

                        dataParameters.Add(sqlParameter);
                    }

                    // TODO: The following will run the command on the default connection string.
                    // However, each command may have been originally executed on a different one.
                    // In the generated XML, save the connection string key, and data provider type
                    // So we can recreate that here.

                    using (new DatabaseContext(Config.GetConnectionString(connectionStringKey)))
                    {
                        await GetDataAccessor(dataProviderType)
                         .ExecuteNonQuery(command, commandType, dataParameters.ToArray());
                    }
                }

                Context.Current.GetService<ICache>().ClearAll();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        static IDataAccess GetDataAccessor(string dataProviderType)
        {
            switch (dataProviderType)
            {
                case "System.Data.SqlClient":
                    return new DataAccess<SqlConnection>();

                //TODO: other data provider types
                default: throw new Exception("Provider type not found");
            }
        }
    }
}// using Olive.Entities;
 // using Olive.Entities.Data;
 // using System;
 // using System.Collections.Generic;
 // using System.Data;
 // using System.Data.SqlClient;
 // using System.Threading.Tasks;
 // using System.Xml;
 // using System.Xml.Linq;

// namespace Olive.Mvc.Testing
// {
//    class DatabaseChangeWatcher
//    {
//        static List<XElement> Changes = new List<XElement>();

//        static DatabaseChangeWatcher()
//        {
//            DatabaseStateChangeCommand.ExecutedChangeCommand += DatabaseStateChangeCommand_ExecutedChangeCommand;
//        }

//        static void DatabaseStateChangeCommand_ExecutedChangeCommand(DatabaseStateChangeCommand change)
//        {
//            var node = new XElement("Change");
//            if (change.CommandType != CommandType.Text)
//                node.Add(new XAttribute("Type", change.CommandType.ToString()));

//            node.Add(new XAttribute("Command", change.CommandText));

//            foreach (var p in change.Params)
//                node.Add(new XElement("Param",
//                    new XAttribute("Name", p.ParameterName),
//                    new XAttribute("Value", p.Value),
//                    new XAttribute("Type", p.DbType)));

//            Changes.Add(node);
//        }

//        internal static void Restart() => Changes.Clear();

//        internal static async Task DispatchChanges()
//        {
//            var response = new XElement("Changes", Changes).ToString();
//            Changes.Clear();
//            await Context.Current.Response().EndWith(response, "text/xml");
//        }

//        internal static void RunChanges()
//        {
//            try
//            {
//                var xmlDocument = new XmlDocument();
//                xmlDocument.LoadXml(Context.Current.Request().Param("Data"));

//                var connectionStringKey = xmlDocument.GetElementsByTagName("ConnectionStringKey")[0].FirstChild.Value;
//                var dataProviderType = xmlDocument.GetElementsByTagName("DataProviderType")[0].FirstChild.Value;
//                var changesNodeList = xmlDocument.GetElementsByTagName("Changes")[0];

//                foreach (XmlElement xmlElement in changesNodeList.ChildNodes)
//                {
//                    var command = xmlElement.GetAttribute("Command").Replace("&#xD;&#xA;", Environment.NewLine);
//                    var dataParameters = new List<SqlParameter>();

//                    foreach (XmlElement innerXmlElement in xmlElement.ChildNodes)
//                    {
//                        var value = innerXmlElement.GetAttribute("Value");
//                        var sqlDbType = innerXmlElement.GetAttribute("Type").To<DbType>();

//                        var sqlParameter = new SqlParameter
//                        {
//                            DbType = sqlDbType,
//                            Value = value.IsEmpty() ? (object)DBNull.Value : value,
//                            ParameterName = innerXmlElement.GetAttribute("Name"),
//                        };

//                        switch (sqlDbType)
//                        {
//                            case DbType.DateTime:
//                                sqlParameter.DbType = DbType.DateTime;
//                                sqlParameter.SqlDbType = SqlDbType.DateTime;
//                                sqlParameter.Value = sqlParameter.Value?.ToString().To<DateTime>();
//                                break;
//                            case DbType.Guid:
//                                sqlParameter.DbType = DbType.Guid;
//                                sqlParameter.SqlDbType = SqlDbType.UniqueIdentifier;
//                                sqlParameter.Value = sqlParameter.Value?.ToString().To<Guid>();
//                                break;
//                            case DbType.DateTime2:
//                                sqlParameter.DbType = DbType.DateTime2;
//                                sqlParameter.SqlDbType = SqlDbType.DateTime2;
//                                sqlParameter.Value = sqlParameter.Value?.ToString().To<DateTime>();
//                                break;
//                        }

//                        dataParameters.Add(sqlParameter);
//                    }

//                    var dataAccessor = GetDataAccessor(dataProviderType, connectionStringKey);
//                    dataAccessor.ExecuteNonQuery(command, commandType, dataParameters.ToArray());

//                    throw new NotImplementedException("In the XML, save the data provider type and connection string key, so it can be executed here.");
//                    // DataAccessor.ExecuteNonQuery(command, CommandType.Text, dataParameters.ToArray());
//                }

//                Cache.Current.ClearAll();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }

//        static IDataAccess GetDataAccessor(string dataProviderType, string connectionStringKey)
//        {
//            switch (dataProviderType)
//            {
//                case "System.Data.SqlClient":
//                    new DatabaseContext(Config.GetConnectionString(connectionStringKey));
//                    var sqlDataProvider = new SqlDataAccessor();
//                    return sqlDataProvider;

//                    //TODO: other data provider types
//            }

//            throw new Exception("Provider type not found");
//        }
//    }
// }