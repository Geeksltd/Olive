using Domain;
using Olive;

namespace Website
{
    public static class FeatureContext
    {
        public static Feature ViewingFeature
        {
            get => Context.Current.Http().Items["ViewingFeature"] as Feature;
            set => Context.Current.Http().Items["ViewingFeature"] = value;
        }
    }
}
