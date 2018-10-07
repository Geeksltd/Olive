﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Provides a DataProvider for accessing data from the database using ADO.NET.
    /// </summary>
    public abstract class DataProvider<TConnection, TDataParameter> : IDataProvider
        where TConnection : DbConnection, new()
        where TDataParameter : IDbDataParameter, new()
    {
        string connectionString, connectionStringKey = "Default";
        readonly static string[] ExtractIdsSeparator = new[] { "</Id>", "<Id>", "," };

        public IDataAccess Access { get; } = new DataAccess<TConnection>();

        public static IDatabase Database => Context.Current.Database();
        protected ICache Cache;
        protected DataProvider(ICache cache)
        {
            Cache = cache;
        }

        public abstract string MapColumn(string propertyName);

        public virtual string MapSubquery(string path, string parent)
        {
            throw new NotSupportedException($"{GetType().Name} does not provide a sub-query mapping for '{path}'.");
        }

        public virtual async Task BulkInsert(IEntity[] entities, int batchSize)
        {
            foreach (var item in entities)
                await Database.Save(item, SaveBehaviour.BypassAll);
        }

        public async Task BulkUpdate(IEntity[] entities, int batchSize)
        {
            foreach (var item in entities)
                await Database.Save(item, SaveBehaviour.BypassAll);
        }

        public async Task<int> Count(IDatabaseQuery query)
        {
            var command = GenerateCountCommand(query);
            return Convert.ToInt32(await ExecuteScalar(command, CommandType.Text, GenerateParameters(query.Parameters)));
        }

        public static List<string> ExtractIds(string idsXml) =>
            idsXml.Split(ExtractIdsSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();

        public bool SupportValidationBypassing() => true;

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// </summary>
        public Task<int> ExecuteNonQuery(string command) => ExecuteNonQuery(command, CommandType.Text);

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// </summary>
        public async Task<int> ExecuteNonQuery(string command, CommandType commandType, params IDataParameter[] @params)
        {
            using (new DatabaseContext(ConnectionString))
                return await Access.ExecuteNonQuery(command, commandType, @params);
        }

        /// <summary>
        /// Executes the specified command text against the database connection of the context and builds an IDataReader.  Make sure you close the data reader after finishing the work.
        /// </summary>
        public async Task<IDataReader> ExecuteReader(string command, CommandType commandType, params IDataParameter[] @params)
        {
            using (new DatabaseContext(ConnectionString))
                return await Access.ExecuteReader(command, commandType, @params);
        }

        /// <summary>
        /// Executes the specified command text against the database connection of the context and returns the single value.
        /// </summary>
        public Task<object> ExecuteScalar(string command) => ExecuteScalar(command, CommandType.Text);

        /// <summary>
        /// Executes the specified command text against the database connection of the context and returns the single value.
        /// </summary>
        public async Task<object> ExecuteScalar(string command, CommandType commandType, params IDataParameter[] @params)
        {
            using (new DatabaseContext(ConnectionString))
                return await Access.ExecuteScalar(command, commandType, @params);
        }

        public IDictionary<string, Tuple<string, string>> GetUpdatedValues(IEntity original, IEntity updated)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            var result = new Dictionary<string, Tuple<string, string>>();

            var type = original.GetType();
            var propertyNames = type.GetProperties().Distinct().Select(p => p.Name.Trim()).ToArray();

            PropertyInfo getProperty(string name)
            {
                return type.GetProperties()
                    .Except(p => p.IsSpecialName || p.GetGetMethod().IsStatic)
                    .Where(p => p.GetSetMethod() != null && p.GetGetMethod().IsPublic)
                    .OrderByDescending(x => x.DeclaringType == type)
                    .FirstOrDefault(p => p.Name == name);
            }

            var dataProperties = propertyNames.Select(getProperty).ExceptNull()
                .Except(x => CalculatedAttribute.IsCalculated(x))
                .Where(x => LogEventsAttribute.ShouldLog(x))
                .ToArray();

            foreach (var p in dataProperties)
            {
                var propertyType = p.PropertyType;
                // Get the original value:
                string originalValue, updatedValue = null;
                if (propertyType == typeof(IList<Guid>))
                {
                    try
                    {
                        originalValue = (p.GetValue(original) as IList<Guid>).ToString(",");
                        if (updated != null)
                            updatedValue = (p.GetValue(updated) as IList<Guid>).ToString(",");
                    }
                    catch
                    {
                        // No logging is needed.
                        continue;
                    }
                }
                else if (propertyType.IsGenericType)
                {
                    try
                    {
                        originalValue = (p.GetValue(original) as IEnumerable<object>).ToString(", ");
                        if (updated != null)
                            updatedValue = (p.GetValue(updated) as IEnumerable<object>).ToString(", ");
                    }
                    catch
                    {
                        // No logging is needed.
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        originalValue = $"{p.GetValue(original)}";
                        if (updated != null)
                            updatedValue = $"{p.GetValue(updated)}";
                    }
                    catch
                    {
                        // No logging is needed
                        continue;
                    }
                }

                if (updated == null || originalValue != updatedValue)
                    if (result.LacksKey(p.Name))
                        result.Add(p.Name, new Tuple<string, string>(originalValue, updatedValue));
            }

            return result;
        }

        /// <summary>
        /// Creates a data parameter with the specified name and value.
        /// </summary>
        public IDataParameter CreateParameter(string parameterName, object value)
        {
            if (value == null) value = DBNull.Value;
            else if (value is Blob blob) value = blob.FileName;

            return new TDataParameter { ParameterName = parameterName.Remove(" "), Value = value };
        }

        /// <summary>
        /// Creates a data parameter with the specified name and value and type.
        /// </summary>
        public IDataParameter CreateParameter(string parameterName, object value, DbType columnType)
        {
            if (value == null) value = DBNull.Value;

            return new TDataParameter { ParameterName = parameterName.Remove(" "), Value = value, DbType = columnType };
        }

        /// <summary>
        /// Deletes the specified record.
        /// </summary>
        public abstract Task Delete(IEntity record);

        /// <summary>
        /// Gets the specified record by its type and ID.
        /// </summary>
        public async Task<IEntity> Get(object objectID)
        {
            var command = $"SELECT {GetFields()} FROM {GetTables()} WHERE {MapColumn("ID")} = @ID";

            using (var reader = await ExecuteReader(command, CommandType.Text, CreateParameter("ID", objectID)))
            {
                var result = new List<IEntity>();

                if (reader.Read()) return Parse(reader);
                else throw new DataException($"There is no record with the the ID of '{objectID}'.");
            }
        }

        /// <summary>
        /// Gets the list of specified records.
        /// </summary>        
        public virtual async Task<IEnumerable<IEntity>> GetList(IDatabaseQuery query)
        {
            using (var reader = await ExecuteGetListReader(query))
            {
                var result = new List<IEntity>();
                while (reader.Read()) result.Add(Parse(reader));
                return result;
            }
        }

        /// <summary>
        /// Reads the many to many relation.
        /// </summary>
        public abstract Task<IEnumerable<string>> ReadManyToManyRelation(IEntity instance, string property);

        /// <summary>
        /// Saves the specified record.
        /// </summary>
        public abstract Task Save(IEntity record);

        /// <summary>
        /// Generates data provider specific parameters for the specified data items.
        /// </summary>
        public IDataParameter[] GenerateParameters(Dictionary<string, object> parametersData) =>
            parametersData.Select(GenerateParameter).ToArray();

        /// <summary>
        /// Generates a data provider specific parameter for the specified data.
        /// </summary>
        public virtual IDataParameter GenerateParameter(KeyValuePair<string, object> data) =>
            new TDataParameter { Value = data.Value, ParameterName = data.Key.Remove(" ") };

        public Task<object> Aggregate(IDatabaseQuery query, AggregateFunction function, string propertyName)
        {
            var command = GenerateAggregateQuery(query, function, propertyName);
            return ExecuteScalar(command, CommandType.Text, GenerateParameters(query.Parameters));
        }

        #region Connection String

        /// <summary>
        /// Gets or sets the connection string key used for this data provider.
        /// </summary>
        public string ConnectionStringKey
        {
            get => connectionStringKey;
            set
            {
                if (value.HasValue()) LoadConnectionString(value);

                connectionStringKey = value;
            }
        }

        void LoadConnectionString(string key) => connectionString = Config.GetConnectionString(key);

        /// <summary>
        /// Gets or sets the connection string key used for this data provider.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (connectionString.HasValue()) return connectionString;

                if (connectionStringKey.HasValue())
                    LoadConnectionString(connectionStringKey);

                return connectionString;
            }
            set
            {
                connectionString = value;
            }
        }

        #endregion

        #region Common things in DataProvider classes
        public abstract Type EntityType { get; }

        public abstract string GetFields();

        public abstract string GetTables();

        public abstract IEntity Parse(IDataReader reader);

        public virtual string GenerateSelectCommand(IDatabaseQuery iquery)
        {
            return GenerateSelectCommand(iquery, GetFields());
        }

        public abstract string GenerateSelectCommand(IDatabaseQuery iquery, string fields);

        public async Task<IDataReader> ExecuteGetListReader(IDatabaseQuery query)
        {
            var command = GenerateSelectCommand(query);
            return await ExecuteReader(command, CommandType.Text, GenerateParameters(query.Parameters));
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

        public string GenerateCountCommand(IDatabaseQuery iquery)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue)
                throw new ArgumentException("PageSize cannot be used for Count().");

            if (query.TakeTop.HasValue)
                throw new ArgumentException("TakeTop cannot be used for Count().");

            return $"SELECT Count(*) FROM {GetTables()} {GenerateWhere(query)}";
        }

        public abstract string GenerateWhere(DatabaseQuery query);

        public virtual string GenerateSort(DatabaseQuery query)
        {
            var parts = new List<string>();

            parts.AddRange(query.OrderByParts.Select(p => query.Column(p.Property) + " DESC".OnlyWhen(p.Descending)));

            var offset = string.Empty;
            if (query.PageSize > 0)
                offset = $" OFFSET {query.PageStartIndex} ROWS FETCH NEXT {query.PageSize} ROWS ONLY";

            return parts.ToString(", ") + offset;
        }

        #endregion

        /// <summary>
        /// Returns a direct database criterion used to eager load associated objects.
        /// Gets the list of specified records.
        /// </summary>        
        public virtual DirectDatabaseCriterion GetAssociationInclusionCriteria(IDatabaseQuery masterQuery, PropertyInfo association)
        {
            var whereClause = GenerateAssociationLoadingCriteria((DatabaseQuery)masterQuery, association);

            return new DirectDatabaseCriterion(whereClause)
            {
                Parameters = masterQuery.Parameters
            };
        }

        string GenerateAssociationLoadingCriteria(DatabaseQuery masterQuery, PropertyInfo association)
        {
            if (masterQuery.PageSize.HasValue && masterQuery.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var masterProvider = masterQuery.Provider as DataProvider<TConnection, TDataParameter>;

            var uniqueItems = masterProvider.GenerateSelectCommand(masterQuery, masterQuery.Column(association.Name));

            return GenerateAssociationLoadingCriteria(MapColumn("ID"), uniqueItems, association);
        }

        protected virtual string GenerateAssociationLoadingCriteria(string id, string uniqueItems, PropertyInfo association)
        {
            return $"{id} IN ({uniqueItems})";
        }
    }
}