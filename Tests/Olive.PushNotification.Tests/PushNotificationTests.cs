using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Olive.Tests;
using System.Collections.Generic;

namespace Olive.PushNotification.Tests
{
    [TestFixture]
    public class PushNotificationTests : TestsBase
    {
        IPushNotificationService PushNotificationService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            services.AddSingleton<ILogger<PushNotificationService>>(Mock.Of<ILogger<PushNotificationService>>());
            services.AddPushNotification();

            Context.Initialize(services);
            Context.Current.Configure(services.BuildServiceProvider());

            PushNotificationService = Context.Current.GetService<IPushNotificationService>();
        }

        [Test]
        public void Is_service_not_null() => PushNotificationService.ShouldNotBeNull();

        [Test]
        public void Can_send_push_notification_iOS()
        {
            PushNotificationService.ShouldNotBeNull();

            var items = new List<iOSUserDevice>
            {
                new iOSUserDevice()
            };

            var result = PushNotificationService.Send("Title", "Geeks push notification", items);

            result.ShouldBeTrue();
        }
    }

    public class iOSUserDevice : IUserDevice
    {
        public string DeviceType => "iOS";

        public string PushNotificationToken => "Token";
    }
}
