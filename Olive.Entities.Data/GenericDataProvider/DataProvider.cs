using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    partial class DataProvider
    {
        string Fields, TablesTemplate;
        Dictionary<string, string> ColumnMapping = new Dictionary<string, string>();
        Dictionary<string, string> SubqueryMapping = new Dictionary<string, string>();

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
            if (ColumnMapping.TryGetValue(propertyName, out string result)) return result;

            return $"{MetaData.TableAlias}.[{propertyName}]";
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
            if (MetaData.BaseClassesInOrder.Any())
                return MetaData.BaseClassesInOrder.First().GetProvider(Cache, Access, SqlCommandGenerator).Delete(record);

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

        public virtual string GetFields() => Fields;

        public virtual string GetTables(string prefix = null) => TablesTemplate.FormatWith(prefix);

        public virtual IEntity Parse(IDataReader reader)
        {
            for (int index = MetaData.DrivedClasses.Length - 1; index > -1; index--)
            {
                var current = MetaData.DrivedClasses[index];

                if (reader[GetSqlCommandColumnAlias(current, current.IdColumnName)] != DBNull.Value)
                    return current.GetProvider(Cache, Access, SqlCommandGenerator).Parse(reader);
            }

            if (MetaData.Type.IsAbstract)
                throw new Exception($"The record with ID of '{reader[GetSqlCommandColumnAlias(MetaData, MetaData.IdColumnName)]}' exists only in the abstract database table of '{MetaData.TableName}' and no concrete table. The data needs cleaning-up.");

            var result = (IEntity)Activator.CreateInstance(EntityType);

            FillData(reader, result);

            foreach (var parent in MetaData.BaseClassesInOrder)
                parent.GetProvider(Cache, Access, SqlCommandGenerator).FillData(reader, result);

            Entity.Services.SetSaved(result);

            return result;
        }

        public virtual string GenerateSelectCommand(IDatabaseQuery iquery, string fields) =>
            SqlCommandGenerator.GenerateSelectCommand(iquery, GetTables(iquery.AliasPrefix), fields);

        public virtual string GenerateWhere(DatabaseQuery query) =>
            SqlCommandGenerator.GenerateWhere(query);

        public virtual string MapSubquery(string path, string parent)
        {
            if (SubqueryMapping.TryGetValue(path, out string value))
                return value.FormatWith(parent, parent.Or(MetaData.TableAlias));

            throw new NotSupportedException($"{GetType().Name} does not provide a sub-query mapping for '{path}'.");
        }

        async Task Update(IEntity record)
        {
            async Task saveAll()
            {
                foreach (var parent in MetaData.BaseClassesInOrder)
                    await parent.GetProvider(Cache, Access, SqlCommandGenerator).Update(record);

                await UpdateSelf(record);
            }

            if (Database.AnyOpenTransaction()) await saveAll();
            else using (var scope = Database.CreateTransactionScope()) { await saveAll(); scope.Complete(); }
        }

        async Task UpdateSelfImpl(IEntity record)
        {
            if ((await ExecuteScalar(UpdateCommand, CommandType.Text, CreateParameters(record, forInsert: false))).ToStringOrEmpty().IsEmpty())
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
                    await parent.GetProvider(Cache, Access, SqlCommandGenerator).Insert(record);

                var result = await ExecuteScalar(InsertCommand, CommandType.Text, CreateParameters(record, forInsert: true));

                Entity.Services.SetSaved(record);

                if (MetaData.HasAutoNumber)
                    MetaData.AutoNumberProperty.Accessor.Set(record, result);
            }

            if (Database.AnyOpenTransaction()) await saveAll();
            else using (var scope = Database.CreateTransactionScope()) { await saveAll(); scope.Complete(); }
        }

        void FillData(IDataReader reader, IEntity entity)
        {
            foreach (var property in MetaData.GetPropertiesForFillData())
            {
                var value = reader[GetSqlCommandColumnAlias(MetaData, property)];

                if (value != DBNull.Value)
                    property.Accessor.Set(entity, value);
            }
        }

        string GetSqlCommandColumn(IDataProviderMetaData medaData, IPropertyData property) =>
            GetSqlCommandColumn(medaData, property.Name);

        string GetSqlCommandColumn(IDataProviderMetaData medaData, string propertyName) =>
            $"{medaData.TableAlias}.[{propertyName}]";

        string GetSqlCommandColumnAlias(IDataProviderMetaData medaData, IPropertyData property) =>
            GetSqlCommandColumnAlias(medaData, property.Name);

        string GetSqlCommandColumnAlias(IDataProviderMetaData medaData, string propertyName) =>
            $"{medaData.TableName}_{propertyName}";

        void PrepareTableTemplate() => TablesTemplate = MetaData.GetTableTemplate();

        void PrepareFields()
        {
            Fields = "";

            foreach (var parent in MetaData.BaseClassesInOrder)
                foreach (var prop in parent.UserDefienedAndIdProperties)
                    Fields += $"{GetSqlCommandColumn(parent, prop)} as {GetSqlCommandColumnAlias(parent, prop)}, ";

            foreach (var prop in MetaData.UserDefienedAndIdProperties)
                Fields += $"{GetSqlCommandColumn(MetaData, prop)} as {GetSqlCommandColumnAlias(MetaData, prop)}, ";

            foreach (var parent in MetaData.DrivedClasses)
                foreach (var prop in parent.UserDefienedAndIdProperties)
                    Fields += $"{GetSqlCommandColumn(parent, prop)} as {GetSqlCommandColumnAlias(parent, prop)}, ";

            Fields = Fields.TrimEnd(2);
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

                var alias = $"[{{0}}.{association.Name}_{associateMetaData.TableName}]";
                var partialAlias = $"{{0}}.{association.Name}_";

                var template = $@"SELECT {alias}.{associateMetaData.IdColumnName}
                    FROM {associateMetaData.GetTableTemplate().FormatWith(partialAlias)}
                    WHERE {alias}.[{associateMetaData.IdColumnName}] = [{{1}}].[{association.Name}]";

                SubqueryMapping.Add(
                    association.Name.WithSuffix(".*"),
                    template);
            }

            foreach (var baseClass in MetaData.BaseClassesInOrder)
                foreach (var pair in baseClass.GetProvider(Cache, Access, SqlCommandGenerator).SubqueryMapping)
                    SubqueryMapping.Add(pair.Key, pair.Value);
        }
    }
}
