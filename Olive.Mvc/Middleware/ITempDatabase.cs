using System.Threading.Tasks;

namespace Olive.Mvc
{
    public interface ITempDatabase
    {
        Task Restart();
        Task AwaitReadiness();
        Task ReCreateDb();
        Task Seed();
    }
}
