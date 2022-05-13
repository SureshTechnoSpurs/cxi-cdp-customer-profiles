using System.Collections.Generic;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Services.Credentials;
using CXI.Contracts.PosProfile.Models.Create;
using Xunit;

namespace ClientWebAppService.PosProfile.Tests
{
    public class OmnivorePosCredentialsServiceTests
    {
        public OmnivorePosCredentialsServiceTests()
        {
            _credentialsService = new OmnivorePosCredentialsService();
        }

        private readonly OmnivorePosCredentialsService _credentialsService;

        [Fact]
        public async Task Process_CorrectParamsPassed_CorrectResult()
        {
            var posType = "omnivore";
            var result = await _credentialsService.Process("partnerId", new PosCredentialsConfigurationOmnivoreCreationDto(posType, new List<string>()));
            
            Assert.NotNull(result);
            Assert.Equal("omnivore", result.PosType);
            Assert.Null(result.KeyVaultReference);
            Assert.Null(result.MerchantId);
        }
    }
}