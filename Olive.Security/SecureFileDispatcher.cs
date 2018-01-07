using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Olive.Entities;
using Olive.Web;

namespace Olive.Security
{
    public class SecureFileDispatcher
    {
        public static readonly AsyncEvent<UnauthorisedRequestEventArgs> UnauthorisedFileRequested =
            new AsyncEvent<UnauthorisedRequestEventArgs>();

        string[] PathParts;

        Type Type;
        object Instance;
        string Property;
        HttpResponse Response;
        Blob Blob;
        IPrincipal CurrentUser;

        /// <summary>
        /// Creates a new SecureFileDispatcher instance.
        /// </summary>
        public SecureFileDispatcher(string path, IPrincipal currentUser)
        {
            CurrentUser = currentUser;

            Response = Context.HttpContextAccessor.HttpContext.Response;

            PathParts = path.Split('/');

            if (PathParts.Length < 2)
            {
                throw new Exception($"Invalid path specified: '{path}'");
            }

            FindRequestedProperty();

            FindRequestedObject();
        }

        public async Task Dispatch() => await DispatchFile(await GetFile());

        public async Task<FileInfo> GetFile()
        {
            await EnsureSecurity();

            var file = Blob.LocalPath;

            return file.AsFile();
        }

        void FindRequestedProperty()
        {
            var typeName = PathParts[0].Split('.')[0];

            Type = Entity.Database.GetRegisteredAssemblies().Select(a => a.GetExportedTypes().SingleOrDefault(t => t.Name == typeName)).ExceptNull().FirstOrDefault();
            if (Type == null) throw new Exception($"Invalid type name specified: '{typeName}'");

            Property = PathParts[0].Split('.')[1];
        }

        void FindRequestedObject()
        {
            var idData = PathParts[1];

            foreach (var key in new[] { ".", "/" })
                if (idData.Contains(key)) idData = idData.Substring(0, idData.IndexOf(key));

            var id = idData.TryParseAs<Guid>();
            if (id == null) throw new Exception($"Invalid object ID specified: '{idData}'");

            Instance = Entity.Database.Get(id.Value, Type);
            if (Instance == null) throw new Exception($"Invalid {Type.FullName} ID specified: '{id}'");

            Blob = EntityManager.ReadProperty(Instance, Property) as Blob;
        }

        async Task EnsureSecurity()
        {
            try
            {
                var method = Type.GetMethod("Is" + Property + "VisibleTo", BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    throw new Exception(Type.FullName + ".Is" + Property + "VisibleTo() method is not defined.");
                }

                if (method.GetParameters().Count() != 1 || !method.GetParameters().Single().ParameterType.Implements<IPrincipal>())
                    throw new Exception(Type.FullName + "." + method.Name +
                        "() doesn't accept a single argument that implements IPrincipal");

                var result = (Task<bool>)method.Invoke(Instance, new object[] { CurrentUser });
                if (!await result)
                    throw new Exception("You are not authorised to view the requested file.");
            }
            catch (Exception ex)
            {
                if (UnauthorisedFileRequested.IsHandled())
                {
                    await UnauthorisedFileRequested.Raise(new UnauthorisedRequestEventArgs
                    {
                        Exception = ex,
                        Instance = Instance as IEntity,
                        Property = Type.GetProperty(Property)
                    });
                }
                else
                {
                    Response.Clear();
                    Response.WriteAsync("<html><body><h2>File access issue</h2></body></html>").RunSynchronously();
                    Log.Error("Invalid secure file access: " + PathParts.ToString("/"), ex);

                    Response.WriteLine("Invalid file request. Please contact your I.T. support.");
                    Response.WriteLine(ex.Message);
                }
            }
        }

        async Task DispatchFile(FileInfo file)
        {
            if (!file.Exists())
            {
                Response.Clear();
                Response.WriteAsync("File does not exist: " + file).RunSynchronously();
                return;
            }

            var fileName = Blob.FileName.Or(file.Name);
            var contentType = file.Extension.OrEmpty().TrimStart(".").ToLower().Or("Application/octet-stream");

            await Response.Dispatch(await file.ReadAllBytesAsync(), fileName, contentType);
        }

        public class UnauthorisedRequestEventArgs : EventArgs
        {
            /// <summary>
            /// A property of type Blob which represents the requested file property.
            /// </summary>
            public PropertyInfo Property;

            /// <summary>
            /// The object on which the blob property was requested.
            /// </summary>
            public IEntity Instance;

            /// <summary>
            /// The security error raised by M# framework.
            /// </summary>
            public Exception Exception;
        }
    }
}