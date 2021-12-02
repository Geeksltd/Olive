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

            if (PropertyInfo.Defines<SecureFileAttribute>())
                SecurityErrors = await GetSecurityErrors();
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

        async Task<string> GetSecurityErrors()
        {
            var methodName = $"Is{PropertyInfo.Name}VisibleTo";
            var type = Type.GetProgrammingName();

            var method = Type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);

            if (method is null) return $"{type}.{methodName}() method is not defined.";

            if (!method.GetParameters().IsSingle() ||
                !method.GetParameters().Single().ParameterType.Implements<IPrincipal>())
                return $"{type}.{methodName}() should take a single argument, and of type IPrincipal";

            var result = method.Invoke(Instance, new object[] { CurrentUser });

            if (result is Task<bool> task) result = await task;

            if (result is null) result = false;

            if (result is bool good)
                if (good) return null;
                else return "You are not authorised to view the requested file.";

            return $"{methodName} returned {result.GetType().GetProgrammingName()}. Expected: bool or Task<bool>";
        }
    }
}