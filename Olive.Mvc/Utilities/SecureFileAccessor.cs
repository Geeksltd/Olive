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
        string[] PathParts;

        Type Type;
        string Property;
        PropertyInfo PropertyInfo;
        IPrincipal CurrentUser;

        static IDatabase Database => Context.Current.Database();

        public IEntity Instance { get; private set; }

        public Blob Blob { get; private set; }

        public string SecurityErrors { get; private set; }

        /// <summary>
        /// Use create method to instantiate the class.
        /// </summary>
        FileAccessor() { }

        /// <summary>
        /// Creates a new SecureFileAccessor instance.
        /// </summary>
        public static async Task<FileAccessor> Create(string path, IPrincipal currentUser)
        {
            var result = new FileAccessor
            {
                CurrentUser = currentUser,
                PathParts = path.Split('/')
            };

            if (result.PathParts.Length < 2)
                throw new Exception($"Invalid path specified: '{path}'");

            result.FindRequestedProperty();

            await result.FindRequestedObject();

            result.SecurityErrors = result.GetSecurityErrors();

            return result;
        }

        public bool IsAllowed() => SecurityErrors.IsEmpty();

        void FindRequestedProperty()
        {
            var typeName = PathParts[0].Split('.')[0];

            Type = Database.GetRegisteredAssemblies()
                .Select(a => a.GetExportedTypes().SingleOrDefault(t => t.Name == typeName))
                .ExceptNull().FirstOrDefault();

            if (Type == null) Type = AppDomain.CurrentDomain.FindImplementers(typeof(IEntity)).SingleOrDefault(x => x.Name == typeName);

            if (Type == null) throw new Exception($"Invalid type name specified: '{typeName}'");

            Property = PathParts[0].Split('.')[1];

            PropertyInfo = Type.GetProperty(Property);
            if (PropertyInfo == null)
                throw new Exception($"Could not find the property '{Property}' on the type '{Type.FullName}'.");
        }

        async Task FindRequestedObject()
        {
            var idData = PathParts[1];

            foreach (var key in new[] { ".", "/" })
                if (idData.Contains(key)) idData = idData.Substring(0, idData.IndexOf(key));

            Instance = await Database.GetOrDefault(idData, Type);

            if (Instance == null) throw new Exception($"Invalid {Type.FullName} ID specified: '{idData}'");

            Blob = Instance.GetType().SafeGetProperty(Property)?.GetValue(Instance) as Blob;
            if (Blob == null)
                throw new Exception("Failed to find a Blob property named '" + Property
                + "' on " + Instance.GetType().FullName + ".");
        }

        bool NeedsSecureAccess() => PropertyInfo.GetCustomAttribute<SecureFileAttribute>() != null;

        string GetSecurityErrors()
        {
            if (!NeedsSecureAccess()) return null;

            var method = Type.GetMethod($"Is{Property}VisibleTo", BindingFlags.Public | BindingFlags.Instance);

            if (method == null)
                return $"{Type.FullName}.Is{Property}VisibleTo() method is not defined.";

            if (!method.GetParameters().IsSingle() ||
                !method.GetParameters().Single().ParameterType.Implements<IPrincipal>())
                return $"{Type.FullName}.{method.Name}() doesn't accept a single argument that implements IUser";

            if (!(bool)method.Invoke(Instance, new object[] { CurrentUser }))
                return "You are not authorised to view the requested file.";

            return null;
        }
    }
}