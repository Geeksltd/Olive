using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Audit
{
    public interface ILogger
    {
        IAuditEvent CreateInstance();
        Task Log(IAuditEvent auditEvent);
    }
}
