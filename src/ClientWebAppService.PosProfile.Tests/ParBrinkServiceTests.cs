using Azure;
using Azure.Security.KeyVault.Secrets;
using ClientWebAppService.PosProfile.Services;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models;
using CXI.Contracts.PosProfile.Models.PosKeyReference;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ClientWebAppService.PosProfile.Tests
{
    public class ParBrinkServiceTests
    {
        private readonly IParBrinkService _parBrinkService;
        private Mock<ISecretClient> _secretClientMock;
        private Mock<ILogger<ParBrinkService>> _loggerMock;
        private readonly Mock<IPosProfileService> _posProfileServiceMock;
        private const string _testKey = "testKey";

        public ParBrinkServiceTests()
        {
            _secretClientMock = new Mock<ISecretClient>();
            _loggerMock = new Mock<ILogger<ParBrinkService>>();
            _posProfileServiceMock = new Mock<IPosProfileService>();

            _parBrinkService = new ParBrinkService(_posProfileServiceMock.Object, _loggerMock.Object, _secretClientMock.Object);
        }

        [Fact]
        public async Task GetParBrinkLocationsAsync_CorrectParams_ReturnsResult()
        {
            // Arrange
            var partnerId = "partnerId";
            var expectedPosType = "parbrink";
            var keyvaultReference = "key";
            var token = "token";
            var url = "http://test.url";

            var posProfile = new PosProfileDto(partnerId, new List<PosCredentialsConfigurationDto>() {
                new PosCredentialsConfigurationDto(expectedPosType,keyvaultReference,String.Empty)
            });

            var location = new List<ParBrinkLocationConfiguration>()
                { new ParBrinkLocationConfiguration
                    {
                         Token = token,
                         Url = url
                    }
                };
            var parBrinkKeyReferenceModel = new ParBrinkKeyReferenceModel() { Locations = location };

            _posProfileServiceMock.Setup(x => x.GetPosProfileByPartnerId(partnerId))
                .ReturnsAsync(posProfile);

            var secretValue = JsonConvert.SerializeObject(parBrinkKeyReferenceModel);
            var keyvault = new Mock<KeyVaultSecret>(_testKey, secretValue);
            var configuration = new Mock<Response<KeyVaultSecret>>();
            configuration.Setup(x => x.Value).Returns(keyvault.Object);
            _secretClientMock.Setup(c => c.GetSecret(It.IsAny<string>()))
                .Returns(configuration.Object);

            // Act
            var result = await _parBrinkService.GetParBrinkLocationsAsync(partnerId);

            // Assert
            result.Should().NotBeNull();

        }

        [Fact]
        public async Task SetParBrinkLocationsAsync_CorrectParams_ReturnsResult()
        {
            // Arrange
            var partnerId = "partnerId";
            var expectedPosType = "parbrink";
            var token = "token";
            var url = "http://test.url";
            var posProfile = new PosProfileDto(partnerId, new List<PosCredentialsConfigurationDto>() {
                new PosCredentialsConfigurationDto(expectedPosType,_testKey,String.Empty)
            });

            var location = new List<ParBrinkLocationConfiguration>() { new ParBrinkLocationConfiguration() { 
                Token = token,
                Url = url
            } };
            var parBrinkLocationRequest = new ParBrinkKeyReferenceModel() { Locations = location };
            _posProfileServiceMock.Setup(x => x.GetPosProfileByPartnerId(partnerId))
                 .ReturnsAsync(posProfile);

            var secretValue = JsonConvert.SerializeObject(parBrinkLocationRequest);
            var keyvault = new Mock<KeyVaultSecret>(_testKey, secretValue);
            var configuration = new Mock<Response<KeyVaultSecret>>();
            configuration.Setup(x => x.Value).Returns(keyvault.Object);
            _secretClientMock.Setup(c => c.SetSecret(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(configuration.Object);

            // Act
            var result = await _parBrinkService.SetParBrinkLocationsAsync(partnerId, parBrinkLocationRequest);

            // Assert
            result.Should().BeTrue();
        }
    }
}
