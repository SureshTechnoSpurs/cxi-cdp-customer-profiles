using ClientWebAppService.PosProfile.Services.Credentials;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models.Create;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ClientWebAppService.PosProfile.Tests
{
    public class ToastPosCredentialsServiceTests
    {
        private readonly Mock<ISecretSetter> _secretSetter;
        private readonly Mock<ISecretClient> _secretClient;
        private readonly Mock<ILogger<ToastPosCredentialsService>> _logger;

        private readonly ToastPosCredentialsService _credentialsService;

        public ToastPosCredentialsServiceTests()
        {
            _secretSetter = new Mock<ISecretSetter>();
            _secretClient = new Mock<ISecretClient>();
            _logger = new Mock<ILogger<ToastPosCredentialsService>>();

            _credentialsService = new ToastPosCredentialsService(_secretSetter.Object, _logger.Object, _secretClient.Object);
        }

        [Fact]
        public async Task Process_CorrectParamsPassed_CorrectResult()
        {
            var posType = "toast";
            var partnerId = "partnerId";
            var partnerCode = "partnerCode";
            var result = 
                await _credentialsService.OnboardingProcess(partnerId, new PosCredentialsConfigurationToastCreationDto(posType, partnerCode));

            Assert.NotNull(result);
            Assert.Equal(posType, result.PosType);
            Assert.Equal(SecretExtensions.GetPosConfigurationSecretName(partnerId, posType), result.KeyVaultReference);
        }

        [Fact]
        public async Task ProcessOffboard_CorrectParamsPassed_CorrectResult()
        {
            var posType = "toast";
            var partnerId = "partnerId";

            var secretName = SecretExtensions.GetPosConfigurationSecretName(partnerId, posType);

            await _credentialsService.OffboardingProcess(partnerId);
            _secretClient.Verify(x => x.DeleteSecretAsync(secretName), Times.Once);
            _secretClient.Verify(x => x.DeleteSecretAsync(It.IsAny<string>()), Times.Once);
        }
    }
}