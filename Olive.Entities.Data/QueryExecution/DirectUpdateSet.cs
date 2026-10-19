namespace Olive.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>How a <see cref="DirectAssignment"/> computes the new value of its column.</summary>
    public enum DirectAssignmentKind
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

    /// <summary>One column of a <see cref="DirectOperations.DirectUpdate{TEntity}"/>.</summary>
    public class DirectAssignment
    {
        /// <summary>The entity property being set, as written in the expression (e.g. "LogId").</summary>
        public string Property { get; internal set; }

        /// <summary>How the new value is computed.</summary>
        public DirectAssignmentKind Kind { get; internal set; }

        /// <summary>The value, for every kind but <see cref="DirectAssignmentKind.Column"/>.</summary>
        public object Value { get; internal set; }

        /// <summary>The property whose column is copied, for <see cref="DirectAssignmentKind.Column"/>.</summary>
        public string SourceProperty { get; internal set; }
    }

    /// <summary>
    /// The SET part of <see cref="DirectOperations.DirectUpdate{TEntity}"/> and
    /// <see cref="DirectOperations.DirectClaim{TEntity}"/>: what each matching row's columns become. Every
    /// assignment is evaluated against the row as it is in the database, in the one statement that updates it,
    /// so two concurrent updates of the same rows cannot lose each other's work the way a load-and-save would.
    /// The values are not validated.
    /// </summary>
    public class DirectUpdateSet<TEntity> where TEntity : IEntity
    {
        internal readonly List<DirectAssignment> Assignments = new List<DirectAssignment>();

        DirectUpdateSet<TEntity> Add(LambdaExpression property, DirectAssignmentKind kind, object value, LambdaExpression source = null)
        {
            Assignments.Add(new DirectAssignment
            {
                Property = property.Body.GetPropertyPath(),
                Kind = kind,
                Value = value,
                SourceProperty = source?.Body.GetPropertyPath()
            });

            return this;
        }

        /// <summary>Sets the column to the value. For an association, set its Id property.</summary>
        public DirectUpdateSet<TEntity> Set<TValue>(Expression<Func<TEntity, TValue>> property, TValue value) =>
            Add(property, DirectAssignmentKind.Value, value);

        /// <summary>Sets the column to another column of the same row.</summary>
        public DirectUpdateSet<TEntity> SetFrom<TValue>(Expression<Func<TEntity, TValue>> property, Expression<Func<TEntity, TValue>> source) =>
            Add(property, DirectAssignmentKind.Column, null, source);

        /// <summary>Adds the amount to the column's own value.</summary>
        public DirectUpdateSet<TEntity> Increment<TValue>(Expression<Func<TEntity, TValue>> property, TValue by) =>
            Add(property, DirectAssignmentKind.Increment, by);

        /// <summary>Sets the column to the value only where it has none: null, or for text also empty.</summary>
        public DirectUpdateSet<TEntity> SetIfEmpty<TValue>(Expression<Func<TEntity, TValue>> property, TValue value) =>
            Add(property, DirectAssignmentKind.IfEmpty, value);

        /// <summary>Sets the column to the value only where it is null or later than the value.</summary>
        public DirectUpdateSet<TEntity> SetIfEarlier<TValue>(Expression<Func<TEntity, TValue>> property, TValue value) =>
            Add(property, DirectAssignmentKind.IfEarlier, value);
    }
}
