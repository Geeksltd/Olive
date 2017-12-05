namespace Olive.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Olive;
    using Olive.Entities;

    partial class Permission
    {
        internal static List<Permission> All = new List<Permission>();

        internal static Permission For(string type)
        {
            var result = All.FirstOrDefault(x => x.Type.MatchesRequestedType(type));
            if (result != null)
            {
                // Exact match:
                return result;
            }

            // Parent maybe?
            var match = All.Select(x => x.Type.Assembly).Distinct().SelectMany(x => x.ExportedTypes).Where(x => x.IsA<Entity>())
                .FirstOrDefault(x => x.MatchesRequestedType(type));

            while (match != null)
            {
                result = All.FirstOrDefault(x => x.Type == match);
                if (result != null) return result;

                match = match.BaseType;
            }

            return null;
        }

        static Permission GetOrCreate<T>()
        {
            var result = All.FirstOrDefault(p => p.Type == typeof(T));

            if (result == null)
            {
                result = new Permission { Type = typeof(T) };
                All.Add(result);
            }

            return result;
        }

        public static void AllowRead<T>(Func<bool> autohrizeWhen, IEnumerable<ICriterion> appendCriteria = null, Func<T, bool> postQueryFilter = null) where T : IEntity
        {
            var permission = GetOrCreate<T>();

            permission.AppendCriteria = appendCriteria?.ToList() ?? new List<ICriterion>();
            if (postQueryFilter != null) permission.PostQueryFilter = item => postQueryFilter((T)item);

            permission.AuthorizeReadWhen = autohrizeWhen;
            permission.CanRead = true;
        }

        public static void AllowRead<T>(Func<bool> autohrizeWhen, ICriterion appendCriteria) where T : IEntity
        {
            AllowRead<T>(autohrizeWhen, new[] { appendCriteria });
        }

        public static void AllowRead<T>(Func<bool> autohrizeWhen, Expression<Func<T, bool>> appendCriteria) where T : IEntity
        {
            AllowRead<T>(autohrizeWhen, Criterion.From<T>(appendCriteria));
        }

        public static void AllowSave<T>(Func<T, bool> autohrizeWhen) where T : IEntity
        {
            var permission = GetOrCreate<T>();

            if (autohrizeWhen != null)
                permission.AuthorizeSaveWhen = x => autohrizeWhen((T)x);

            permission.CanSave = true;
        }

        public static void AllowDelete<T>(Func<T, bool> autohrizeWhen) where T : IEntity
        {
            var permission = GetOrCreate<T>();

            if (autohrizeWhen != null)
                permission.AuthorizeDeleteWhen = x => autohrizeWhen((T)x);

            permission.CanDelete = true;
        }

        public static void AllowPatch<T>(Func<T, Dictionary<PropertyInfo, object>, bool> autohrizeWhen) where T : IEntity
        {
            var permission = GetOrCreate<T>();

            if (autohrizeWhen != null)
                permission.AuthorizePatchWhen = (x, c) => autohrizeWhen((T)x, c);

            permission.CanPatch = true;
        }
    }
}