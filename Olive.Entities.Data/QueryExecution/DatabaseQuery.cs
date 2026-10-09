namespace Olive.Entities.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    partial class Database
    {
        public IDatabaseQuery Of(Type type) => new DatabaseQuery(type);
        public IDatabaseQuery<TEntity> Of<TEntity>() where TEntity : IEntity => new DatabaseQuery<TEntity>();
    }

    public partial class DatabaseQuery : IDatabaseQuery
    {
        Dictionary<string, AssociationInclusion> include = new Dictionary<string, AssociationInclusion>();
        readonly ICache Cache;
        public IDataProvider Provider { get; }
        public Type EntityType { get; private set; }
        public List<ICriterion> Criteria { get; } = new List<ICriterion>();
        public IEnumerable<AssociationInclusion> Include => include.Values.ToArray();
        public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public int PageStartIndex { get; set; }
        public int? PageSize { get; set; }
        public int? TakeTop { get; set; }

        public string[] Columns { get; set; } = new string[0];

        internal DatabaseQuery(Type entityType)
        {
            if (entityType == null) throw new ArgumentNullException(nameof(entityType));

            if (!entityType.IsA<IEntity>())
                throw new ArgumentException(entityType.Name + " is not an IEntity.");

            EntityType = entityType;
            Provider = Context.Current.Database().GetProvider(entityType);
            Cache = Context.Current.Database().Cache;
        }

        public string Column(string propertyName, string alias = null)
        {
            var result = MapColumn(propertyName);

            if (alias.IsEmpty()) return result;
            return AliasPrefix + alias + "." + result.Split('.').Last();
        }

        IDatabaseQuery IDatabaseQuery.Where(params ICriterion[] criteria)
        {
            Criteria.AddRange(criteria);
            return this;
        }

        IDatabaseQuery IDatabaseQuery.Include(string associations)
        {
            var immediateAssociation = associations.Split('.').First();
            var nestedAssociations = associations.Split('.').ExceptFirst().ToString(".");

            var property = EntityType.GetProperty(immediateAssociation)
                ?? throw new Exception(EntityType.Name + " does not have a property named " + immediateAssociation);

            if (!property.PropertyType.IsA<IEntity>())
                throw new Exception(EntityType.Name + "." + immediateAssociation + " is not an Entity type.");

            if (!include.ContainsKey(immediateAssociation))
                include.Add(immediateAssociation, AssociationInclusion.Create(property));

            if (nestedAssociations.HasValue())
                include[immediateAssociation].IncludeNestedAssociation(nestedAssociations);

            // TODO: Support one-to-many too
            return this;
        }

        IDatabaseQuery IDatabaseQuery.Select(params string[] colunms)
        {
            Columns = colunms.Trim().ToArray();
            return this;
        }

        IDatabaseQuery IDatabaseQuery.Include(IEnumerable<string> associations)
        {
            foreach (var item in associations)
                ((IDatabaseQuery)this).Include(item);

            return this;
        }

        IDatabaseQuery IDatabaseQuery.Top(int rows)
        {
            TakeTop = rows;
            return this;
        }

        IDatabaseQuery IDatabaseQuery.OrderBy(string property) => this.OrderBy(property, descending: false);

        Task<IEntity> IDatabaseQuery.WithMin(string property) =>
               this.OrderBy(property).FirstOrDefault();

        Task<IEntity> IDatabaseQuery.WithMax(string property) =>
               this.OrderBy(property, descending: true).FirstOrDefault();

        public IDatabaseQuery CloneFor(Type type)
        {
            var result = new DatabaseQuery(type)
            {
                PageStartIndex = PageStartIndex,
                TakeTop = TakeTop,
                PageSize = PageSize
            };

            result.Criteria.AddRange(Criteria);
            result.include.Add(include);
            result.Parameters.Add(Parameters);
            result.AliasPrefix = AliasPrefix;

            return result;
        }

        /// <summary>
        /// Returns a copy of this query without its sort, if the sort doesn't affect which rows are returned.
        /// Used when this query is embedded as an IN (...) subquery, where ORDER BY is meaningless
        /// and rejected by SQL Server unless TOP or OFFSET is also specified.
        /// </summary>
        internal DatabaseQuery WithoutRedundantSort()
        {
            if (OrderByParts.None() || TakeTop.HasValue || PageSize.HasValue) return this;
            return (DatabaseQuery)CloneFor(EntityType);
        }
    }
}