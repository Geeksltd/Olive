using System.Threading.Tasks;

namespace Olive
{
    class GetFail<T> : GetImplementation<T>
    {
        public GetFail() : base(null) { }

        public override Task<bool> Attempt(string url) => Task.FromResult(false);
    }
}