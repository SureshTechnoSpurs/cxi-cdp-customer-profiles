using ClientWebAppService.UserProfile.Core;
using CXI.Common.MessageBrokers.Producers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ClientWebAppService.UserProfile.Business.Tests
{
    public class EmailServiceTests
    {
        public readonly IEmailService _service;

        public Mock<IProducer> _producerMock = new Mock<IProducer>();
        public Mock<IOptions<AdB2CInvitationOptions>> _optionsMock = new Mock<IOptions<AdB2CInvitationOptions>>();

        public EmailServiceTests()
        {
            AdB2CInvitationOptions opts = new AdB2CInvitationOptions()
            {
                InvitationClientId = "testClinetId",
                Domain = "test-Domain",
                Instance = "test-Instance",
                SignUpSignInPolicyId = "test-SignUpSignInPolicyId",
                RedirectUrl = "test-RedirectUrl",
                TokenSecurityKey = "test-TokenSecurityKey",
                TokenIssuer = "test-TokenIssuer",
                TokenAudience = "test-TokenAudience"
            };

            _optionsMock.Setup(opt => opt.Value).Returns(opts);

            _service = new EmailService(_producerMock.Object, _optionsMock.Object);
        }

        [Fact]
        public async Task SendInvitationMessageToAssociateAsync_EmailPassed_ExceptionNotThrown()
        {
            var testInput = "testMail@gmail.com";

            _producerMock.Setup(mock => mock.SendMessages(It.IsAny<string[]>())).Returns(Task.CompletedTask);

            var invocation = _service.Invoking(x => x.SendInvitationMessageToAssociateAsync(testInput));

            await invocation.Should().NotThrowAsync();
        }
    }
}
