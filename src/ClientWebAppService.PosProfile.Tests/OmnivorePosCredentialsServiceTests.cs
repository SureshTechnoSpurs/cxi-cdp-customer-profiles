﻿using System.Collections.Generic;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Services.Credentials;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models.Create;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClientWebAppService.PosProfile.Tests
{
    public class OmnivorePosCredentialsServiceTests
    {
        private readonly Mock<ISecretSetter> _secretSetter;
        private readonly Mock<ISecretClient> _secretClient;
        private readonly Mock<ILogger<OmnivorePosCredentialsService>> _logger;

        public OmnivorePosCredentialsServiceTests()
        {
            _secretSetter = new Mock<ISecretSetter>();
            _secretClient = new Mock<ISecretClient>();
            _logger = new Mock<ILogger<OmnivorePosCredentialsService>>();
            _credentialsService = new OmnivorePosCredentialsService(_secretSetter.Object, _secretClient.Object, _logger.Object);
        }

        private readonly OmnivorePosCredentialsService _credentialsService;

        [Fact]
        public async Task Process_CorrectParamsPassed_CorrectResult()
        {
            var posType = "omnivore";
            var partnerId = "partnerId";
            var locations = new List<string>() { "location1", "location2" };
            var result = await _credentialsService.OnboardingProcess(partnerId, new PosCredentialsConfigurationOmnivoreCreationDto(posType, locations));
            
            Assert.NotNull(result);
            Assert.Equal("omnivore", result.PosType);
            Assert.Equal(SecretExtensions.GetPosConfigurationSecretName(partnerId, posType), result.KeyVaultReference);
            Assert.Null(result.MerchantId);
        }

        [Fact]
        public async Task ProcessOffboard_CorrectParamsPassed_CorrectResult()
        {
            var posType = "omnivore";
            var partnerId = "partnerId";

            var secretName = SecretExtensions.GetPosConfigurationSecretName(partnerId, posType);

            await _credentialsService.OffboardingProcess(partnerId);
            _secretClient.Verify(x => x.DeleteSecretAsync(secretName), Times.Once);
        }
    }
}