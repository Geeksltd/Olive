using System.Security.Claims;
using System.Threading.Tasks;

namespace Olive.Audit
{
    public interface IAuditLogger
    {
        IAuditEvent CreateInstance();
        Task Log(IAuditEvent auditEvent);
    }

    public interface IContextUserProvider
    {
        ClaimsPrincipal GetUser();
        string GetUserIP();
    }
}