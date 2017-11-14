namespace Olive.Entities.Data
{
    using Npgsql;
    using NpgsqlTypes;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    public abstract class PostgreSqlDataProvider<TTargetEntity> : PostgreSqlDataProvider where TTargetEntity : IEntity
    {
        public override Type EntityType => typeof(TTargetEntity);
    }

    public abstract partial class PostgreSqlDataProvider : DataProvider<NpgsqlConnection, NpgsqlParameter>
    {
        public override IDataParameter GenerateParameter(KeyValuePair<string, object> data)
        {
            var value = data.Value;

            var result = new NpgsqlParameter { Value = value, ParameterName = data.Key.Remove(" ") };

            if (value is DateTime)
            {
                result.DbType = DbType.DateTime2;
                result.NpgsqlDbType = NpgsqlDbType.Date;
            }

            return result;
        }

        public override string GenerateSelectCommand(IDatabaseQuery iquery)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue && query.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var r = new StringBuilder("SELECT");

            r.AppendLine($" {GetFields()} FROM {GetTables()}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine(GenerateSort(query).WithPrefix(" ORDER BY "));
            r.AppendLine(query.TakeTop.ToStringOrEmpty().WithPrefix(" LIMIT "));

            return r.ToString();
        }

        public override string GenerateWhere(DatabaseQuery query)
        {
            var r = new StringBuilder();

            if (SoftDeleteAttribute.RequiresSoftdeleteQuery(query.EntityType))
                query.Criteria.Add(new Criterion("IsMarkedSoftDeleted", false));

            r.Append($" WHERE { query.Column("Id")} IS NOT NULL");

            var whereGenerator = new PostgreSqlCriterionGenerator(query);
            var temp = new List<ICriterion>();
            foreach (var c in query.Criteria)
                if (c.PropertyName == "ID") temp.Add(c);

            query.Criteria.RemoveAll(x => x.PropertyName == "ID");

            foreach (var c in temp)
                query.Criteria.Add(new Criterion("Id", c.FilterFunction, c.Value));

            foreach (var c in query.Criteria)
                r.Append(whereGenerator.Generate(c).WithPrefix(" AND "));

            return r.ToString();
        }
    }
}
