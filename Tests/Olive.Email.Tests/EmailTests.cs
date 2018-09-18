using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Olive.Entities;
using Olive.Tests;
using System.Threading.Tasks;

namespace Olive.Email.Tests
{
    [TestFixture]
    public class EmailTests : TestsBase
    {
        IEmailOutbox EmailOutbox;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            services.AddSingleton<IDatabase>(Mock.Of<IDatabase>());

            services.AddSingleton<ILogger<EmailOutbox>>(Mock.Of<ILogger<EmailOutbox>>());

            services.AddEmail();

            Context.Initialize(services);

            Context.Current.Configure(services.BuildServiceProvider());

            EmailOutbox = Context.Current.GetService<IEmailOutbox>();
        }

        [Test]
        public async Task Can_send_email()
        {
            EmailOutbox.ShouldNotBeNull();

            //Mock email message
            var mock = Mock.Of<IEmailMessage>();

            mock.To = "paymon@geeks.ltd.uk";
            mock.SendableDate = LocalTime.Now.AddDays(1); //send email tomorrow, so we will get false result.

            var result = await EmailOutbox.Send(mock);

            result.ShouldBeFalse();
        }
    }
}