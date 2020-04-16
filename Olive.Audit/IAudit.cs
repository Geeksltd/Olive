using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Audit
{
    public interface IAudit
    {
        Task Log(IAuditEvent auditEvent);
        Task Log(string @event, string data, string group = null);
        Task Log(string @event, string data, IEntity item, string group = null);
        Task Log(string @event, string data, IEntity item, string userId, string userIp, string group = null);
        Task LogDelete(IEntity entity);
        Task LogInsert(IEntity entity);
        Task LogUpdate(IEntity entity);
    }
}