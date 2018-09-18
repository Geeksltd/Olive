using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Olive.Entities;
using Olive.Tests;
using System;
using System.Threading.Tasks;

namespace Olive.SMS.Tests
{
    [TestFixture]
    public class SmsTests : TestsBase
    {
        ISmsService SmsService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            services.AddSingleton<IDatabase>(Mock.Of<IDatabase>());
            services.AddSingleton<ILogger<SmsService>>(Mock.Of<ILogger<SmsService>>());
            services.AddSms();

            Context.Initialize(services);

            Context.Current.Configure(services.BuildServiceProvider());

            SmsService = Context.Current.GetService<ISmsService>();
        }

        [Test]
        public async Task Can_send_sms()
        {
            SmsService.ShouldNotBeNull();

            var smsQueueItem = Mock.Of<ISmsMessage>();

            var result = await SmsService.Send(smsQueueItem);

            result.ShouldBeTrue();
        }

    }
}
