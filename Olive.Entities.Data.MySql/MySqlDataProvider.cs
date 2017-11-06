namespace Olive.Entities.Data
{
    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    public abstract partial class MySqlDataProvider : DataProvider<MySqlConnection, MySqlParameter>
    {
        public override IDataParameter GenerateParameter(KeyValuePair<string, object> data)
        {
            var value = data.Value;

            var result = new MySqlParameter { Value = value, ParameterName = data.Key.Remove(" ") };

            if (value is DateTime)
            {
                result.DbType = DbType.DateTime2;
                result.MySqlDbType = MySqlDbType.DateTime;
            }

            return result;
        }

        //public abstract string GetFields();

        //public abstract string GetTables();

        //public abstract string GetSelectCommand();

        //public abstract IEntity Parse(IDataReader reader);

        //public override async Task<IEntity> Get(object id)
        //{
        //    var command = $"SELECT {GetFields()} FROM {GetTables()} WHERE {MapColumn("ID")} = @ID";

        //    using (var reader = await ExecuteReader(command, CommandType.Text, CreateParameter("ID", id)))
        //    {
        //        var result = new List<IEntity>();

        //        if (reader.Read()) return Parse(reader);
        //        else throw new DataException($"There is no record with the the ID of '{id}'.");
        //    }
        //}

        //protected async Task<IDataReader> ExecuteGetListReader(IDatabaseQuery query)
        //{
        //    var command = GenerateSelectCommand(query);
        //    return await ExecuteReader(command, CommandType.Text, GenerateParameters(query.Parameters));
        //}

        //public override async Task<IEnumerable<IEntity>> GetList(IDatabaseQuery query)
        //{
        //    using (var reader = await ExecuteGetListReader(query))
        //    {
        //        var result = new List<IEntity>();
        //        while (reader.Read()) result.Add(Parse(reader));
        //        return result;
        //    }
        //}

        //public override async Task<Int64> Count(IDatabaseQuery query)
        //{
        //    var command = GenerateCountCommand(query);
        //    return (Int64)await ExecuteScalar(command, CommandType.Text, GenerateParameters(query.Parameters));
        //}

        //public override Task<object> Aggregate(IDatabaseQuery query, AggregateFunction function, string propertyName)
        //{
        //    var command = GenerateAggregateQuery(query, function, propertyName);
        //    return ExecuteScalar(command, CommandType.Text, GenerateParameters(query.Parameters));
        //}

        //public string GenerateAggregateQuery(IDatabaseQuery query, AggregateFunction function, string propertyName)
        //{
        //    var sqlFunction = function.ToString();

        //    var columnValueExpression = MapColumn(propertyName);

        //    if (function == AggregateFunction.Average)
        //    {
        //        sqlFunction = "AVG";

        //        var propertyType = query.EntityType.GetProperty(propertyName).PropertyType;

        //        if (propertyType == typeof(int) || propertyType == typeof(int?))
        //            columnValueExpression = $"CAST({columnValueExpression} AS decimal)";
        //    }

        //    return $"SELECT {sqlFunction}({columnValueExpression}) FROM {GetTables()}" +
        //        GenerateWhere((DatabaseQuery)query);
        //}

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

        //public string GenerateCountCommand(IDatabaseQuery iquery)
        //{
        //    var query = (DatabaseQuery)iquery;

        //    if (query.PageSize.HasValue)
        //        throw new ArgumentException("PageSize cannot be used for Count().");

        //    if (query.TakeTop.HasValue)
        //        throw new ArgumentException("TakeTop cannot be used for Count().");

        //    return $"SELECT Count(*) FROM {GetTables()} {GenerateWhere(query)}";
        //}

        public override string GenerateWhere(DatabaseQuery query)
        {
            var r = new StringBuilder();

            if (SoftDeleteAttribute.RequiresSoftdeleteQuery(query.EntityType))
                query.Criteria.Add(new Criterion("IsMarkedSoftDeleted", false));

            r.Append($" WHERE { query.Column("ID")} IS NOT NULL");

            var whereGenerator = new MySqlCriterionGenerator(query);
            foreach (var c in query.Criteria)
                r.Append(whereGenerator.Generate(c).WithPrefix(" AND "));

            return r.ToString();
        }

        //string GenerateSort(DatabaseQuery query)
        //{
        //    var parts = new List<string>();

        //    parts.AddRange(query.OrderByParts.Select(p => query.Column(p.Property) + " DESC".OnlyWhen(p.Descending)));

        //    var offset = string.Empty;
        //    if (query.PageSize > 0)
        //        offset = $" OFFSET {query.PageStartIndex} ROWS FETCH NEXT {query.PageSize} ROWS ONLY";

        //    return parts.ToString(", ") + offset;
        //}
    }
}
