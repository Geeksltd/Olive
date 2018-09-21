using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Olive.Tests;

namespace Olive.GeoLocation.Tests
{

    public class GeoLocationTests : TestsBase
    {
        IGeoLocationService GeoLocationService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            services.AddGeoLocationService();

            Context.Initialize(services);
            Context.Current.Configure(services.BuildServiceProvider());

            GeoLocationService = Context.Current.GetService<IGeoLocationService>();
        }

        [Test]
        public void Is_service_not_null()
        {
            GeoLocationService.ShouldNotBeNull();
        }
    }
}
