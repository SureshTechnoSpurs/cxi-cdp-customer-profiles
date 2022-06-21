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
    /// <summary>
    /// Test cases for IOloService
    /// </summary>
    public class KeyVaultReferenceServiceTests
    {
        private readonly IKeyVaultReferenceService _keyVaultReferenceService;
        private Mock<ISecretClient> _secretClientMock;
        private Mock<ILogger<KeyVaultReferenceService>> _loggerMock;
        private readonly Mock<IPosProfileService> _posProfileServiceMock;
        private const string _testKey = "testKey";

        public KeyVaultReferenceServiceTests()
        {
            _secretClientMock = new Mock<ISecretClient>();
            _loggerMock = new Mock<ILogger<KeyVaultReferenceService>>();
            _posProfileServiceMock = new Mock<IPosProfileService>();

            _keyVaultReferenceService = new KeyVaultReferenceService(_posProfileServiceMock.Object, _loggerMock.Object, _secretClientMock.Object);
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
            var result = await _keyVaultReferenceService.GetKeyVaultValueByReferenceAsync<ParBrinkKeyReferenceModel>(partnerId, expectedPosType);

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
            var result = await _keyVaultReferenceService.SetKeyVaultValueByReferenceAsync<ParBrinkKeyReferenceModel>(partnerId, expectedPosType, parBrinkLocationRequest);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetOloApiKeyAsync_CorrectParams_ReturnsResult()
        {
            // Arrange
            var partnerId = "partnerId";
            var expectedPosType = "olo";
            var keyvaultReference = "key";
            var apiKey = "apiKey1";

            var posProfile = new PosProfileDto(partnerId, new List<PosCredentialsConfigurationDto>() {
                new PosCredentialsConfigurationDto(expectedPosType,keyvaultReference,String.Empty)
            });

            var oloKeyReferenceModel = new OloKeyReferenceModel() { ApiKey = apiKey };

            _posProfileServiceMock.Setup(x => x.GetPosProfileByPartnerId(partnerId))
                .ReturnsAsync(posProfile);

            var secretValue = JsonConvert.SerializeObject(oloKeyReferenceModel);
            var keyvault = new Mock<KeyVaultSecret>(_testKey, secretValue);
            var configuration = new Mock<Response<KeyVaultSecret>>();
            configuration.Setup(x => x.Value).Returns(keyvault.Object);
            _secretClientMock.Setup(c => c.GetSecret(It.IsAny<string>()))
                .Returns(configuration.Object);

            // Act
            var result = await _keyVaultReferenceService.GetKeyVaultValueByReferenceAsync<OloKeyReferenceModel>(partnerId, expectedPosType);

            // Assert
            result.Should().NotBeNull();

        }

        [Fact]
        public async Task SetOloApiKeyAsync_CorrectParams_ReturnsResult()
        {
            // Arrange
            var partnerId = "partnerId";
            var expectedPosType = "olo";
            var apiKey = "apiKey1";

            var posProfile = new PosProfileDto(partnerId, new List<PosCredentialsConfigurationDto>() {
                new PosCredentialsConfigurationDto(expectedPosType,_testKey,String.Empty)
            });

            var oloLocationRequest = new OloKeyReferenceModel() { ApiKey = apiKey };
            _posProfileServiceMock.Setup(x => x.GetPosProfileByPartnerId(partnerId))
                 .ReturnsAsync(posProfile);

            var secretValue = JsonConvert.SerializeObject(oloLocationRequest);
            var keyvault = new Mock<KeyVaultSecret>(_testKey, secretValue);
            var configuration = new Mock<Response<KeyVaultSecret>>();
            configuration.Setup(x => x.Value).Returns(keyvault.Object);
            _secretClientMock.Setup(c => c.SetSecret(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(configuration.Object);

            // Act
            var result = await _keyVaultReferenceService.SetKeyVaultValueByReferenceAsync<OloKeyReferenceModel>(partnerId, expectedPosType, oloLocationRequest);

            // Assert
            result.Should().BeTrue();
        }
    }
}
