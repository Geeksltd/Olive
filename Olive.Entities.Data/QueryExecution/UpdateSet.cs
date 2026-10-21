namespace Olive.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>How a <see cref="ColumnAssignment"/> computes the new value of its column.</summary>
    public enum ColumnAssignmentKind
    {
        /// <summary>The given value.</summary>
        Value,

        /// <summary>The value of another column of the same row.</summary>
        Column,

        /// <summary>The column's own value plus the given amount.</summary>
        Increment,

        /// <summary>The given value, only where the column is null (or, for text, empty).</summary>
        IfEmpty,

        /// <summary>The given value, only where the column is null or later than it.</summary>
        IfEarlier
    }

    /// <summary>One column of a <see cref="WriteOperations.Update{TEntity}"/>.</summary>
    public class ColumnAssignment
    {
        /// <summary>The entity property being set, as written in the expression (e.g. "LogId").</summary>
        public string Property { get; internal set; }

        /// <summary>How the new value is computed.</summary>
        public ColumnAssignmentKind Kind { get; internal set; }

        /// <summary>The value, for every kind but <see cref="ColumnAssignmentKind.Column"/>.</summary>
        public object Value { get; internal set; }

        /// <summary>The property whose column is copied, for <see cref="ColumnAssignmentKind.Column"/>.</summary>
        public string SourceProperty { get; internal set; }

        /// <summary>Makes the same change to an entity in memory, for an update through Olive's entity pipeline.</summary>
        internal Action<IEntity> Apply { get; set; }
    }

    /// <summary>
    /// The SET part of <see cref="WriteOperations.Update{TEntity}"/> and <see cref="WriteOperations.Claim{TEntity}"/>:
    /// what each matching row's columns become.
    /// <para>
    /// With bypassEntityPipeline, every assignment is evaluated against the row as it is in the database, in the one
    /// statement that updates it, so two concurrent updates of the same rows cannot lose each other's work, and the
    /// values are not validated. Without it, each entity is loaded, changed the same way in memory and saved, as
    /// <c>Database.Update</c> does.
    /// </para>
    /// </summary>
    public class UpdateSet<TEntity> where TEntity : IEntity
    {
        internal readonly List<ColumnAssignment> Assignments = new List<ColumnAssignment>();

        /// <param name="compute">The new value, from the entity and a function that reads the property's current
        /// value. Only the kinds that depend on it call that function: on an association, reading loads the record.</param>
        UpdateSet<TEntity> Add<TValue>(Expression<Func<TEntity, TValue>> property, ColumnAssignmentKind kind, object value,
            Func<TEntity, Func<TValue>, TValue> compute, LambdaExpression source = null)
        {
            var read = new Lazy<Func<TEntity, TValue>>(property.Compile);
            var write = new Lazy<Action<TEntity, TValue>>(() => Setter(property));

            Assignments.Add(new ColumnAssignment
            {
                Property = property.Body.GetPropertyPath(),
                Kind = kind,
                Value = value,
                SourceProperty = source?.Body.GetPropertyPath(),
                Apply = item =>
                {
                    var entity = (TEntity)item;
                    write.Value(entity, compute(entity, () => read.Value(entity)));
                }
            });

            return this;
        }

        /// <summary>Sets the column to the value. For an association, set its Id property.</summary>
        public UpdateSet<TEntity> Set<TValue>(Expression<Func<TEntity, TValue>> property, TValue value) =>
            Add(property, ColumnAssignmentKind.Value, value, (entity, read) => value);

        /// <summary>Sets the column to another column of the same row.</summary>
        public UpdateSet<TEntity> SetFrom<TValue>(Expression<Func<TEntity, TValue>> property, Expression<Func<TEntity, TValue>> source)
        {
            var copy = new Lazy<Func<TEntity, TValue>>(source.Compile);
            return Add(property, ColumnAssignmentKind.Column, null, (entity, read) => copy.Value(entity), source);
        }

        /// <summary>Adds the amount to the column's own value.</summary>
        public UpdateSet<TEntity> Increment<TValue>(Expression<Func<TEntity, TValue>> property, TValue by)
        {
            var add = new Lazy<Func<TValue, TValue>>(() => Adder(by));
            return Add(property, ColumnAssignmentKind.Increment, by, (entity, read) => add.Value(read()));
        }

        /// <summary>Sets the column to the value only where it has none: null, or for text also empty.</summary>
        public UpdateSet<TEntity> SetIfEmpty<TValue>(Expression<Func<TEntity, TValue>> property, TValue value) =>
            Add(property, ColumnAssignmentKind.IfEmpty, value,
                (entity, read) =>
                {
                    var current = read();
                    return current == null || (current is string text && text.Length == 0) ? value : current;
                });

        /// <summary>Sets the column to the value only where it is null or later than the value.</summary>
        public UpdateSet<TEntity> SetIfEarlier<TValue>(Expression<Func<TEntity, TValue>> property, TValue value) =>
            Add(property, ColumnAssignmentKind.IfEarlier, value,
                (entity, read) =>
                {
                    var current = read();
                    return value != null && (current == null || Comparer<TValue>.Default.Compare(value, current) < 0) ? value : current;
                });

        /// <summary>Adds the amount to a value, as the database's + does: null plus anything is null. Expression
        /// trees have no + for byte and sbyte (C# widens them to int), so those are added as int.</summary>
        static Func<TValue, TValue> Adder<TValue>(TValue by)
        {
            var current = Expression.Parameter(typeof(TValue));
            var type = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
            var widen = type == typeof(byte) || type == typeof(sbyte) ?
                (Nullable.GetUnderlyingType(typeof(TValue)) == null ? typeof(int) : typeof(int?)) : typeof(TValue);

            Expression Widen(Expression x) => x.Type == widen ? x : Expression.Convert(x, widen);

            var sum = Expression.Add(Widen(current), Widen(Expression.Constant(by, typeof(TValue))));
            var body = sum.Type == typeof(TValue) ? (Expression)sum : Expression.Convert(sum, typeof(TValue));

            return Expression.Lambda<Func<TValue, TValue>>(body, current).Compile();
        }

        /// <summary>Assigns the property the expression names, which must be a settable property of the entity itself.</summary>
        static Action<TEntity, TValue> Setter<TValue>(Expression<Func<TEntity, TValue>> property)
        {
            var body = property.Body is UnaryExpression convert && convert.NodeType == ExpressionType.Convert ? convert.Operand : property.Body;

            if (!(body is MemberExpression member) || member.Expression != property.Parameters[0] ||
                !(member.Member is PropertyInfo info) || !info.CanWrite)
                throw new ArgumentException($"{property.Body} is not a settable property of {typeof(TEntity).Name}.");

            var value = Expression.Parameter(typeof(TValue));
            var assign = Expression.Assign(member, value.Type == member.Type ? (Expression)value : Expression.Convert(value, member.Type));

            return Expression.Lambda<Action<TEntity, TValue>>(assign, property.Parameters[0], value).Compile();
        }
    }
}
