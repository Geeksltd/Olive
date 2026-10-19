using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    partial class DataProvider : IDirectDataProvider
    {
        // ================================================================ direct operations

        public async Task<int> DirectDelete(IDatabaseQuery iquery)
        {
            var query = ForDirect(iquery, "DirectDelete");

            if (MetaData.IsSoftDeleteEnabled)
                throw new NotSupportedException($"DirectDelete() would hard-delete {EntityType.Name}, which is soft-deleted. Use DirectUpdate() to mark it.");

            var command = DirectGenerator().GenerateDirectDeleteCommand(query, DirectTarget());

            try { return await ExecuteNonQuery(command, CommandType.Text, GenerateParameters(query.Parameters)); }
            finally { InvalidateCache(); }
        }

        public async Task<int> DirectUpdate(IDatabaseQuery iquery, IEnumerable<DirectAssignment> assignments)
        {
            var query = ForDirect(iquery, "DirectUpdate");
            var values = new Dictionary<string, object>();
            var set = GenerateAssignments(assignments, values);

            var command = DirectGenerator().GenerateDirectUpdateCommand(query, DirectTarget(), set);

            try { return await ExecuteNonQuery(command, CommandType.Text, GenerateParameters(Merge(query, values))); }
            finally { InvalidateCache(); }
        }

        public async Task<IEntity[]> DirectClaim(IDatabaseQuery iquery, IEnumerable<DirectAssignment> assignments)
        {
            var query = ForDirect(iquery, "DirectClaim");
            var values = new Dictionary<string, object>();
            var set = GenerateAssignments(assignments, values);

            var command = DirectGenerator().GenerateDirectClaimCommand(query, DirectTarget(), set);

            DataTable claimed;
            try
            {
                using (new DatabaseContext(ConnectionString))
                    claimed = await Access.ExecuteQuery(command, CommandType.Text, GenerateParameters(Merge(query, values)));
            }
            finally { InvalidateCache(); }

            var ids = claimed.Rows.Cast<DataRow>().Select(r => r[0]).Where(x => x != DBNull.Value).ToArray();
            if (ids.None()) return new IEntity[0];

            // Read back through the ORM, so the caller gets entities as GetList gives them, in the query's order.
            var reload = new DatabaseQuery(EntityType);
            reload.Criteria.Add(new Criterion(PropertyData.DEFAULT_ID_COLUMN, FilterFunction.In, InList(ids)));
            reload.OrderByParts.AddRange(query.OrderByParts);

            return await reload.GetList();
        }

        /// <summary>Clears the type from the cache now, and again when the open transaction ends, as a save
        /// does: a list read inside the transaction would otherwise stay cached with rows it rolled back.</summary>
        void InvalidateCache()
        {
            var type = EntityType;
            var cache = Database.Cache;

            cache.Remove(type, invalidateCachedReferences: true);

            if (System.Transactions.Transaction.Current != null)
                System.Transactions.Transaction.Current.TransactionCompleted += (s, e) => cache.Remove(type, invalidateCachedReferences: true);

            DbTransactionScope.Root?.OnTransactionCompleted(() => cache.Remove(type, invalidateCachedReferences: true));
        }

        /// <summary>The query's own parameters, which generating its WHERE clause adds, with the SET values.</summary>
        static Dictionary<string, object> Merge(DatabaseQuery query, Dictionary<string, object> values)
        {
            var result = new Dictionary<string, object>(query.Parameters);
            foreach (var value in values) result[value.Key] = value.Value;
            return result;
        }

        DatabaseQuery ForDirect(IDatabaseQuery iquery, string operation)
        {
            var query = iquery as DatabaseQuery ?? throw new NotSupportedException($"{operation}() needs an Olive DatabaseQuery.");

            if (IsInheritance())
                throw new NotSupportedException($"{operation}() does not support {EntityType.Name}: its rows span the tables of an inheritance hierarchy.");

            if (query.PageSize.HasValue)
                throw new ArgumentException($"{operation}() does not take paging. Use Top() to limit the rows.");

            return query;
        }

        bool IsInheritance() => MetaData.BaseClassesInOrder.Any() || MetaData.DrivedClasses.Any();

        SqlCommandGenerator DirectGenerator() =>
            SqlCommandGenerator as SqlCommandGenerator ??
            throw new NotSupportedException($"{SqlCommandGenerator.GetType().Name} does not support direct operations.");

        DirectCommandTarget DirectTarget()
        {
            var generator = DirectGenerator();

            return new DirectCommandTarget
            {
                Table = generator.GetFullTableName(MetaData),
                Tables = GetTables(),
                Alias = generator.SafeAlias(MetaData.TableAlias),
                IdColumn = generator.SafeId(MetaData.IdColumnName)
            };
        }

        /// <summary>The SET list, with each value added to <paramref name="parameters"/>. Values are converted
        /// the way a save converts them: set on a blank entity and read back through the column's accessor.</summary>
        string GenerateAssignments(IEnumerable<DirectAssignment> assignments, Dictionary<string, object> parameters)
        {
            var generator = DirectGenerator();
            var result = new List<string>();

            foreach (var assignment in assignments)
            {
                var property = DirectProperty(assignment.Property);
                var column = generator.SafeId(property.Name);

                if (assignment.Kind == DirectAssignmentKind.Column)
                {
                    result.Add($"{column} = {generator.SafeId(DirectProperty(assignment.SourceProperty).Name)}");
                    continue;
                }

                var name = "@directSet" + parameters.Count;
                parameters[name] = ToColumnValue(property, assignment.Value) ?? DBNull.Value;

                switch (assignment.Kind)
                {
                    case DirectAssignmentKind.Value:
                        result.Add($"{column} = {name}"); break;
                    case DirectAssignmentKind.Increment:
                        result.Add($"{column} = {column} + {name}"); break;
                    case DirectAssignmentKind.IfEmpty:
                        result.Add(property.PropertyInfo?.PropertyType == typeof(string)
                            ? $"{column} = COALESCE(NULLIF({column}, ''), {name})"
                            : $"{column} = COALESCE({column}, {name})");
                        break;
                    case DirectAssignmentKind.IfEarlier:
                        result.Add($"{column} = CASE WHEN {column} IS NULL OR {name} < {column} THEN {name} ELSE {column} END"); break;
                    default: throw new NotSupportedException(assignment.Kind.ToString());
                }
            }

            return result.ToString(", ");
        }

        /// <summary>The column behind a property as an expression names it: the property itself, or for an
        /// association, its Id property.</summary>
        IPropertyData DirectProperty(string name)
        {
            var result = MetaData.UserDefienedProperties.FirstOrDefault(p => p.PropertyInfo?.Name == name || p.Name == name)
                ?? throw new ArgumentException($"{EntityType.Name}.{name} is not a column that can be set.");

            if (result.IsAutoNumber || result.IsComputed || result.IsCustomPrimaryKey)
                throw new ArgumentException($"{EntityType.Name}.{name} cannot be set: it is a key or computed by the database.");

            return result;
        }

        object ToColumnValue(IPropertyData property, object value)
        {
            if (value == null) return null;
            if (value is IEntity entity) value = entity.GetId();

            var blank = (IEntity)Activator.CreateInstance(EntityType);
            property.PropertyInfo.SetValue(blank, value);
            return property.Accessor.Get(blank);
        }

        static string InList(IEnumerable<object> ids) =>
            "(" + ids.Select(x => x is string || x is Guid ? "'" + x.ToString().Replace("'", "''") + "'" : x.ToString()).ToString(", ") + ")";

        // ================================================================ direct insert

        public async Task DirectInsert(IEntity[] entities)
        {
            if (entities.None()) return;

            if (entities.Any(e => !e.IsNew || e.GetType() != EntityType))
                throw new ArgumentException($"DirectInsert() takes new {EntityType.Name} instances only.");

            try
            {
                if (CanBulkCopy()) await BulkCopy(entities);
                else
                    foreach (var item in entities)
                        await Database.Save(item, SaveBehaviour.BypassAll);
            }
            finally { InvalidateCache(); }
        }

        /// <summary>Whether the database has a bulk copy and this table allows it: a single table, no
        /// auto-number to read back, no Blob to store, and either no open transaction or Olive's own
        /// DbTransactionScope, whose connection the copy joins.</summary>
        bool CanBulkCopy() =>
            SqlCommandGenerator is IBulkCopyCommandGenerator &&
            !IsInheritance() &&
            !MetaData.HasAutoNumber &&
            !MetaData.Properties.Any(p => p.PropertyInfo?.PropertyType.IsA<Blob>() == true) &&
            (!Database.AnyOpenTransaction() || DbTransactionScope.Root != null);

        async Task BulkCopy(IEntity[] entities)
        {
            var properties = MetaData.GetPropertiesForInsert().ToArray();

            var rows = new DataTable();
            foreach (var property in properties) rows.Columns.Add(property.Name, typeof(object));

            foreach (var entity in entities)
                rows.Rows.Add(properties.Select(p => p.Accessor.Get(entity) ?? DBNull.Value).ToArray());

            var copier = (IBulkCopyCommandGenerator)SqlCommandGenerator;
            var table = DirectGenerator().GetFullTableName(MetaData);

            using (new DatabaseContext(ConnectionString))
            {
                var scope = DbTransactionScope.Root;
                if (scope != null)
                {
                    var connection = (System.Data.Common.DbConnection)await scope.GetDbConnection();
                    await copier.BulkCopy(connection, await scope.GetDbTransaction(), table, rows);
                }
                else
                    using (var connection = (System.Data.Common.DbConnection)await Access.CreateConnection())
                    {
                        if (connection.State != ConnectionState.Open) await connection.OpenAsync();
                        await copier.BulkCopy(connection, null, table, rows);
                    }
            }

            foreach (var entity in entities)
            {
                Entity.Services.SetSaved(entity);
                Entity.Services.SetOriginalId(entity);
            }
        }
    }
}
