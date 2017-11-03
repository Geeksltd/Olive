using Olive.Entities;

namespace Olive.Web
{
    public interface IWebResource : IEntity
    {
        string GetUrl();
    }
}
