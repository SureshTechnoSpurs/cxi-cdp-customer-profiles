using System.Collections.Generic;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Services.Credentials;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models.Create;
using Moq;
using Xunit;

namespace ClientWebAppService.PosProfile.Tests
{
    public class OmnivorePosCredentialsServiceTests
    {
        private readonly Mock<ISecretSetter> _secretSetter;

        public OmnivorePosCredentialsServiceTests()
        {
            _secretSetter = new Mock<ISecretSetter>();
            _credentialsService = new OmnivorePosCredentialsService(_secretSetter.Object);
        }

        private readonly OmnivorePosCredentialsService _credentialsService;

        [Fact]
        public async Task Process_CorrectParamsPassed_CorrectResult()
        {
            var posType = "omnivore";
            var partnerId = "partnerId";
            var locations = new List<string>() { "location1", "location2" };
            var result = await _credentialsService.Process(partnerId, new PosCredentialsConfigurationOmnivoreCreationDto(posType, locations));
            
            Assert.NotNull(result);
            Assert.Equal("omnivore", result.PosType);
            Assert.Equal(SecretExtensions.GetPosConfigurationSecretName(partnerId, posType), result.KeyVaultReference);
            Assert.Null(result.MerchantId);
        }
    }
}