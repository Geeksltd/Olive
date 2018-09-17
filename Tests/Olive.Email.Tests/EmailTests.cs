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
        IEmailSender emailSender;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            services.AddSingleton<IDatabase>(Mock.Of<IDatabase>());

            services.AddSingleton<ILogger<EmailSender>>(Mock.Of<ILogger<EmailSender>>());

            services.AddEmail();

            Context.Initialize(services);

            Context.Current.Configure(services.BuildServiceProvider());

            emailSender = Context.Current.GetService<IEmailSender>();
        }

        [Test]
        public async Task SendEmail()
        {
            emailSender.ShouldNotBeNull();

            //Mock email message
            var mock = Mock.Of<IEmailMessage>();

            mock.To = "paymon@geeks.ltd.uk";
            mock.SendableDate = LocalTime.Now.AddDays(1); //send email tomorrow, so we will get false result.

            var result = await emailSender.Send(mock);

            result.ShouldBeFalse();
        }
    }
}