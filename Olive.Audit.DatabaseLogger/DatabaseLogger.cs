using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Audit
{
    public class DatabaseLogger : ILogger
    {
        static Type AuditEventImplementation;

        public static void SetApplicationEventType(Type type) => AuditEventImplementation = type;

        public IAuditEvent CreateInstance()
        {
            if (AuditEventImplementation != null)
                return Activator.CreateInstance(AuditEventImplementation) as IAuditEvent;

            var possible = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
            {
                try { return a.GetExportedTypes().Where(t => t.Implements<IAuditEvent>() && !t.IsInterface).ToList(); }
                catch { return new List<Type>(); }
            }).ToList();

            if (possible.Count == 0)
                throw new Exception("No type in the currently loaded assemblies implements IApplicationEvent.");

            if (possible.Count > 1)
                throw new Exception($"More than one type in the currently loaded assemblies implement IApplicationEvent:{possible.Select(x => x.FullName).ToString(" and ")}");

            AuditEventImplementation = possible.Single();
            return CreateInstance();
        }

        public Task Log(IAuditEvent auditEvent) => Entity.Database.Save(auditEvent);
    }
}
