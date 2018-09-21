using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    abstract class DevCommand : IDevCommand
    {
        public HttpContext Context { get; }

        protected DevCommand(IHttpContextAccessor contextAccessor)
        {
            Context = contextAccessor.HttpContext;
        }

        protected string Param(string key) => Context.Request.Param(key);

        public abstract string Name { get; }

        public virtual string Title => null;

        public virtual bool IsEnabled() => true;

        public abstract Task<bool> Run();
    }
}
