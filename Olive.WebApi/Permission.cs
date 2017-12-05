namespace Olive.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Olive;
    using Olive.Entities;

    public partial class Permission
    {
        public bool CanRead { get; internal set; }
        public bool CanSave { get; internal set; }
        public bool CanDelete { get; internal set; }
        public bool CanPatch { get; internal set; }

        public List<ICriterion> AppendCriteria { get; internal set; }

        public Func<bool> AuthorizeReadWhen { get; internal set; }
        public Func<IEntity, bool> AuthorizeSaveWhen { get; internal set; }
        public Func<IEntity, bool> AuthorizeDeleteWhen { get; internal set; }
        public Func<IEntity, bool> PostQueryFilter { get; internal set; }

        public Func<IEntity, Dictionary<PropertyInfo, object>, bool> AuthorizePatchWhen { get; internal set; }

        public Type Type { get; internal set; }

        public override string ToString() => Type.Name + ": " + "Read".OnlyWhen(CanRead);

        internal string GetReadAuthorizationError()
        {
            if (!CanRead) return "No read permission is granted for " + Type.FullName;

            if (AuthorizeReadWhen?.Invoke() == false)
                return "Failed the 'read authorisation' rule in the permission settings on " + Type.FullName;

            return null;
        }
    }
}