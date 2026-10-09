namespace Olive.Entities.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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

        internal static void ThrowIfCalculated(PropertyInfo property, string operation)
        {
            if (CalculatedAttribute.IsCalculated(property))
                throw new NotSupportedException(
                    $"Cannot use calculated property '{property.DeclaringType.Name}.{property.Name}' in {operation}(). " +
                    "Properties marked with [Calculated] do not exist in the database.");
        }

        internal void ValidatePropertyIsNotCalculated(string propertyPath, string operation)
        {
            if (propertyPath.IsEmpty()) return;

            var type = EntityType;
            foreach (var part in propertyPath.Split('.'))
            {
                var property = type.GetProperty(part);
                if (property == null) break;

                ThrowIfCalculated(property, operation);
                type = property.PropertyType;
            }
        }

        internal void AddWhereCriteria(IEnumerable<ICriterion> criteria)
        {
            var items = criteria.ToArray();

            void validate(ICriterion criterion)
            {
                if (criterion is BinaryCriterion binary)
                {
                    validate(binary.Left);
                    validate(binary.Right);
                }
                else if (!(criterion is DirectDatabaseCriterion))
                {
                    ValidatePropertyIsNotCalculated(criterion.PropertyName, "Where");

                    if (criterion is DynamicValueCriterion && criterion.Value is string otherProperty)
                        ValidatePropertyIsNotCalculated(otherProperty, "Where");
                }
            }

            foreach (var criterion in items) validate(criterion);
            Criteria.AddRange(items);
        }

        IDatabaseQuery IDatabaseQuery.Where(params ICriterion[] criteria)
        {
            AddWhereCriteria(criteria);
            return this;
        }

        IDatabaseQuery IDatabaseQuery.Include(string associations)
        {
            ValidatePropertyIsNotCalculated(associations, "Include");

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
    }
}