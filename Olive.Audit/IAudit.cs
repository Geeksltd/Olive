using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Audit
{
    public interface IAudit
    {
        Task Log(IAuditEvent auditEvent);
        Task Log(string @event, string data);
        Task Log(string @event, string data, IEntity item);
        Task Log(string @event, string data, IEntity item, string userId, string userIp);
        Task LogDelete(IEntity entity);
        Task LogInsert(IEntity entity);
        Task LogUpdate(IEntity entity);
    }
}