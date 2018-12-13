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

        public SqlDialect SqlDialect { get; }

        public SqlCommandGenerator SqlCommandGenerator { get; }

        public DataProviderMedaData MedaData {get;}

        public override Type EntityType => entityType;

        protected ObjectDataProvider(Type type, ICache cache, SqlDialect sqlDialect, SqlCommandGenerator sqlCommandGenerator)
            : base(cache)
        {
            entityType = type;
            SqlDialect = sqlDialect;
            SqlCommandGenerator = sqlCommandGenerator;

            MedaData = ObjectDataProviderFactory.Create(type);
        }

        public override string MapColumn(string propertyName)
        {
            if (propertyName == "ID" || MedaData.Properties.Any(p => p.IsPrimaryKey))
                return GetSqlCommandColumn(MedaData, MedaData.Properties.First(p => p.IsPrimaryKey));

            foreach (var prop in MedaData.Properties)
                if (prop.Name == propertyName)
                    return GetSqlCommandColumn(MedaData, prop);

            foreach (var parent in MedaData.BaseClassesInOrder)
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
            return ExecuteNonQuery(GetDeleteCommand(), CommandType.Text, CreateParameter("Id", record.GetId()));
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

                foreach (var parent in MedaData.BaseClassesInOrder)
                    foreach (var prop in parent.Properties)
                        Fields += $"{GetSqlCommandColumn(parent, prop)} as {parent.TableName}_{prop.Name}";

                foreach (var prop in MedaData.Properties)
                    Fields += $"{GetSqlCommandColumn(MedaData, prop)} as {MedaData.TableName}_{prop.Name}";

                foreach (var parent in MedaData.DrivedClasses)
                    foreach (var prop in parent.Properties)
                        Fields += $"{GetSqlCommandColumn(parent, prop)} as {parent.TableName}_{prop.Name}";
            }

            return Fields;
        }

        public override string GetTables(string prefix = null)
        {
            throw new NotImplementedException();
        }

        public override IEntity Parse(IDataReader reader)
        {
            throw new NotImplementedException();
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

        string GetSqlCommandColumn(DataProviderMedaData medaData, PropertyData property) => $"{medaData.TableAlias}.[{property.Name}]";
    }
}
