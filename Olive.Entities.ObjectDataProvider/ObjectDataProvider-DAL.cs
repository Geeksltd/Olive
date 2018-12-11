using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Entities.ObjectDataProvider
{
    public abstract partial class ObjectDataProvider : IDataProvider
    {
        internal IEntity Parse(IDataReader reader, IEnumerable<QueryColumn> fieldsinput = null)
        {
            if (ObjectType.IsAbstract)
                throw new Exception($"The record with ID of '{{reader[\"{ObjectType.TableName}_Id\"]}}' exists only in the abstract database table of '{ObjectType.TableName}' and no concrete table. The data needs cleaning-up.");

            var result = Activator.CreateInstance(Type.GetType(ObjectType.AssemblyFullName));

            if (ParticipatesInHierarchy)
            {
                var fieldList = fieldsinput ?? FullHierarchyFields;
                result = FillData(reader, (IEntity)result, fieldList);
            }
            else
                result = FillData(reader, (IEntity)result);


            var saved = false;
            if (reader.GetValue(0) != null) saved = true;

            if (saved)
            {

                if (result.GetType().GetProperty("OriginalId") != null)
                    result.GetType().GetProperty("OriginalId").SetValue(result, GetPrimaryKeyReaderValue(reader, 0));
                if (result.GetType().GetProperty("ID") != null)
                    result.GetType().GetProperty("ID").SetValue(result, GetPrimaryKeyReaderValue(reader, 0));

                result.GetType().GetProperty("IsNew").SetValue(result, !saved);

            }
            else
            {
                Entity.Services.SetSaved((IEntity)result, saved);
            }

            return (IEntity)result;
        }

        dynamic GetPrimaryKeyReaderValue(IDataReader reader, int indexOfCell)
        {
            switch (ObjectType.PrimaryKeyType)
            {
                case "Guid":
                case "System.Guid":
                    return reader.GetGuid(indexOfCell);
                case "int":
                case "System.Int32":
                    return reader.GetInt32(indexOfCell);
                case "long":
                case "System.Int64":
                    return reader.GetInt64(indexOfCell);
                case "string":
                case "System.String":
                    return reader.GetString(indexOfCell);
                default: throw new NotSupportedException();
            }
        }

        public IEntity Parse(IDataReader reader) => Parse(reader, FullHierarchyFields);

        Dictionary<string, object> SetData(Property foundedProperty, object readerValue)
        {
            var result = new Dictionary<string, object>();
            object value = null;
            if (foundedProperty is Association)
            {
                result = SetDataAssociation(foundedProperty, readerValue);
            }
            else if (Sql == SqlDialect.SQLite && (foundedProperty as DateTimeProperty)?.TimeOnly == true)
            {
                result.Add(foundedProperty.Name, readerValue.ToString().TryParseAs<TimeSpan>());
            }
            else if ((foundedProperty as DateTimeProperty)?.HasTime == false)
            {
                if (Sql == SqlDialect.SQLite)
                {
                    if (!foundedProperty.IsMandatory)
                        value = (readerValue as string)?.To<DateTime>().Date;
                    else if (foundedProperty.IsMandatory)
                        value = (readerValue as string).To<DateTime>().Date;
                }
                else
                    value = readerValue == null ? (DateTime?)null : Convert.ToDateTime(readerValue).Date;

                result.Add(foundedProperty.Name, value);
            }
            else if (foundedProperty is BinaryProperty)
            {
                result.Add(foundedProperty.Name, new Blob { FileName = readerValue as string });
            }
            else if (NeedsConvert(foundedProperty))
            {
                var type = GetOfficialTypeName(foundedProperty.PropertyType.TrimEnd("?"));

                if (type == "Guid")
                    value = Convert.ToString(readerValue).To<Guid>();
                else
                    value = ConvertTo(readerValue, Type.GetType(type));

                result.Add(foundedProperty.Name, value);
            }
            else
            {
                result.Add(foundedProperty.Name, GetCastedValueExpression(foundedProperty, readerValue));
            }

            return result;
        }

        Dictionary<string, object> SetDataAssociation(Property foundedProperty, object readerValue)
        {
            var result = new Dictionary<string, object>();
            var association = foundedProperty as Association;
            var idTypeString = association.ReferencedType.PrimaryKeyType;
            var idType = Type.GetType(idTypeString);
            object value = null;
            if (association.IsManyToOne)
            {
                if (Sql == SqlDialect.SQLite && idTypeString != "string")
                    value = readerValue.ToStringOrEmpty().To(idType);
                else
                    value = ConvertTo(readerValue, idType);
                result.Add(foundedProperty.Name + "Id", value);
            }
            else
            {
                if (idTypeString != "string")
                    value = ExtractIds((string)readerValue).Select(id => id.To(idType)).ToList();
                else
                    value = ExtractIds((string)readerValue).ToList();

                result.Add(foundedProperty.Name + "Ids", value);
            }

            return result;
        }

        internal IEntity FillData(IDataReader reader, IEntity entity, IEnumerable<QueryColumn> fields)
        {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);

            var handleSoftDelete = ObjectType.SoftDelete;

            if (handleSoftDelete)
            {
                entity.GetType().GetProperty($"{ObjectType.TableName}__SoftDeleted").SetValue(entity, (bool)values[SoftDeletePosition]);
                entity.GetType().GetProperty("IsMarkedSoftDeleted").SetValue(entity, true);
            }

            return FillDataFields(reader, entity, fields);
        }

        internal IEntity FillData(IDataReader reader, IEntity entity)
        {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);

            var handleSoftDelete = ObjectType.SoftDelete;

            handleSoftDelete = ObjectType.WithAllParents.Any(x => x.SoftDelete);

            var fields = ObjectType.WithAllParents.SelectMany(x => x.DirectDatabaseFields).ToArray();
            if (handleSoftDelete)
                entity = HandleSoftDelete(fields, values, entity);

            var fieldsList = FullHierarchyFields.Where(x => fields.Any(f => f.ColumnIdentifier == x.Identifier));
            return FillDataFields(reader, entity, fieldsList);

        }

        IEntity FillDataFields(IDataReader reader, IEntity entity, IEnumerable<QueryColumn> fields)
        {
            foreach (var item in fields)
            {
                var foundedProperty = ObjectType.Properties.FirstOrDefault(x => x.Name == item.Identifier);
                if (foundedProperty == null) continue;

                var propertyType = typeof(object); ;
                if (foundedProperty is Association)
                    propertyType = typeof(System.Guid);
                else if (foundedProperty is BinaryProperty)
                    //propertyType = typeof(BinaryProperty);
                    propertyType = typeof(string);
                else
                    propertyType = Type.GetType(foundedProperty.PropertyType);

                var method = typeof(NullSafeGetter).GetMethods().Single(m =>
                                                                       m.Name == "GetValueOrDefault" &&
                                                                       m.GetParameters()[1].ParameterType == typeof(string));

                var readerValue = method.MakeGenericMethod(propertyType).Invoke(null, new object[] { reader, item });

                var propertyNameValue = SetData(foundedProperty, readerValue);

                if (foundedProperty.IsNeedsDbNullCheck && readerValue == DBNull.Value)
                {
                    var property = entity.GetType().GetProperty(item.Identifier);
                    property.SetValue(entity, null);
                }
                else
                {
                    var property = entity.GetType().GetProperty(item.Identifier);
                    propertyNameValue.TryGetValue(foundedProperty.Name, out object valForDc);
                    property.SetValue(entity, valForDc);
                }

                if (foundedProperty is Association)
                {
                    var property = entity.GetType().GetProperty(item.Identifier + "Id");
                    propertyNameValue.TryGetValue(foundedProperty.Name + "Id", out object valForDc);
                    property.SetValue(entity, valForDc);
                }
            }
            return entity;
        }
        IEntity HandleSoftDelete(Property[] fields, object[] values, IEntity entity)
        {
            if (ParticipatesInHierarchy)
            {
                var index = Array.FindIndex(fields, (x) => (x.Name == $"{ObjectType.TableName}__SoftDeleted"));
                if ((bool)values[index])
                    SoftDeleteAttribute.MarkDeleted((Entity)entity);
            }
            else
                if ((bool)values[1])
                SoftDeleteAttribute.MarkDeleted((Entity)entity);
            return entity;
        }

        public async Task Save(IEntity record)
        {
            if (ParticipatesInHierarchy)
                if (record.GetType().ToString() != ObjectType.ClassFullName)
                    throw new ArgumentException($"Invalid argument type specified. Expected: '{ObjectType.ClassFullName}', Provided: '{record.GetType()}'");

            if (NeedsTransactionForSave)
            {
                using (var scope = Database.CreateTransactionScope())
                {
                    await SaveInner(record);
                    scope.Complete();
                }
            }
            else
            {
                await SaveInner(record);
            }

        }

        async Task SaveInner(IEntity record)
        {
            if (record.IsNew)
                await InsertAsync(record).ConfigureAwait(false);
            else
                await UpdateAsync(record).ConfigureAwait(false); ;

            if (ManyToManyAssociations.Any())
                await SaveManyToManyRelation(record).ConfigureAwait(false);
        }
        internal void Insert(IEntity record)
        {
            if (InsertCommand.HasMany())
            {
                Action saveAll = () =>
                {
                    Task.Factory.RunSync(() => InsertInner(InsertCommand, record));
                };

                if (Database.AnyOpenTransaction())
                    saveAll();
                else
                {
                    using (var scope = Database.CreateTransactionScope())
                    {
                        saveAll();
                        scope.Complete();
                    }

                }
            }
        }

        async Task InsertAsync(IEntity record)
        {
            if (InsertCommand.HasMany())
            {
                async Task saveAll()
                {
                    await InsertInner(InsertCommand, record).ConfigureAwait(false);
                }

                if (Database.AnyOpenTransaction())
                    await saveAll().ConfigureAwait(false);
                else
                {
                    using (var scope = Database.CreateTransactionScope())
                    {
                        await saveAll().ConfigureAwait(false);
                        scope.Complete();
                    }
                }
            }
            else
                await InsertInner(InsertCommand, record).ConfigureAwait(false);

        }

        async Task InsertInner(Dictionary<string, string> insertCommands, IEntity record)
        {
            foreach (var type in TopToBottomHierarchy)
            {
                var tableName = type.TableName;
                var insertCommandfortable = insertCommands[type.TableName];

                var insertValueResult = await Db.ExecuteScalar(insertCommandfortable, CommandType.Text, CreateParameters(type, record)).ConfigureAwait(false);

                var autoNumber = ObjectType.FindAutoNumber();
                if (autoNumber != null)
                {
                    if (Sql == SqlDialect.MSSQL)
                    {
                        var autoNumberType = Type.GetType(autoNumber.PropertyType);
                        var insertValueResultConverted = ConvertTo(insertValueResult, autoNumberType);
                        record.GetType().GetProperty(autoNumber.Name).SetValue(record, insertValueResultConverted);
                    }
                    else
                        record.GetType().GetProperty(autoNumber.Name).SetValue(record, Convert.ToInt32(insertValueResult));
                }
            }
        }

        async Task UpdateAsync(IEntity record)
        {
            var hierarchy = TopToBottomHierarchy.Where(x => x.GetSaveSqlFields().Any()).ToList();
            var multiTable = hierarchy.HasMany();

            if (!hierarchy.None())
            {
                async Task saveAll()
                {
                    await UpdateInner(UpdateCommand, record, hierarchy).ConfigureAwait(false);
                }

                if (Database.AnyOpenTransaction())
                    await saveAll().ConfigureAwait(false);
                else
                {
                    using (var scope = Database.CreateTransactionScope())
                    {
                        await saveAll().ConfigureAwait(false);
                        scope.Complete();
                    }
                }
            }
        }

        async Task UpdateInner(Dictionary<string, string> updateCommands, IEntity record, List<EntityType> hierarchy)
        {
            var saveCode = new StringBuilder();

            foreach (var type in hierarchy)
            {
                var tableName = type.TableName;
                var updateCommandfortable = updateCommands[type.TableName];

                var updateValueResult = await Db.ExecuteScalar(updateCommandfortable, CommandType.Text, CreateParameters(type, record)).ConfigureAwait(false);

                var errorText = $"Failed to update the '{tableName}' table. There is no row with the ID ";

                var version = type.Properties.OfType<StringProperty>().FirstOrDefault(x => x.IsRowVersion);

                if (version == null)
                {
                    if (updateValueResult.ToStringOrEmpty().IsEmpty())
                    {
                        Cache.Remove(record);
                        throw new ConcurrencyException(errorText);
                    }
                }
                else
                {
                    errorText += $", or {version.Name} was different";
                    var versionValueProperty = ConvertTo(updateValueResult, Type.GetType(version.PropertyType));
                    if (versionValueProperty != null)
                        type.GetType().GetProperty(version.Name).SetValue(record, versionValueProperty);
                    else
                    {
                        Cache.Remove(record);
                        throw new ConcurrencyException(errorText);
                    }
                }
            }
        }

        async Task SaveManyToManyRelation(IEntity record)
        {
            if (!ManyToManyAssociations.None())
            {
                foreach (var property in ManyToManyAssociations)
                {
                    var inner = MetaData.GetInnerColumn(property);
                    var outer = MetaData.GetOuterColumn(property);
                    var bridge = property.BridgeTableName;

                    var command = "DELETE FROM {0} WHERE {1} = @{2}".FormatWith(bridge, inner, inner);
                    var commands = new List<string> { command };
                    var parameters = new List<IDataParameter> { CreateParameter(inner, record.GetId(), property.DbType) };

                    var objectIdsProperty = record.GetType().GetProperties().FirstOrDefault(x => x.Name == property.Name + "Ids");
                    if (objectIdsProperty == null) continue;
                    var objectIdsPropertyList = objectIdsProperty.GetValue(record, null) as IList<string>;

                    var objDataType = property.DbType;

                    for (var i = 0; i < objectIdsPropertyList.Count; i++)
                    {

                        var insertCommand = $"INSERT INTO {bridge} ({inner}, {outer}) VALUES (@{inner}, @{outer}{i})";
                        commands.Add(insertCommand);

                        parameters.Add(CreateParameter(outer + i, objectIdsPropertyList[i], objDataType));
                    }
                    await Db.ExecuteNonQuery(commands.ToString(";"), CommandType.Text, parameters.ToArray()).ConfigureAwait(false);

                }
            }
        }

        internal static IDataParameter[] CreateParameters(EntityType type, IEntity item)
        {
            var result = new List<IDataParameter>();

            var originalIdValue = item.GetType().GetProperty("OriginalId").GetValue(item, null);
            result.Add(CreateParameter("OriginalId", originalIdValue, DbType.Guid));

            if (type.CanSaveId())
                result.Add(CreateParameter("Id", item.GetId(), DbType.Int32));

            foreach (var column in type.GetDatabaseFieldsList())
            {
                if ((column.Value as NumberProperty)?.IsAutoNumber == true)
                    continue;

                var propertyValue = item.GetType().GetProperty(GetPropertySetter(type, column.Key)).GetValue(item, null);

                if (propertyValue != null)
                    if (column.Value is DateTimeProperty dp && column.Value.DatabaseColumnType == "datetime2")
                        result.Add(CreateParameter(column.Key, propertyValue, DbType.DateTime2));
                    else
                        result.Add(CreateParameter(column.Key, propertyValue, column.Value.DbType));
                else
                    result.Add(CreateParameter(column.Key, DBNull.Value, column.Value.DbType));
            }

            if (type.SoftDelete)
            {
                var propertyValue = item.GetType().GetProperty("IsMarkedSoftDeleted").GetValue(item, null);
                result.Add(CreateParameter("_DELETED", propertyValue, DbType.Boolean));
            }

            return result.ToArray();
        }

        internal static IDataParameter CreateParameter(string parameterName, object value, DbType columnType)
        {
            if (value == null) value = DBNull.Value;
            else if (value is Blob blob) value = blob.FileName;
            return new ObjectDataProviderTDataParameter(parameterName.Remove(" "), value, columnType);
        }

        public string MapColumn(string propertyName)
        {
            if (MapColumnCache.TryGetValue(propertyName, out var result))
                return result;

            var defaultExpression = $"{GetSelectAlias(ObjectType)}.[{propertyName}]";
            var mappings = PropertyToColumnMappings.ToList();
            var exceptions = mappings.Where(x => x.Item2 != GetSelectAlias(ObjectType) || x.Item1 != x.Item3);

            if (exceptions.Any())
            {
                if (exceptions.IsSingle())
                {
                    var item = exceptions.Single();
                    //return propertyName == item.Item1 ? $"{item.Item2}.{item.Item3}" : defaultExpression;
                    return MapColumnCache[propertyName] = propertyName == item.Item1 ? $"{item.Item2}.{item.Item3}" : defaultExpression;
                }
                else
                {
                    var item = exceptions.FirstOrDefault(x => x.Item1 == propertyName);
                    return MapColumnCache[propertyName] = item == null ? defaultExpression : $"{item.Item2}.{item.Item3}";
                }
            }
            else
                return MapColumnCache[propertyName] = $"{GetSelectAlias(ObjectType)}.{propertyName}";
        }

        internal static Association GetAssosciationObject(PropertyInfo myProperty)
        {
            if (GetAssosciationObjectCache.TryGetValue(myProperty, out var result))
                return result;

            string bridgColumn1 = "", bridgColumn2 = "", bridgTableName = "";

            if (Attribute.IsDefined(myProperty, typeof(BridgColumnAttribute)))
            {
                var bridgAttributeObject = Attribute.GetCustomAttribute(myProperty, typeof(BridgColumnAttribute)) as BridgColumnAttribute;
                var bridgType = bridgAttributeObject.Type;
                if (bridgType == 1)
                    bridgColumn1 = bridgAttributeObject.ColumnName;
                else if (bridgType == 2)
                    bridgColumn2 = bridgAttributeObject.ColumnName;
            }

            if (Attribute.IsDefined(myProperty, typeof(BridgTableAttribute)))
            {
                var bridgAttributeObject = Attribute.GetCustomAttribute(myProperty, typeof(BridgTableAttribute)) as BridgTableAttribute;
                bridgTableName = bridgAttributeObject.TableName;
            }

            var inverseAssociation = new Association();
            if (Attribute.IsDefined(myProperty, typeof(InversePropertyAttribute)))
            {
                var inversePropertyAttribute = Attribute.GetCustomAttribute(myProperty, typeof(InversePropertyAttribute)) as InversePropertyAttribute;
                var inversePropertyName = inversePropertyAttribute.Property;

                var inversePropertyType = myProperty.PropertyType.GetProperty(inversePropertyName);

                inverseAssociation = GetAssosciationObject(inversePropertyType);
            }

            var referencedType = MetaData.ExtractMetadata(myProperty.DeclaringType, tryToFindParent: false, tryToFindProperties: false);
            var doneflag = false;
            foreach (var item in myProperty.DeclaringType.GetProperties())
            {
                if (Attribute.IsDefined(item, typeof(PrimaryKeyAttribute)))
                {
                    referencedType.IdColumnName = item.Name;
                    referencedType.PrimaryKeyType = item.PropertyType.ToString();
                    referencedType.HasCustomPrimaryKeyColumn = true;
                    doneflag = true;
                    break;
                }
            }

            if (doneflag == false)
            {
                referencedType.IdColumnName = "Id";
                referencedType.PrimaryKeyType = myProperty.DeclaringType.GetProperties().First(x => x.Name.ToLower() == "id").PropertyType.ToString();
                referencedType.HasCustomPrimaryKeyColumn = false;
            }

            return GetAssosciationObjectCache[myProperty] = new Association
            {
                ReferencedType = referencedType,

                IsManyToOne = !Attribute.IsDefined(myProperty, typeof(ManyToManyAttribute)),

                IsManyToMany = Attribute.IsDefined(myProperty, typeof(ManyToManyAttribute)),

                LazyLoad = Attribute.IsDefined(myProperty, typeof(LazyLoadAttribute)),

                BridgeColumn1 = bridgColumn1,//inner coloumn

                BridgeColumn2 = bridgColumn2,//outer column

                BridgeTableName = bridgTableName,

                InverseAssociation = inverseAssociation,
            };

        }

        public async Task Delete(IEntity record)
        {
            await Db.ExecuteNonQuery(DeleteCommand, CommandType.Text, CreateParameter("Id", record.GetId(), DbType.Int32)).ConfigureAwait(false);
        }

        public async Task BulkInsert(IEntity[] entities, int batchSize)
        {
            var commands = new List<KeyValuePair<string, IDataParameter[]>>();

            var entity = Type.GetType(ObjectType.ClassFullName);
            var manyToManyAssociationFlag = false;
            if (ManyToManyAssociations.Any())
                manyToManyAssociationFlag = true;
            foreach (var item in entities)//entities.Cast<{ObjectType.ClassFullName}>())
            {
                if (ParticipatesInHierarchy && item.GetType() != Type.GetType(ObjectType.AssemblyFullName))
                    throw new ArgumentException($"Invalid argument type specified. Expected: '{ObjectType.ClassFullName}', Provided: '{item.GetType()}'");

                foreach (var type in TopToBottomHierarchy)
                {
                    var tableName = type.TableName;
                    var insertCommandfortable = InsertCommand[type.TableName];

                    commands.Add(insertCommandfortable, CreateParameters(type, item));
                }

                Task.Factory.RunSync(async () => await Db.ExecuteBulkNonQueries(CommandType.Text, commands).ConfigureAwait(false));

                if (manyToManyAssociationFlag)
                    await SaveManyToManyRelation(item);
            }
        }

        public async Task BulkUpdate(IEntity[] entities, int batchSize)
        {
            foreach (var item in entities)
                await Database.Save(item, SaveBehaviour.BypassAll).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property)
        {
            /*
             * Async mode : async task<ieumrable<String>>  ===>iscore 
             * normal mode <ienumrable<string>>
             * 
             * */

            var lazyLoadables = ManyToManyAssociations.Where(z => z.LazyLoad).ToList();

            if (lazyLoadables.Any())
            {
                var query = string.Empty;
                var parameter = CreateParameter("InstanceId", instance.GetId(), DbType.Int32);

                foreach (var propertyItem in lazyLoadables)
                {
                    var select = propertyItem.InverseAssociation == null ? propertyItem.BridgeColumn2 : propertyItem.BridgeColumn1;
                    var where = propertyItem.InverseAssociation == null ? propertyItem.BridgeColumn1 : propertyItem.BridgeColumn2;

                    if (property == propertyItem.DatabaseColumnName + "Ids")
                        query = $"SELECT {select} FROM {propertyItem.BridgeTableName} WHERE {where} = @InstanceId";
                }
                if (query.IsEmpty())
                    throw new ArgumentException($"The property '{property}' is not supported for the instance of '{instance.GetType()}'");
                using (var reader = await Db.ExecuteReader(query, CommandType.Text, parameter).ConfigureAwait(false))
                {
                    var result = new List<string>();
                    while (reader.Read())
                        result.Add(reader[0].ToString());
                    return result;
                }

            }
            else
            {
                throw new ArgumentException($"The property '{property}' is not supported for the instance of '{instance.GetType()}'");
            }
        }

        public async Task<IEnumerable<IEntity>> GetList(IDatabaseQuery query)
        {
            using (var reader = await ExecuteGetListReader(query).ConfigureAwait(false))
            {
                var fields = FullHierarchyFields;
                var result = new List<IEntity>();
                while (reader.Read()) result.Add(Parse(reader, fields));
                return result;
            }
        }

        internal async Task<IDataReader> ExecuteGetListReader(IDatabaseQuery query)
        {
            var command = GenerateSelectCommand(query, FieldsString);
            return await Db.ExecuteReader(command, CommandType.Text, GenerateParameters(query.Parameters)).ConfigureAwait(false);
        }

        public string MapSubquery(string path, string parent)
        {
            if (MapSubQueryCache.TryGetValue(path, out var result))
                return result;

            if (SubqueryAssociations.None())
                return MapSubQueryCache[path] = string.Empty;

            foreach (var query in SubqueryAssociations)
            {
                if (path != query.Name + ".*")
                    return MapSubquery(path, parent);

                var targetList = new MetaData(query.ReferencedType.GetType(), Sql).AllTypesInMyHierarchy
                    .ToDictionary(c => $"target{c.TableName}", c => $"{parent}.{query.Name}_{c.ClassName}");

                result = GenerateSubQueryExpression(query, targetList).Trim();
                foreach (var item in targetList)
                    result = result.Replace($"{{{item.Key}}}", item.Value);

                return MapSubQueryCache[path] = result;
            }

            throw new Exception("Impossible execution path");
        }
    }
}
