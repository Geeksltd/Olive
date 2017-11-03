using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides a DataProvider for accessing data from the database using ADO.NET based on the SqlClient provider.
    /// </summary>
    public abstract partial class SqlDataProvider : DataProvider<SqlConnection, SqlParameter>
    {
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

        public abstract string GetFields();

        public abstract string GetTables();

        public abstract IEntity Parse(IDataReader reader);

        public override async Task<IEntity> Get(object id)
        {
            var command = $"SELECT {GetFields()} FROM {GetTables()} WHERE {MapColumn("ID")} = @ID";

            using (var reader = await ExecuteReader(command, CommandType.Text, CreateParameter("ID", id)))
            {
                var result = new List<IEntity>();

                if (reader.Read()) return Parse(reader);
                else throw new DataException($"There is no record with the the ID of '{id}'.");
            }
        }

        protected async Task<IDataReader> ExecuteGetListReader(IDatabaseQuery query)
        {
            var command = GenerateSelectCommand(query);
            return await ExecuteReader(command, CommandType.Text, GenerateParameters(query.Parameters));
        }

        public override async Task<IEnumerable<IEntity>> GetList(IDatabaseQuery query)
        {
            using (var reader = await ExecuteGetListReader(query))
            {
                var result = new List<IEntity>();
                while (reader.Read()) result.Add(Parse(reader));
                return result;
            }
        }

        public override DirectDatabaseCriterion GetAssociationInclusionCriteria(IDatabaseQuery query,
            PropertyInfo association)
        {
            var whereClause = GenerateAssociationLoadingCriteria(query, association);
            return new DirectDatabaseCriterion(whereClause) { Parameters = query.Parameters };
        }

        public override async Task<int> Count(IDatabaseQuery query)
        {
            var command = GenerateCountCommand(query);
            return (int)await ExecuteScalar(command, CommandType.Text, GenerateParameters(query.Parameters));
        }

        public override Task<object> Aggregate(IDatabaseQuery query, AggregateFunction function, string propertyName)
        {
            var command = GenerateAggregateQuery(query, function, propertyName);
            return ExecuteScalar(command, CommandType.Text, GenerateParameters(query.Parameters));
        }

        public string GenerateAggregateQuery(IDatabaseQuery query, AggregateFunction function, string propertyName)
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

            return $"SELECT {sqlFunction}({columnValueExpression}) FROM {GetTables()}" +
                GenerateWhere((DatabaseQuery)query);
        }

        string GenerateAssociationLoadingCriteria(IDatabaseQuery iquery, PropertyInfo association)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue && query.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var r = new StringBuilder();

            r.Append($"ID IN (");
            r.Append("SELECT ");
            r.Append(query.TakeTop.ToStringOrEmpty().WithPrefix(" TOP "));
            r.AppendLine($" {association.Name} FROM {GetTables()}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine((query.PageSize.HasValue ? GenerateSort(query) : string.Empty).WithPrefix(" ORDER BY "));
            r.Append(")");

            return r.ToString();
        }

        public string GenerateSelectCommand(IDatabaseQuery iquery)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue && query.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var r = new StringBuilder("SELECT");

            r.Append(query.TakeTop.ToStringOrEmpty().WithPrefix(" TOP "));
            r.AppendLine($" {GetFields()} FROM {GetTables()}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine(GenerateSort(query).WithPrefix(" ORDER BY "));

            return r.ToString();
        }

        public string GenerateCountCommand(IDatabaseQuery iquery)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue)
                throw new ArgumentException("PageSize cannot be used for Count().");

            if (query.TakeTop.HasValue)
                throw new ArgumentException("TakeTop cannot be used for Count().");

            return $"SELECT Count(*) FROM {GetTables()} {GenerateWhere(query)}";
        }

        string GenerateWhere(DatabaseQuery query)
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

        string GenerateSort(DatabaseQuery query)
        {
            var parts = new List<string>();

            parts.AddRange(query.OrderByParts.Select(p => query.Column(p.Property) + " DESC".OnlyWhen(p.Descending)));

            var offset = string.Empty;
            if (query.PageSize > 0)
                offset = $" OFFSET {query.PageStartIndex} ROWS FETCH NEXT {query.PageSize} ROWS ONLY";

            return parts.ToString(", ") + offset;
        }
    }
}
