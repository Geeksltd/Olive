using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Olive.Entities.ObjectDataProvider
{
    class MetaData
    {
        public EntityType ExtractedEntityType; // Meta-data
        public ConcurrentDictionary<EntityType, string> Aliases = new ConcurrentDictionary<EntityType, string>();
        public EntityType RootType;
        public List<QueryColumn> FullHierarchyFields;
        public EntityType[] AllTypesInMyHierarchy;
        public bool ParticipatesInHierarchy;
        public List<EntityType> TopToBottomHierarchy;
        public IEnumerable<Association> ManyToManyAssociations;
        public SqlDialect Sql;

        public MetaData(Type runtimeType, SqlDialect sqlDialect)
        {
            ExtractedEntityType = ExtractMetadata(runtimeType, tryToFindParent: true, tryToFindProperties: true);
            ExtractedEntityType.WithAllParents = WithAllParentGenerator(ExtractedEntityType);
            ExtractedEntityType.AllChildren = GetAllChildren(runtimeType, findMoreChildren: true);
            ExtractedEntityType.Derivatives = GetAllChildren(runtimeType, findMoreChildren: false);
            ExtractedEntityType.DirectDatabaseFields = ExtractDatabaseFields(ExtractedEntityType.Properties);
            UpdateDatabaseRepresentationFlag();
            Sql = sqlDialect;
            SetHierachyVariable();
        }

        internal MetaData(EntityType entityType)
        {
            ExtractedEntityType = entityType;
            SetHierachyVariable();
        }

        void SetHierachyVariable()
        {
            TopToBottomHierarchy = GetTopToBottomHierarchy();
            RootType = TopToBottomHierarchy.FirstOrDefault();
            AllTypesInMyHierarchy = FindAllTypesInMyHierarchy();
            FullHierarchyFields = FindFullHierarchyFields();
            ParticipatesInHierarchy = ParticipatesInDatabaseHierarchy(ExtractedEntityType);

            ManyToManyAssociations = GetManyToManyAssociations();
        }

        public List<Association> GetSubqueryAssociations()
        {
            return TopToBottomHierarchy
                 .SelectMany(x => x.DirectDatabaseFields)
                 .OfType<Association>()
                 .Where(a => a.IsManyToOne && a.ReferencedType.HasKnownDatabase)
                 .ToList();
        }

        public static EntityType ExtractMetadata(Type runtimeType, bool tryToFindParent = false, bool tryToFindProperties = true)
        {
            if (runtimeType.TypeIsNotCorrect()) return null;

            var propertyList = new List<Property>();
            var dbFieldsList = new List<Property>();

            var allPropertiesList = Enumerable.Empty<Property>();
            var assignIdByDatabase = false;

            if (tryToFindProperties)
            {
                propertyList = GetPropertiesOfEntityObject(runtimeType);
                var parentProperties = GetPropertiesOfEntityObject(runtimeType.GetTypeInfo().BaseType);
                var parentWithoutDuplicate = parentProperties.Where(p => propertyList.None(p2 => p2.Name == p.Name));
                allPropertiesList = propertyList.Concat(parentWithoutDuplicate);
            }

            var allParents = tryToFindParent ? GetAllAncestors(runtimeType) : null;

            var primaryKeyProperty = "";
            var idColumnName = "id";
            var hasCustomPrimaryKeyColumn = false;

            var primaryKeyProp = runtimeType.GetProperties().FirstOrDefault(x => Attribute.IsDefined(x, typeof(PrimaryKeyAttribute)) && x.Name.ToLower() != "id");
            if (primaryKeyProp != null)
            {
                primaryKeyProperty = primaryKeyProp.PropertyType.ToString();
                idColumnName = primaryKeyProp.Name;
                hasCustomPrimaryKeyColumn = true;
            }
            else
                if (runtimeType.GetProperties().Any(x => x.Name.ToLower() == "id"))
                primaryKeyProperty = runtimeType.GetProperties().First(x => x.Name.ToLower() == "id").PropertyType.ToString();

            return new EntityType
            {
                Type = runtimeType,
                AssemblyFullName = runtimeType.AssemblyQualifiedName,
                ClassFullName = runtimeType.FullName,
                ClassName = runtimeType.Name,
                TableName = TableNameAttribute.GetTableName(runtimeType),
                Name = runtimeType.Name.ToPlural(),
                AllProperties = allPropertiesList.ToArray(),
                DirectDatabaseFields = propertyList.ToArray(),
                Properties = propertyList.ToArray(),
                IdColumnName = idColumnName,
                PrimaryKeyType = primaryKeyProperty,
                PluralName = runtimeType.Name.ToPlural(),
                AllParents = runtimeType.BaseType != null ? allParents : null,
                WithAllParents = allParents,

                IsTransient = Attribute.IsDefined(runtimeType, typeof(TransientEntityAttribute)),
                IsAbstract = runtimeType.IsAbstract,
                SoftDelete = Attribute.IsDefined(runtimeType, typeof(SoftDeleteAttribute)),

                AssignIDByDatabase = assignIdByDatabase,
                HasCustomPrimaryKeyColumn = hasCustomPrimaryKeyColumn,
                HasDataAccessClass = true,
                //HasKnownDatabase = Attribute.IsDefined(runtimeType, typeof(HasKnownDatabaseAttribute)),
                BaseType = GetSmallEntityType(runtimeType.BaseType),
            };
        }

        static List<Property> GetPropertiesOfEntityObject(Type runtimeType)
        {
            var propertyList = new List<Property>();
            if (runtimeType.TypeIsNotCorrect()) return propertyList;

            var myTypeProperties = runtimeType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);

            foreach (var myProperty in myTypeProperties)
            {
                var currentProperty = CreatePropertyObject(myProperty, myTypeProperties.IndexOf(myProperty)); ;
                propertyList.Add(currentProperty);
            }

            return propertyList;
        }

        static Property CreatePropertyObject(PropertyInfo myProperty, int orderIndexOfPropertyItem)
        {
            var isTrueType = true;
            var isFalseType = false;
            var result = new Property();

            // var mydbType = FindType(Type.GetType(myProperty.PropertyType.ToString()));
            var mydbType = Type.GetType(myProperty.PropertyType.ToString()).ToDbType();

            var databaseColumnName = myProperty.Name;
            var databaseColumnType = Convert.ToString(mydbType); ;

            var myPropertyType = myProperty.PropertyType;
            if (myPropertyType == typeof(DateTime?))
            {
                result = new DateTimeProperty
                {
                    HasTime = !Attribute.IsDefined(myProperty, typeof(DateOnlyAttribute)),
                    HasDate = true,
                    TimeOnly = myProperty.PropertyType == typeof(TimeSpan) ? isTrueType : isFalseType,
                };
            }
            else if (Type.GetType(myProperty.PropertyType.ToString()).IsNumericType())
            {
                result = new NumberProperty
                {
                    IsAutoNumber = Attribute.IsDefined(myProperty, typeof(AutoNumberAttribute)),
                    SpecifiedPropertyType = myProperty.PropertyType.ToString(),
                };
            }
            else if (myPropertyType == typeof(string))
            {
                result = new StringProperty
                {
                    IsRowVersion = myProperty.PropertyType == typeof(byte) ? isTrueType : isFalseType,
                };
            }
            else if (Attribute.IsDefined(myProperty, typeof(CalculatedAttribute)) == false
                        && myProperty.IsAccociation())
            {
                result = ObjectDataProvider.GetAssosciationObject(myProperty);
            }
            else if (myPropertyType == typeof(Blob))
            {
                result = new BinaryProperty();
                databaseColumnName = myProperty.Name + "_FileName";
                databaseColumnType = DbType.String.ToString();
            }

            result.EntityType = GetSmallEntityType(myProperty.ReflectedType);
            result.Initialize(myProperty, databaseColumnName, databaseColumnType, mydbType, orderIndexOfPropertyItem);

            return result;
        }

        static EntityType[] GetAllAncestors(Type runtimeType)
        {
            var parentItems = new List<EntityType>();
            if (runtimeType.TypeIsNotCorrect())
                return parentItems.ToArray();
            var output = ExtractMetadata(runtimeType.GetTypeInfo().BaseType, tryToFindParent: true);

            if (output != null)
            {
                parentItems.AddRange(output.AllParents);
                parentItems.Add(output);
            }

            return parentItems.ToArray();
        }

        static EntityType GetSmallEntityType(Type runtimeType)
        {
            if (runtimeType.TypeIsNotCorrect()) return null;

            return new EntityType
            {
                Type = runtimeType,
                AssemblyFullName = runtimeType.AssemblyQualifiedName,
                ClassFullName = runtimeType.FullName,
                ClassName = runtimeType.Name,
                TableName = TableNameAttribute.GetTableName(runtimeType),
                Name = runtimeType.Name.ToPlural(),

                PluralName = runtimeType.Name.ToPlural(),
                IsTransient = Attribute.IsDefined(runtimeType, typeof(TransientEntityAttribute)),
                IsAbstract = runtimeType.GetTypeInfo().IsAbstract,
                SoftDelete = Attribute.IsDefined(runtimeType, typeof(SoftDeleteAttribute)),

                HasDataAccessClass = Attribute.IsDefined(runtimeType, typeof(DatabaseGeneratedAttribute)),
                //HasKnownDatabase = Attribute.IsDefined(runtimeType, typeof(HasKnownDatabaseAttribute)),

                BaseType = GetSmallEntityType(runtimeType.BaseType),

            };
        }

        static EntityType[] WithAllParentGenerator(EntityType type)
        {
            var result = new List<EntityType> { type };

            foreach (var item in type.AllParents)
                if (item.Type.IsA<IEntity>())
                    result.Add(item);

            return result.ToArray();
        }

        static EntityType[] GetAllChildren(Type runtimeType, bool findMoreChildren)
        {
            var result = new List<EntityType>();
            if (runtimeType.TypeIsNotCorrect())
                return result.ToArray();
            var childTypes = Assembly.GetAssembly(runtimeType).GetTypes().Where(t => t.IsA(runtimeType) && t.Name != runtimeType.Name);

            foreach (var type in childTypes)
                result.Add(ExtractMetadata(type, findMoreChildren));

            return result.ToArray();
        }

        Property[] ExtractDatabaseFields(Property[] properties)
        {
            var result = new List<Property>();
            result.AddRange(properties);
            foreach (var item in properties)
            {
                if (item is Association)
                    result.RemoveAll(x => x.Name == item.Name + "Id");
                else if (item.IsCalculated)
                    result.RemoveAll(x => x.Name == item.Name);
                else if (item.IsPrimaryKey && item.Name.ToLower() != "id")
                    result.RemoveAll(x => x.Name.ToLower() == "id");
            }

            return result.ToArray();
        }

        void UpdateDatabaseRepresentationFlag()
        {
            var difference = ExtractedEntityType.Properties.Except<Property>(ExtractedEntityType.DirectDatabaseFields);
            foreach (var item in difference)
                Array.Find<Property>(ExtractedEntityType.Properties, x => x.Name == item.Name).HasDatabaseRepresentation = false;

            if (ExtractedEntityType.DirectDatabaseFields.Any(x => x.Name.ToUpper() == "ID"))
                ExtractedEntityType.AssignIDByDatabase = true;
        }

        List<EntityType> GetTopToBottomHierarchy() => ExtractedEntityType.WithAllParents.OrderBy(x => x.AllParents.Count()).ToList();

        EntityType[] FindAllTypesInMyHierarchy()
        {
            if (ExtractedEntityType.AllChildren != null)
                return ExtractedEntityType.WithAllParents.Concat(ExtractedEntityType.AllChildren)
                .Where(x => x.HasDataAccessClass).ToArray();
            else
                return ExtractedEntityType.WithAllParents
            .Where(x => x.HasDataAccessClass).ToArray();
        }

        List<QueryColumn> FindFullHierarchyFields()
        {
            var result = new List<QueryColumn>();

            foreach (var table in AllTypesInMyHierarchy)
            {
                result.Add(new QueryColumn
                {
                    Identifier = table.TableName + "_Id",
                    SqlExpression = GetSelectAlias(table) + "." + table.IdColumnName,
                    Type = table.Type
                });

                if (table.SoftDelete)
                    result.Add(new QueryColumn
                    {
                        Identifier = table.TableName + "__SoftDeleted",
                        SqlExpression = GetSelectAlias(table) + "." + "[.Deleted]"
                    });

                foreach (var p in table.DirectDatabaseFields)
                {
                    if (p.IsCalculated) break;
                    var value = $"{GetSelectAlias(table)}.[{p.DatabaseColumnName}]";

                    if (p is Association association && association.IsManyToMany)
                    {
                        var r = new StringBuilder();
                        r.Append("(SELECT ");
                        if (Sql == SqlDialect.MySQL) r.Append("group_concat(");

                        r.Append(association.BridgeTableName);
                        r.Append(".");
                        r.Append(GetOuterColumn(association));
                        if (Sql == SqlDialect.MySQL) r.Append(")");

                        r.AppendFormat(
                         " AS Id FROM {0} WHERE {1}.{2} = {3}.{4}",
                            association.BridgeTableName,
                            association.BridgeTableName,
                            GetInnerColumn(association),
                            GetSelectAlias(table),
                            table.IdColumnName);

                        if (Sql == SqlDialect.MSSQL) r.Append(" FOR XML PATH('')");

                        r.Append(")");

                        value = r.ToString();
                    }

                    result.Add(new QueryColumn
                    {
                        SqlExpression = value,
                        Property = p,
                        Identifier = $"{p.ColumnIdentifier}"
                    });
                }
            }

            return result;
        }

        static bool ParticipatesInDatabaseHierarchy(EntityType type)
        {
            return (type.BaseType != null && type.GetType().IsA<IEntity>()) || (type.Derivatives != null && type.Derivatives.Any(x => !x.IsTransient));
        }

        string GetSelectAlias(EntityType type)
        {
            return Aliases.GetOrAdd(type, t =>
            {
                if (t == ExtractedEntityType && AllTypesInMyHierarchy.IsSingle())
                    return ExtractedEntityType.TableName.First().ToString();

                if (AllTypesInMyHierarchy.Select(x => x.TableName.First()).Distinct().Count() == AllTypesInMyHierarchy.Length)
                {
                    return t.TableName.First().ToString();
                }

                return "root_" + t.TableName;
            });
        }

        internal bool NeedsTransactionForSave()
        {
            if (ExtractedEntityType.BaseType != null) return true;
            if (ManyToManyAssociations.Any()) return true;

            return false;
        }

        internal IEnumerable<Association> GetManyToManyAssociations()
        {
            return ExtractedEntityType.AllProperties.OfType<Association>().Where(a => a.IsManyToMany && !a.IsCalculated).ToList();
        }

        internal static string GetInnerColumn(Association manyToManyAssociation)
        {
            return manyToManyAssociation.InverseAssociation == null ? manyToManyAssociation.BridgeColumn1 : manyToManyAssociation.BridgeColumn2;
        }

        internal static string GetOuterColumn(Association manyToManyAssociation)
        {
            return manyToManyAssociation.InverseAssociation == null ? manyToManyAssociation.BridgeColumn2 : manyToManyAssociation.BridgeColumn1;
        }

        internal string GenerateFromTablesExpression() => GenerateFromTablesExpression(GetSelectAlias);

        internal string GenerateFromTablesExpression(Func<EntityType, string> getAlias)
        {
            var r = new StringBuilder();

            if (RootType.GetType().IsA<IEntity>())
                return "";

            r.Append(GetTableIdentifier(RootType, alias: getAlias(RootType)));

            var rootTable = getAlias(RootType);
            var rootId = RootType.IdColumnName;

            foreach (var child in AllTypesInMyHierarchy)
            {
                if (child == RootType) continue;

                var table = GetTableIdentifier(child, alias: getAlias(child));

                r.Append($" LEFT OUTER JOIN {table} ON {getAlias(child)}.Id = {rootTable}.{rootId} ");
            }

            return r.ToString();
        }

        internal string GetFieldsString()
        {
            var separator = ", ";
            return FullHierarchyFields.Select(x => x.GetCode($"{x.SqlExpression} AS [{x.Identifier}]")).ToString(separator);
        }

        internal static string GetTableIdentifier(EntityType type, string alias = null)
        {
            return type.TableName + alias.WithPrefix(" AS ");
        }

        internal string GetDeleteCommand()
        {
            return $"DELETE FROM {GetTableIdentifier(RootType)} WHERE {RootType.IdColumnName} = @Id";
        }

        internal Dictionary<string, string> GetInsertCommands()
        {
            var insertCommand = new Dictionary<string, string>();
            if (ExtractedEntityType.BaseType == null)
                insertCommand.Add(ExtractedEntityType.TableName, GetInsertCommandText(ExtractedEntityType, Sql));
            else
                foreach (var type in TopToBottomHierarchy)
                    insertCommand.Add(type.TableName, GetInsertCommandText(type, Sql));
            return insertCommand;
        }

        internal Dictionary<string, string> GetUpdateCommands()
        {
            var insertCommand = new Dictionary<string, string>();
            if (ExtractedEntityType.BaseType == null)
                insertCommand.Add(ExtractedEntityType.TableName, GetUpdateCommandText(ExtractedEntityType, Sql));
            else
                foreach (var type in TopToBottomHierarchy)
                    insertCommand.Add(type.TableName, GetUpdateCommandText(type, Sql));
            return insertCommand;
        }

        internal IEnumerable<Tuple<string, string, string>> GetPropertyToColumnMappings()
        {
            yield return Tuple.Create("ID", GetSelectAlias(ExtractedEntityType), ExtractedEntityType.IdColumnName);

            foreach (var table in TopToBottomHierarchy)
            {
                foreach (var column in table.DirectDatabaseFields)
                {
                    var association = column as Association;
                    if (association != null)
                    {
                        if (!association.IsManyToOne) continue;
                        if (association.ReferencedType.IsTransient) continue;
                    }

                    yield return Tuple.Create(column.Name, GetSelectAlias(table), column.DatabaseColumnName);

                    if (association != null && association.ReferencedType.PrimaryKeyType != "Guid")
                        yield return Tuple.Create(column.Name + "Id", GetSelectAlias(table), column.DatabaseColumnName);
                }

                if (table.SoftDelete)
                    yield return Tuple.Create("IsMarkedSoftDeleted", GetSelectAlias(table), "[.DELETED]");
            }
        }

        static internal string GetInsertCommandText(EntityType type, SqlDialect sql)
        {
            var sqlFields = type.GetSaveSqlFields();

            var sqlParameters = sqlFields.Select(i => GetParameterName(i)).ToString(", ");

            var r = new StringBuilder();

            r.Append("INSERT INTO " + GetTableIdentifier(type));

            var saveFields = type.GetSaveSqlFields().Select(i => i).ToString(" ,  ");

            if (saveFields.HasValue())
            {
                r.Append($" ({saveFields}) ");
            }

            var outputValue = type.Properties.OfType<StringProperty>().FirstOrDefault(x => x.IsRowVersion);
            var outputFields = new[] { type.FindAutoNumber(), outputValue }.ExceptNull();

            if (sql == SqlDialect.MSSQL)
            {
                r.Append(outputFields.Select(c => $" [INSERTED].{c.DatabaseColumnName} ")
                    .ToString(", ").WithWrappers("  OUTPUT  ", " "));
            }

            if (saveFields.HasValue())
            {
                r.Append(" VALUES ");
                r.Append($" ({sqlParameters}) ");
            }

            if (sql != SqlDialect.MSSQL && type.FindAutoNumber() != null)
            {
                r.Append(" ; ");
                if (outputFields.Any())
                    r.Append(" SELECT LAST_INSERT_ID(); ");
            }

            return r.ToString().Trim().KeepReplacing("  ", " ");
        }

        static internal string GetUpdateCommandText(EntityType type, SqlDialect sql)
        {
            var fullTableName = GetTableIdentifier(type);
            var sqlFields = type.GetSaveSqlFields();

            // if (type.Project.ConstantIDs)
            //    sqlFields.Remove("ID");

            var sqlParameters = sqlFields.Select(i => i + " = " + GetParameterName(i)).ToString(", ");

            var pk = "ID";
            if (type.HasCustomPrimaryKeyColumn)
                pk = type.Properties.Single(x => x.IsPrimaryKey).DatabaseColumnName;

            var r = new StringBuilder();

            if (sql != SqlDialect.MSSQL)
                r.AppendLine($"SELECT {pk} FROM {fullTableName} WHERE {pk} = @OriginalId;");

            r.Append($" UPDATE {fullTableName} SET ");
            r.Append(sqlParameters);

            if (sql == SqlDialect.MSSQL)
            {
                r.Append(" OUTPUT ");
                var versoinRowProp = type.Properties.OfType<StringProperty>().FirstOrDefault(x => x.IsRowVersion);
                var versin = "";
                if (versoinRowProp != null)
                    versin = $"{"INSERTED"}.{versoinRowProp.DatabaseColumnName}";
                r.Append(versin.Or($" {"INSERTED"}.{pk} "));
            }

            r.Append($" WHERE {pk} = @OriginalId ");

            foreach (var v in type.AllProperties.OfType<StringProperty>().Where(x => x.IsRowVersion)
                .Select(x => x.DatabaseColumnName))
                r.Append($" AND {v} = {GetParameterName(v)} ");

            return r.ToString().KeepReplacing("  ", " ");
        }

        static string GetParameterName(string column) => "@" + column.Replace(".DELETED", "_DELETED").Remove("[").Remove("]").Remove(" ");


    }
}