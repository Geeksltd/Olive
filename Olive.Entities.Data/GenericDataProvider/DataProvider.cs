using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    partial class DataProvider
    {
        MappedProperty[] AllFields;
        string fields, TablesTemplate;
        readonly Dictionary<string, string> ColumnMapping = new();
        readonly Dictionary<string, string> SubqueryMapping = new();

        public ISqlCommandGenerator SqlCommandGenerator { get; }

        public IDataProviderMetaData MetaData { get; }

        public string DeleteCommand { get; }

        public string UpdateCommand { get; }

        public string InsertCommand { get; }

        public Func<IEntity, Task> UpdateSelf { get; }

        internal DataProvider(Type type, ICache cache, IDataAccess access, ISqlCommandGenerator sqlCommandGenerator)
        {
            EntityType = type;
            SqlCommandGenerator = sqlCommandGenerator;
            Cache = cache;
            Access = access;

            MetaData = DataProviderMetaDataGenerator.Generate(type);


            DeleteCommand = SqlCommandGenerator.GenerateDeleteCommand(MetaData);
            UpdateCommand = SqlCommandGenerator.GenerateUpdateCommand(MetaData);
            InsertCommand = SqlCommandGenerator.GenerateInsertCommand(MetaData);

            if (UpdateCommand.IsEmpty())
                UpdateSelf = entity => Task.CompletedTask;
            else
                UpdateSelf = UpdateSelfImpl;
        }

        internal void Prepare()
        {
            PrepareTableTemplate();
            PrepareFields();
            PrepareColumnMappingDictonary();
            PrepareSubqueryMappingDictonary();
        }

        public virtual string MapColumn(string propertyName)
        {
            if (ColumnMapping.TryGetValue(propertyName, out var result)) return result;

            return $"{MetaData.TableAlias}.{SqlCommandGenerator.SafeId(propertyName)}";
        }

        protected virtual string SafeId(string objectName)
        {
            throw new NotImplementedException("Should be moved to data access class.");
        }

        /// <summary>
        /// Deletes the specified record.
        /// </summary>
        public virtual Task Delete(IEntity record)
        {
            var myBase = MetaData.BaseClassesInOrder.FirstOrDefault();

            if (myBase != null) return For(myBase).Delete(record);
            return ExecuteNonQuery(DeleteCommand, CommandType.Text, Access.CreateParameter("Id", record.GetId()));
        }

        /// <summary>
        /// Reads the many to many relation.
        /// </summary>
        public virtual Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property)
        {
            throw new ArgumentException($"The property '{property}' is not supported for the instance of '{instance.GetType()}'");
        }

        /// <summary>
        /// Saves the specified record.
        /// </summary>
        public virtual async Task Save(IEntity record)
        {
            if (record.IsNew) await Insert(record);
            else await Update(record);
        }

        public string GetFields() => fields ?? (fields = AllFields.ToString(", "));

        string GetFields(IDatabaseQuery query)
        {
            if (query is DatabaseQuery q && q.Columns.Any())
                return AllFields.Where(v => q.Columns.Contains(v.Name)).ToString(", ");
            else
                return GetFields();
        }

        public virtual string GetTables(string prefix = null) => TablesTemplate.FormatWith(prefix);

        public virtual IEntity Parse(IDataReader reader)
        {
            for (var index = MetaData.DrivedClasses.Length - 1; index > -1; index--)
            {
                var current = MetaData.DrivedClasses[index];

                if (reader[GetSqlCommandColumnAlias(current, current.IdColumnName)] != DBNull.Value)
                    return For(current).Parse(reader);
            }

            if (MetaData.Type.IsAbstract)
                throw new Exception($"The record with ID of '{reader[GetSqlCommandColumnAlias(MetaData, MetaData.IdColumnName)]}' exists only in the abstract database table of '{MetaData.TableName}' and no concrete table. The data needs cleaning-up.");

            var result = (IEntity)Activator.CreateInstance(EntityType);

            Entity.Services.SetSaved(result);

            FillData(reader, result);

            foreach (var parent in MetaData.BaseClassesInOrder)
                For(parent).FillData(reader, result);

            Entity.Services.SetOriginalId(result);

            return result;
        }

        public virtual string GenerateSelectCommand(IDatabaseQuery iquery, string fields) =>
            SqlCommandGenerator.GenerateSelectCommand(iquery, GetTables(iquery.AliasPrefix), fields);

        public virtual string GenerateWhere(DatabaseQuery query) =>
            SqlCommandGenerator.GenerateWhere(query);

        public virtual string MapSubquery(string path, string parent)
        {
            if (SubqueryMapping.TryGetValue(path, out var value))
                return value.FormatWith(parent, parent.Or(MetaData.TableAlias));

            throw new NotSupportedException($"{GetType().Name} does not provide a sub-query mapping for '{path}'.");
        }

        async Task Update(IEntity record)
        {
            async Task saveAll()
            {
                foreach (var parent in MetaData.BaseClassesInOrder)
                    await For(parent).Update(record);

                await UpdateSelf(record);
            }

            if (Database.AnyOpenTransaction()) await saveAll();
            else using (var scope = Database.CreateTransactionScope()) { await saveAll(); scope.Complete(); }
        }

        async Task UpdateSelfImpl(IEntity record)
        {
            if ((await ExecuteNonQuery(UpdateCommand, CommandType.Text, CreateParameters(record, forInsert: false))) == 0)
            {
                Cache.Remove(record);
                throw new ConcurrencyException($"Failed to update the '{MetaData.TableName}' table. There is no row with the ID of {record.GetId()}.");
            }
        }

        IDataParameter[] CreateParameters(IEntity record, bool forInsert)
        {
            var result = new List<IDataParameter>();

            foreach (var prop in forInsert ? MetaData.GetPropertiesForInsert() : MetaData.Properties)
                result.Add(Access.CreateParameter(prop.ParameterName, prop.Accessor.Get(record) ?? DBNull.Value));

            return result.ToArray();
        }

        async Task Insert(IEntity record)
        {
            async Task saveAll()
            {
                foreach (var parent in MetaData.BaseClassesInOrder)
                    await For(parent).InsertSelf(record);

                await InsertSelf(record);
            }

            if (Database.AnyOpenTransaction()) await saveAll();
            else using (var scope = Database.CreateTransactionScope()) { await saveAll(); scope.Complete(); }
        }

        async Task InsertSelf(IEntity record)
        {
            var result = await ExecuteScalar(InsertCommand, CommandType.Text, CreateParameters(record, forInsert: true));

            Entity.Services.SetSaved(record);

            if (MetaData.HasAutoNumber)
                MetaData.AutoNumberProperty.Accessor.Set(record, result);

            Entity.Services.SetOriginalId(record);
        }

        void FillData(IDataReader reader, IEntity entity)
        {
            var schema = reader.GetSchemaTable();
            var columns = schema.Rows.Cast<DataRow>()
                                     .Select(r => r["ColumnName"].ToString())
                                     .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

            var props = MetaData.GetPropertiesForFillData().ToArray();

            foreach (var property in props)
            {
                var alias = GetSqlCommandColumnAlias(MetaData, property);
                if (columns.Contains(alias))
                {
                    var columnOrder = reader.GetOrdinal(alias);
                    property.Accessor.Set(entity, reader, columnOrder);
                }
            }
        }

        string GetSqlCommandColumn(IDataProviderMetaData medaData, IPropertyData property) =>
            GetSqlCommandColumn(medaData, property.Name);

        string GetSqlCommandColumn(IDataProviderMetaData medaData, string propertyName) =>
            $"{medaData.TableAlias}.{SqlCommandGenerator.SafeId(propertyName)}";

        string GetSqlCommandColumnAlias(IDataProviderMetaData medaData, IPropertyData property) =>
            GetSqlCommandColumnAlias(medaData, property.Name);

        string GetSqlCommandColumnAlias(IDataProviderMetaData medaData, string propertyName) =>
            $"{medaData.TableName}_{propertyName.Remove(".")}";

        void PrepareTableTemplate() => TablesTemplate = MetaData.GetTableTemplate(SqlCommandGenerator);

        struct MappedProperty
        {
            public string Name, SqlSource, Alias;

            public MappedProperty(string name, string sql, string alias)
            {
                Name = name;
                SqlSource = sql;
                Alias = alias;
            }

            public override string ToString() => $"{SqlSource} as {Alias}";
        }



        MappedProperty GetField(IDataProviderMetaData table, IPropertyData prop)
        {
            return new MappedProperty(prop.Name, GetSqlCommandColumn(table, prop), GetSqlCommandColumnAlias(table, prop));
        }

        void PrepareFields()
        {
            AllFields = MetaData.BaseClassesInOrder.Concat(MetaData).Concat(MetaData.DrivedClasses)
                .SelectMany(parent => parent.UserDefienedAndIdAndDeletedProperties.Select(prop => GetField(parent, prop)))
                .ToArray();
        }

        void PrepareColumnMappingDictonary()
        {
            ColumnMapping.Add(PropertyData.DEFAULT_ID_COLUMN, GetSqlCommandColumn(MetaData, MetaData.IdColumnName));

            if (MetaData.IsSoftDeleteEnabled)
                ColumnMapping.Add(
                    PropertyData.IS_MARKED_SOFT_DELETED,
                    GetSqlCommandColumn(MetaData, MetaData.Properties.First(p => p.IsDeleted)));

            foreach (var prop in MetaData.UserDefienedProperties)
                ColumnMapping.Add(prop.Name, GetSqlCommandColumn(MetaData, prop));

            foreach (var parent in MetaData.BaseClassesInOrder)
                foreach (var prop in parent.UserDefienedProperties)
                    ColumnMapping.Add(prop.Name, GetSqlCommandColumn(parent, prop));
        }

        void PrepareSubqueryMappingDictonary()
        {
            foreach (var association in MetaData.AssociateProperties)
            {
                var associateMetaData = DataProviderMetaDataGenerator.Generate(association.AssociateType);

                var alias = SqlCommandGenerator.SafeId($"{{0}}.{association.Name}_{associateMetaData.TableName}");
                var partialAlias = $"{{0}}.{association.Name}_";

                var template = $@"SELECT {alias}.{associateMetaData.IdColumnName}
                    FROM {associateMetaData.GetTableTemplate(SqlCommandGenerator).FormatWith(partialAlias)}
                    WHERE {alias}.{SqlCommandGenerator.SafeId(associateMetaData.IdColumnName)} = {SqlCommandGenerator.SafeId("{1}")}.{SqlCommandGenerator.SafeId(association.Name)}";

                SubqueryMapping.Add(
                    association.Name.WithSuffix(".*"),
                    template);
            }

            foreach (var baseClass in MetaData.BaseClassesInOrder)
                foreach (var pair in For(baseClass).SubqueryMapping)
                {
                    if (SubqueryMapping.ContainsKey(pair.Key) && SubqueryMapping[pair.Key] != pair.Value)
                        throw new Exception("Multiple subqueries needed with the same key.");

                    SubqueryMapping[pair.Key] = pair.Value;
                }
        }

        DataProvider For(IDataProviderMetaData meta) => DataProviderFactory.GetOrCreate(meta.Type, Cache, Access, SqlCommandGenerator);
    }
}
