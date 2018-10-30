using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Olive.Entities.Data
{
    public abstract class SqlDataProvider<TTargetEntity> : SqlDataProvider where TTargetEntity : IEntity
    {
        protected SqlDataProvider(ICache cache) : base(cache)
        {
        }

        public override Type EntityType => typeof(TTargetEntity);
    }

    /// <summary>
    /// Provides a DataProvider for accessing data from the database using ADO.NET based on the SqlClient provider.
    /// </summary>
    public abstract partial class SqlDataProvider : DataProvider<SqlConnection, SqlParameter>
    {
        protected SqlDataProvider(ICache cache) : base(cache)
        {
        }

        public override IDataParameter GenerateParameter(KeyValuePair<string, object> data)
        {
            var value = data.Value;

            var result = new SqlParameter { Value = value, ParameterName = data.Key.Remove(" ") };

            if (value is DateTime)
            {
                result.DbType = DbType.DateTime2;
                result.SqlDbType = SqlDbType.DateTime2;
            }

            return result;
        }

        public override string GenerateSelectCommand(IDatabaseQuery iquery, string fields)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue && query.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var r = new StringBuilder("SELECT");

            r.Append(query.TakeTop.ToStringOrEmpty().WithPrefix(" TOP "));
            r.AppendLine($" {fields} FROM {GetTables()}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine(GenerateSort(query).WithPrefix(" ORDER BY "));

            return r.ToString();
        }

        public override string GenerateWhere(DatabaseQuery query)
        {
            var r = new StringBuilder();

            if (SoftDeleteAttribute.RequiresSoftdeleteQuery(query.EntityType))
                query.Criteria.Add(new Criterion("IsMarkedSoftDeleted", false));

            r.Append($" WHERE { query.Column("ID")} IS NOT NULL");

            var whereGenerator = new SqlCriterionGenerator(query);
            foreach (var c in query.Criteria)
                r.Append(whereGenerator.Generate(c).WithPrefix(" AND "));

            return r.ToString();
        }

        protected override string SafeId(string objectName) => $"[{objectName}]";
    }
}