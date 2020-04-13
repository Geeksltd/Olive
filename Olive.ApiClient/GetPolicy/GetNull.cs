using System.Threading.Tasks;

namespace Olive
{
    class GetNull<T> : GetImplementation<T>
    {
        public GetNull(ApiClient client) : base(client) { }

        public override async Task<bool> Attempt(string url)
        {
            Result = default(T);

            if (FallBackEventPolicy == ApiFallBackEventPolicy.Raise)
            {
                await OnFallBackEvent(new FallBackEvent
                {
                    Url = url,
                    FriendlyMessage = $"Failed to get results from {url.AsUri().Host}."
                });
            }

            return true;
        }
    }
}