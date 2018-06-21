using System;
using System.Threading.Tasks;

namespace Olive
{
    class GetFail<T> : GetImplementation<T>
    {
        public GetFail() : base(null) { }

        public override Task<bool> Attempt(string url)
        {
            throw new Exception("Get request failed.");
        }
    }
}