using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Olive.Entities;
using Olive.Web;

namespace Olive.Mvc
{
    public class FileAccessor
    {
        string[] PathParts;

        Type Type;
        string Property;
        PropertyInfo PropertyInfo;
        IUser CurrentUser;

        public IEntity Instance { get; private set; }

        public Blob Blob { get; private set; }

        public string SecurityErrors { get; private set; }

        /// <summary>
        /// Use create method to instantiate the class.
        /// </summary>
        private FileAccessor() { }

        /// <summary>
        /// Creates a new SecureFileAccessor instance.
        /// </summary>
        public static async Task<FileAccessor> Create(string path, IUser currentUser)
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

        public FileInfo GetFile()
        {
            var file = Blob.LocalPath;

            // Fall-back logic
            if (!File.Exists(file))
                file = Blob.FallbackPaths.FirstOrDefault(File.Exists);

            return file.AsFile();
        }

        void FindRequestedProperty()
        {
            var typeName = PathParts[0].Split('.')[0];

            Type = Entity.Database.GetRegisteredAssemblies().Select(a => a.GetExportedTypes().SingleOrDefault(t => t.Name == typeName)).ExceptNull().FirstOrDefault();
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

            Instance = await Entity.Database.GetOrDefault(idData, Type);

            if (Instance == null) throw new Exception($"Invalid {Type.FullName} ID specified: '{idData}'");

            Blob = EntityManager.ReadProperty(Instance, Property) as Blob;
        }

        bool NeedsSecureAccess() => PropertyInfo.GetCustomAttribute<SecureFileAttribute>() != null;

        string GetSecurityErrors()
        {
            if (!NeedsSecureAccess()) return null;

            var method = Type.GetMethod($"Is{Property}VisibleTo", BindingFlags.Public | BindingFlags.Instance);

            if (method == null)
                return $"{Type.FullName}.Is{Property}VisibleTo() method is not defined.";

            if (method.GetParameters().Count() != 1 || !method.GetParameters().Single().ParameterType.Implements<IUser>())
                return $"{Type.FullName}.{method.Name}() doesn't accept a single argument that implements IUser";

            if (!(bool)method.Invoke(Instance, new object[] { CurrentUser }))
                return "You are not authorised to view the requested file.";

            return null;
        }
    }
}