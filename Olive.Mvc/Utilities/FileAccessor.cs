using Olive.Entities;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class FileAccessor
    {
        protected readonly Type Type;
        protected readonly PropertyInfo PropertyInfo;
        protected string Id;
        protected readonly IPrincipal CurrentUser;
        protected readonly IDatabase Database;

        public IEntity Instance { get; private set; }

        public Blob Blob { get; private set; }

        public string SecurityErrors { get; private set; }

        /// <summary>
        /// Use create method to instantiate the class.
        /// </summary>
        public FileAccessor(Type type, PropertyInfo propertyInfo, string id, IPrincipal user, IDatabase database)
        {
            Type = type;
            PropertyInfo = propertyInfo;
            Id = id;
            CurrentUser = user;
            Database = database;
        }

        public async Task LoadBlob()
        {
            await FindRequestedObject();

            if (NeedsSecureAccess())
                SecurityErrors = GetSecurityErrors();
        }

        public bool IsAllowed() => SecurityErrors.IsEmpty();

        async Task FindRequestedObject()
        {
            foreach (var key in new[] { ".", "/" })
                if (Id.Contains(key)) Id = Id.Substring(0, Id.IndexOf(key));

            Instance = await Database.GetOrDefault(Id, Type);

            if (Instance == null) throw new Exception($"Invalid {Type.FullName} ID specified: '{Id}'");

            Blob = PropertyInfo?.GetValue(Instance) as Blob;
            if (Blob == null)
                throw new Exception("Failed to find a Blob property named '" + PropertyInfo.Name
                + "' on " + Instance.GetType().FullName + ".");
        }

        bool NeedsSecureAccess() => PropertyInfo.GetCustomAttribute<SecureFileAttribute>() != null;

        string GetSecurityErrors()
        {
            var method = Type.GetMethod($"Is{PropertyInfo.Name}VisibleTo", BindingFlags.Public | BindingFlags.Instance);

            if (method == null)
                return $"{Type.FullName}.Is{PropertyInfo.Name}VisibleTo() method is not defined.";

            if (!method.GetParameters().IsSingle() ||
                !method.GetParameters().Single().ParameterType.Implements<IPrincipal>())
                return $"{Type.FullName}.{method.Name}() doesn't accept a single argument that implements IPrincipal";

            if (!(bool)method.Invoke(Instance, new object[] { CurrentUser }))
                return "You are not authorised to view the requested file.";

            return null;
        }
    }
}