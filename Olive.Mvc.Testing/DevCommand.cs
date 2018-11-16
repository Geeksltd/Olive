using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    abstract class DevCommand : IDevCommand
    {
        protected string Param(string key) => Context.Current.Request().Param(key);

        public abstract string Name { get; }

        public virtual string Title => null;

        public virtual bool IsEnabled() => true;

        public abstract Task<string> Run();
    }
}