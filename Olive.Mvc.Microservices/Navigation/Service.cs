using Newtonsoft.Json;


namespace Olive.Mvc.Microservices
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class Service
    {
        public string BaseUrl { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }

    }
}
