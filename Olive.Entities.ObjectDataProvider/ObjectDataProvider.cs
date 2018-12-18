using Olive.Entities.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Entities.ObjectDataProvider
{
    public abstract partial class ObjectDataProvider : IDataProvider
    {
        static IDatabase Database => Context.Current.Database();

        readonly static string[] ExtractIdsSeparator = new[] { "</Id>", "<Id>", "," };

        ConcurrentDictionary<EntityType, string> Aliases = new ConcurrentDictionary<EntityType, string>();
        EntityType ObjectType, RootType;
        public Type RunTimeType;
        internal List<QueryColumn> FullHierarchyFields;
        EntityType[] AllTypesInMyHierarchy;
        bool ParticipatesInHierarchy, NeedsTransactionForSave;
        List<EntityType> TopToBottomHierarchy;
        public IDataAccess Db;
        Dictionary<string, string> InsertCommand = new Dictionary<string, string>();
        Dictionary<string, string> UpdateCommand = new Dictionary<string, string>();
        string DeleteCommand = string.Empty;
        string FieldsString = string.Empty;
        public string TableString = string.Empty;
        IEnumerable<Association> ManyToManyAssociations;
        IEnumerable<Tuple<string, string, string>> PropertyToColumnMappings;
        List<Association> SubqueryAssociations;
        IEnumerable<string> FullHierarchyFieldIdentifier;
        int SoftDeletePosition;
        Dictionary<string, string> MapSubQueryCache = new Dictionary<string, string>();
        Dictionary<string, string> MapColumnCache = new Dictionary<string, string>();
        static Dictionary<PropertyInfo, Association> GetAssosciationObjectCache = new Dictionary<PropertyInfo, Association>();
        SqlDialect Sql;
        protected ICache Cache;

        protected ObjectDataProvider(Type runtimeType, IDataAccess dataAccess, ICache cache, SqlDialect sqlDialect)
        {
            RunTimeType = runtimeType;
            var meta = new MetaData(runtimeType, sqlDialect);

            Cache = cache;

            ObjectType = meta.ExtractedEntityType;
            RootType = meta.RootType;
            AllTypesInMyHierarchy = meta.AllTypesInMyHierarchy;
            FullHierarchyFields = meta.FullHierarchyFields;
            ParticipatesInHierarchy = meta.ParticipatesInHierarchy; ;
            TopToBottomHierarchy = meta.TopToBottomHierarchy;
            SubqueryAssociations = meta.GetSubqueryAssociations();
            FieldsString = meta.GetFieldsString();
            TableString = meta.GenerateFromTablesExpression();
            DeleteCommand = meta.GetDeleteCommand();
            InsertCommand = meta.GetInsertCommands();
            UpdateCommand = meta.GetUpdateCommands();
            NeedsTransactionForSave = meta.NeedsTransactionForSave();
            ManyToManyAssociations = meta.ManyToManyAssociations;
            PropertyToColumnMappings = meta.GetPropertyToColumnMappings();
            SoftDeletePosition = ObjectType.GetSoftDeletePosition();

            FullHierarchyFieldIdentifier = FullHierarchyFields.Select(c => c.GetCode(c.Identifier));
            Db = dataAccess;
            Sql = sqlDialect;
        }

        public IDataParameter[] GenerateParameters(Dictionary<string, object> parametersData) =>
            parametersData.Select(x => Db.CreateParameter(x.Key, x.Value)).ToArray();

        public abstract string GenerateSelectCommand(IDatabaseQuery iquery, string fields);

        Type IDataProvider.EntityType { get => RunTimeType; }

        string IDataProvider.ConnectionString { get => DataAccess.GetCurrentConnectionString(); set { } }

        string IDataProvider.ConnectionStringKey { get => DataAccess.GetCurrentConnectionString(); set { } }

        IDataAccess IDataProvider.Access => Db;

        public abstract string GenerateWhere(DatabaseQuery query);

        public virtual string GenerateSort(DatabaseQuery query)
        {
            var parts = new List<string>();

            parts.AddRange(query.OrderByParts.Select(p => query.Column(p.Property) + " DESC".OnlyWhen(p.Descending)));

            var offset = string.Empty;
            if (query.PageSize > 0)
                offset = $" OFFSET {query.PageStartIndex} ROWS FETCH NEXT {query.PageSize} ROWS ONLY";

            return parts.ToString(", ") + offset;
        }

        public async Task<IEntity> Get(object objectID)
        {
            var command = $"SELECT {FieldsString} FROM {TableString} WHERE {MapColumn("ID")} = @ID";
            using (var reader = await Db.ExecuteReader(command, CommandType.Text, CreateParameter("ID", objectID, DbType.Int32)).ConfigureAwait(false))
            {
                var result = new List<IEntity>();

                if (reader.Read()) return Parse(reader);
                else throw new DataException($"There is no record with the the ID of '{objectID}'.");
            }
        }

        public virtual DirectDatabaseCriterion GetAssociationInclusionCriteria(IDatabaseQuery masterQuery, PropertyInfo association)
        {
            var whereClause = GenerateAssociationLoadingCriteria((DatabaseQuery)masterQuery, association);

            return new DirectDatabaseCriterion(whereClause)
            {
                Parameters = masterQuery.Parameters
            };
        }

        string GenerateAssociationLoadingCriteria(DatabaseQuery masterQuery, PropertyInfo association)
        {
            if (masterQuery.PageSize.HasValue && masterQuery.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var uniqueItems = GenerateSelectCommand(masterQuery, masterQuery.Column(association.Name));

            return GenerateAssociationLoadingCriteria(MapColumn("ID"), uniqueItems, association);
        }

        public virtual string GenerateAssociationLoadingCriteria(string id, string uniqueItems, PropertyInfo association)
        {
            return $"{id} IN ({uniqueItems})";
        }

        public async Task<int> Count(IDatabaseQuery query)
        {
            var command = GenerateCountCommand(query);
            return Convert.ToInt32(await Db.ExecuteScalar(command, CommandType.Text, GenerateParameters(query.Parameters)).ConfigureAwait(false));
        }

        internal string GenerateCountCommand(IDatabaseQuery iquery)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue)
                throw new ArgumentException("PageSize cannot be used for Count().");

            if (query.TakeTop.HasValue)
                throw new ArgumentException("TakeTop cannot be used for Count().");

            return $"SELECT Count(*) FROM {TableString} {GenerateWhere(query)}";
        }

        public Task<object> Aggregate(IDatabaseQuery query, AggregateFunction function, string propertyName)
        {
            var command = GenerateAggregateQuery(query, function, propertyName);
            return Db.ExecuteScalar(command, CommandType.Text, GenerateParameters(query.Parameters));
        }

        internal string GenerateAggregateQuery(IDatabaseQuery query, AggregateFunction function, string propertyName)
        {
            var sqlFunction = function.ToString();

            var columnValueExpression = MapColumn(propertyName);

            if (function == AggregateFunction.Average)
            {
                sqlFunction = "AVG";

                var propertyType = query.EntityType.GetProperty(propertyName).PropertyType;

                if (propertyType == typeof(int) || propertyType == typeof(int?))
                    columnValueExpression = $"CAST({columnValueExpression} AS decimal)";
            }

            return $"SELECT {sqlFunction}({columnValueExpression}) FROM {TableString}" +
                GenerateWhere((DatabaseQuery)query);
        }

        public IDictionary<string, Tuple<string, string>> GetUpdatedValues(IEntity original, IEntity updated)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var result = new Dictionary<string, Tuple<string, string>>();

            var type = original.GetType();
            var propertyNames = type.GetProperties().Distinct().Select(p => p.Name.Trim()).ToArray();

            PropertyInfo getProperty(string name)
            {
                return type.GetProperties()
                    .Except(p => p.IsSpecialName || p.GetGetMethod().IsStatic)
                    .Where(p => p.GetSetMethod() != null && p.GetGetMethod().IsPublic)
                    .OrderByDescending(x => x.DeclaringType == type)
                    .FirstOrDefault(p => p.Name == name);
            }

            var dataProperties = propertyNames.Select(getProperty).ExceptNull()
                .Except(x => CalculatedAttribute.IsCalculated(x))
                .Where(x => LogEventsAttribute.ShouldLog(x))
                .ToArray();

            foreach (var p in dataProperties)
            {
                var propertyType = p.PropertyType;
                string originalValue, updatedValue = null;
                if (propertyType == typeof(IList<Guid>))
                {
                    try
                    {
                        originalValue = (p.GetValue(original) as IList<Guid>).ToString(",");
                        if (updated != null)
                            updatedValue = (p.GetValue(updated) as IList<Guid>).ToString(",");
                    }
                    catch
                    {
                        throw;
                    }
                }
                else if (propertyType.IsGenericType)
                {
                    try
                    {
                        originalValue = (p.GetValue(original) as IEnumerable<object>).ToString(", ");
                        if (updated != null)
                            updatedValue = (p.GetValue(updated) as IEnumerable<object>).ToString(", ");
                    }
                    catch
                    {
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        originalValue = $"{p.GetValue(original)}";
                        if (updated != null)
                            updatedValue = $"{p.GetValue(updated)}";
                    }
                    catch
                    {
                        throw;
                    }
                }

                if (updated == null || originalValue != updatedValue)
                    if (result.LacksKey(p.Name))
                        result.Add(p.Name, new Tuple<string, string>(originalValue, updatedValue));
            }

            return result;
        }

        public Task<int> ExecuteNonQuery(string command) => Db.ExecuteNonQuery(command);

        public Task<object> ExecuteScalar(string command) => Db.ExecuteScalar(command);

        public bool SupportValidationBypassing() => true;

        dynamic GetCastedValueExpression(Property foundedProperty, object readerValue)
        {
            if (Sql == SqlDialect.MySQL && foundedProperty is NumberProperty num && num.IsAutoNumber)
                return Convert.ToInt32(readerValue);

            var propertyBaseType = foundedProperty.PropertyType.TrimEnd("?");

            if (propertyBaseType == "string")
                return readerValue as string;

            if (propertyBaseType == "double")
                return (double)(decimal)readerValue;

            return ConvertTo(readerValue, Type.GetType(propertyBaseType));
        }

        static string GetOfficialTypeName(string type)
        {
            switch (type)
            {
                case "int": return "Int32";
                case "long": return "Int64";
                case "bool": return "Boolean";
                default:
                    return type[0].ToUpper() + type.Substring(1);
            }
        }

        bool NeedsConvert(Property foundedProperty)
        {
            if (foundedProperty.ShouldConvertDatabaseValue) return true;
            if (Sql == SqlDialect.SQLite)
                if (foundedProperty is BooleanProperty || foundedProperty is NumberProperty) return true;

            return false;
        }

        internal static object ConvertTo(dynamic source, Type dest)
        {
            if (source == null) return null;

            if (dest.IsGenericType && dest.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                dest = Nullable.GetUnderlyingType(dest);

            return Convert.ChangeType(source, dest);
        }

        internal static List<string> ExtractIds(string idsXml) =>
                    idsXml.Split(ExtractIdsSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();

        internal string GenerateSubQueryExpression(Association query, Dictionary<string, string> targetList)
        {
            var list = targetList;
            var preFix = query.EntityType.TableName + "_" + query.Name + "_";
            var mainTable = $"{{target{query.ReferencedType.TableName}}}";
            var targetId = query.ReferencedType.IdColumnName;

            var queryMetaData = new MetaData(query.ReferencedType);
            var tableExpression = queryMetaData.GenerateFromTablesExpression(t => "{target" + t.TableName + "}");

            var r = new StringBuilder();
            r.AppendLine($"SELECT {mainTable}.{targetId}");
            r.AppendLine($"FROM {tableExpression}");
            r.AppendLine($"WHERE {mainTable}.{targetId} = {"{parent}" }.{query.DatabaseColumnName}");

            var result = r.ToString();

            var softDelete = query.ReferencedType.WithAllParents.FirstOrDefault(t => t.SoftDelete);
            if (softDelete != null)
            {
                var softDeleteSubAlias = preFix + GetSelectAlias(softDelete);

                result = $"@\"{result}\" +  $\" AND {mainTable}.{".DELETED"} = 0\".Unless(SoftDeleteAttribute.Context.ShouldByPassSoftDelete())";
            }
            else
                if (!SoftDeleteAttribute.Context.ShouldByPassSoftDelete())
                result = result.Trim();

            return result;
        }

        static string GetPropertySetter(EntityType type, string field)
        {
            foreach (var property in type.Properties.Where(i => i.HasDatabaseRepresentation).OrderBy(i => i.Order))
            {
                if (property.DatabaseColumnName != field) continue;

                if (property is Association association)
                {
                    if (!association.IsManyToMany)
                        return association.Name + "Id";
                }
                else if (property.PropertyType == "Blob")
                {
                    return property.Name + ".FileName";
                }
                else
                {
                    return property.Name;
                }
            }

            throw new NotSupportedException("The {0} field is not found in Type {1}.".FormatWith(field, type.Name));
        }

        string GetSelectAlias(EntityType type)//TODO: Check duplicate code
        {
            return Aliases.GetOrAdd(type, t =>
            {
                if (t == ObjectType && AllTypesInMyHierarchy.IsSingle())
                    return ObjectType.TableName.First().ToString();

                if (AllTypesInMyHierarchy.Select(x => x.TableName.First()).Distinct().Count() == AllTypesInMyHierarchy.Length)
                {
                    return t.TableName.First().ToString();
                }

                return "root_" + t.TableName;
            });
        }
    }
}