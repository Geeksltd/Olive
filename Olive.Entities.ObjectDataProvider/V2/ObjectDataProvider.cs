using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.ObjectDataProvider.V2
{
    public abstract class ObjectDataProvider<TConnection, TDataParameter> : DataProvider<TConnection, TDataParameter>
         where TConnection : DbConnection, new()
         where TDataParameter : IDbDataParameter, new()
    {
        readonly Type entityType;

        string Fields = null;
        string TablesTemplate = null;

        public SqlDialect SqlDialect { get; }

        public SqlCommandGenerator SqlCommandGenerator { get; }

        public DataProviderMetaData MetaData {get;}

        public override Type EntityType => entityType;

        protected ObjectDataProvider(Type type, ICache cache, SqlDialect sqlDialect, SqlCommandGenerator sqlCommandGenerator)
            : base(cache)
        {
            entityType = type;
            SqlDialect = sqlDialect;
            SqlCommandGenerator = sqlCommandGenerator;

            MetaData = DataProviderMetaDataGenerator.Generate(type);
        }

        public override string MapColumn(string propertyName)
        {
            if (propertyName == "ID" || MetaData.Properties.Any(p => p.IsPrimaryKey))
                return GetSqlCommandColumn(MetaData, MetaData.Properties.First(p => p.IsPrimaryKey));

            foreach (var prop in MetaData.Properties)
                if (prop.Name == propertyName)
                    return GetSqlCommandColumn(MetaData, prop);

            foreach (var parent in MetaData.BaseClassesInOrder)
                foreach (var prop in parent.Properties)
                    if (prop.Name == propertyName)
                        return GetSqlCommandColumn(parent, prop);

            throw new ArgumentOutOfRangeException(nameof(propertyName));
        }

        protected override string SafeId(string objectName)
        {
            throw new NotImplementedException("Should be moved to data access class.");
        }

        public override Task Delete(IEntity record)
        {
            return ExecuteNonQuery(GetDeleteCommand(), CommandType.Text, Access.CreateParameter("Id", record.GetId()));
        }

        public override Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property)
        {
            throw new NotImplementedException();
        }

        public override async Task Save(IEntity record)
        {
            if (record.IsNew)
                await Insert(record);
            else
                await Update(record);
        }

        public override string GetFields()
        {
            if (Fields.IsEmpty())
            {
                Fields = "";

                foreach (var parent in MetaData.BaseClassesInOrder)
                    foreach (var prop in parent.Properties)
                        Fields += $"{GetSqlCommandColumn(parent, prop)} as {GetSqlCommandColumnAlias(parent, prop)}";

                foreach (var prop in MetaData.Properties)
                    Fields += $"{GetSqlCommandColumn(MetaData, prop)} as {GetSqlCommandColumnAlias(MetaData, prop)}";

                foreach (var parent in MetaData.DrivedClasses)
                    foreach (var prop in parent.Properties)
                        Fields += $"{GetSqlCommandColumn(parent, prop)} as {GetSqlCommandColumnAlias(parent, prop)}";
            }

            return Fields;
        }

        public override string GetTables(string prefix = null)
        {
            if (TablesTemplate.IsEmpty())
            {
                TablesTemplate = "";
                DataProviderMetaData temp = null;

                void addTable(DataProviderMetaData medaData)
                {
                    TablesTemplate += "LEFT OUTER JOIN ".OnlyWhen(temp != null) +
                        $"{medaData.Schema.WithSuffix(".")}{medaData.TableName} AS {{0}}{medaData.TableAlias} " +
                        $"ON {{0}}{medaData.TableAlias}.{medaData.IdColumnName} = {{0}}{temp?.TableAlias}.{temp?.IdColumnName}".OnlyWhen(temp != null);

                    temp = medaData;
                }

                foreach (var parent in MetaData.BaseClassesInOrder)
                    addTable(parent);

                addTable(MetaData);

                foreach (var drived in MetaData.DrivedClasses)
                    addTable(drived);
            }

            return TablesTemplate.FormatWith(prefix);
        }

        public override IEntity Parse(IDataReader reader)
        {
            for (int index = MetaData.DrivedClasses.Length - 1; index > -1; index++)
            {
                var current = MetaData.DrivedClasses[index];

                if (reader[GetSqlCommandColumnAlias(current, current.IdColumnName)] != DBNull.Value)
                    return ObjectDataProviderFactory.Get<TConnection, TDataParameter>(current.Type).Parse(reader);
            }

            if(MetaData.Type.IsAbstract)
                throw new Exception($"The record with ID of '{reader[GetSqlCommandColumnAlias(MetaData, MetaData.IdColumnName)]}' exists only in the abstract database table of '{MetaData.TableName}' and no concrete table. The data needs cleaning-up.");

            var result = (IEntity) Activator.CreateInstance(EntityType);

            FillData(reader, result);

            foreach (var parent in MetaData.BaseClassesInOrder)
                ObjectDataProviderFactory.Get<TConnection, TDataParameter>(parent.Type).FillData(reader, result);

            return result;
        }

        public override string GenerateSelectCommand(IDatabaseQuery iquery, string fields)
        {
            throw new NotImplementedException();
        }

        public override string GenerateWhere(DatabaseQuery query)
        {
            throw new NotImplementedException();
        }

        private Task Update(IEntity record)
        {
            throw new NotImplementedException();
        }

        private Task Insert(IEntity record)
        {
            throw new NotImplementedException();
        }

        private string GetDeleteCommand()
        {
            throw new NotImplementedException();
        }

        void FillData(IDataReader reader, IEntity entity)
        {
            foreach (var property in MetaData.Properties)
            {
                var value = reader[GetSqlCommandColumnAlias(MetaData, property)];

                if(value != DBNull.Value)
                    property.PropertyInfo.SetValue(entity, value);
            }
        }

        string GetSqlCommandColumn(DataProviderMetaData medaData, PropertyData property)
            => $"{medaData.TableAlias}.[{property.Name}]";

        string GetSqlCommandColumnAlias(DataProviderMetaData medaData, PropertyData property)
            => GetSqlCommandColumnAlias(medaData, property.Name);

        private string GetSqlCommandColumnAlias(DataProviderMetaData medaData, string propertyName) 
            => $"{medaData.TableName}_{propertyName}";
    }
}
