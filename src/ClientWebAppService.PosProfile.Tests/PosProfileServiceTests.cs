using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Models;
using ClientWebAppService.PosProfile.Services;
using ClientWebAppService.PosProfile.Services.Credentials;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PartnerProfile;
using CXI.Contracts.PosProfile.Models;
using CXI.Contracts.PosProfile.Models.Create;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace ClientWebAppService.PosProfile.Tests
{
    public class PosProfileServiceTests
    {
        private const string SecretNotFoundErrorCode = "SecretNotFound";

        private readonly IPosProfileService _posProfileService;
        private readonly Mock<IPosProfileRepository> _posProfileRepositoryMock;
        private readonly Mock<ILogger<PosProfileService>> _loggerMock;
        private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock;
        private readonly Mock<ISecretClient> _secretClientMock;
        private readonly Mock<IPosCredentialsServiceResolver> _posCredentialsServiceResolver;
        private readonly Mock<IPosCredentialsOffboardingService> _squareCredentialsOffboardingService;
        private readonly Mock<IPartnerProfileM2MServiceClient> _partnerProfileServiceClientMock;

        public PosProfileServiceTests()
        {
            _posProfileRepositoryMock = new Mock<IPosProfileRepository>();
            _configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            _posProfileRepositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()));
            _loggerMock = new Mock<ILogger<PosProfileService>>();
            _secretClientMock = new Mock<ISecretClient>();
            _posCredentialsServiceResolver = new Mock<IPosCredentialsServiceResolver>();
            _squareCredentialsOffboardingService = new Mock<IPosCredentialsOffboardingService>();
            _partnerProfileServiceClientMock = new Mock<IPartnerProfileM2MServiceClient>();

            _posProfileService = new PosProfileService(
                _posProfileRepositoryMock.Object,
                _loggerMock.Object,
                _configurationMock.Object,
                _secretClientMock.Object,
                _posCredentialsServiceResolver.Object, 
                _partnerProfileServiceClientMock.Object);
        }

        [Fact]
        public async Task CreatePosProfileAsync_CorrectParametersPassed_SuccessfulResultReturned()
        {
            var creationDto = new PosProfileCreationModel<PosCredentialsConfigurationSquareCreationDto>()
            {
                PartnerId = "partnerId",
                PosConfigurations = new PosCredentialsConfigurationSquareCreationDto(
                    "Square", "AccessToken", "RefreshToken", DateTime.Today, "merchantId")
            };

            var posProfile = new Models.PosProfile
            {
                PartnerId = "partnerId",
                PosConfiguration = new List<PosCredentialsConfiguration>
                {
                    new PosCredentialsConfiguration
                    {
                        MerchantId = "merchantId",
                        PosType = "pssTpes",
                        KeyVaultReference = "reference"
                    }
                }
            };
            
            var service = new Mock<IPosCredentialsService<PosCredentialsConfigurationSquareCreationDto>>();
            service.Setup(x => x.OnboardingProcess("partnerId", creationDto.PosConfigurations)).ReturnsAsync(
                () => new PosCredentialsConfiguration() { PosType = "Square", MerchantId = "merchantId" });
            _posCredentialsServiceResolver.Setup(x => x.Resolve(creationDto.PosConfigurations)).Returns(service.Object);
            _posProfileRepositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
               .ReturnsAsync(posProfile);

            var invocation = _posProfileService.Invoking(x => x.CreatePosProfileAndSecretsAsync(creationDto));

            await invocation.Should().NotThrowAsync();
        }

        [Fact]
        public async Task DeletePosProfileAndSecretsAsync_PosProfilesNotFound_Success()
        {
            var partnerId = "test-partner-id";
            var posType = "square";
            var service = new Mock<IPosCredentialsOffboardingService>();

            _posProfileRepositoryMock
                .Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile>());

            _posCredentialsServiceResolver.Setup(x => x.ResolveOffboardingService(posType))
                .Returns(service.Object);

            await _posProfileService.DeletePosProfileAndSecretsAsync(partnerId, posType);

            _secretClientMock.Verify(x => x.DeleteSecretAsync(It.IsAny<string>()), Times.Never);
            _posProfileRepositoryMock.Verify(x => x.DeleteMany(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task DeletePosProfileAndSecretsAsync_RemovedSecretsAndPosProfile_Success()
        {
            var service = new Mock<IPosCredentialsOffboardingService>();
            var partnerId = "test-partner-id";
            var posType = "square";
            var posProfile = new Models.PosProfile
            {
                Id = new ObjectId(),
                PartnerId = partnerId,
                PosConfiguration = new List<PosCredentialsConfiguration>
                {
                    new PosCredentialsConfiguration { KeyVaultReference = "ref", PosType = posType }
                }
            };

            _posCredentialsServiceResolver.Setup(x => x.ResolveOffboardingService(posType))
                .Returns(service.Object);

            _posProfileRepositoryMock
                .Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile> { posProfile });

            await _posProfileService.DeletePosProfileAndSecretsAsync(partnerId, posType);

            _posProfileRepositoryMock.Verify(x => x.DeleteMany(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task DeletePosProfileAndSecretsAsync_UpdatedPosProfile_Success()
        {
            var service = new Mock<IPosCredentialsOffboardingService>();
            var partnerId = "test-partner-id";
            var posType = "square";
            var posType2 = "omnivore";
            var posConfiguration = new List<PosCredentialsConfiguration>
            {
                new PosCredentialsConfiguration { KeyVaultReference = "ref", PosType = posType },
                new PosCredentialsConfiguration { KeyVaultReference = "ref", PosType = posType2 }
            };
            var posProfile = new Models.PosProfile
            {
                Id = new ObjectId(),
                PartnerId = partnerId,
                PosConfiguration = posConfiguration
            };

            _posCredentialsServiceResolver.Setup(x => x.ResolveOffboardingService(posType))
                .Returns(service.Object);

            _posProfileRepositoryMock
                .Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile> { posProfile });

            await _posProfileService.DeletePosProfileAndSecretsAsync(partnerId, posType);

            posProfile.PosConfiguration = posConfiguration.Where(x => !x.PosType.Equals(posType));

            _posProfileRepositoryMock.Verify(x => x.UpdateAsync(partnerId, posProfile), Times.Once);
        }

        [Fact(Skip = "Obsolete due to transfer logic to another service")]
        public async Task DeletePosProfileAndSecretsAsync_SecretNotFound_RequestFailedExceptionThrown()
        {
            var partnerId = "test-partner-id";
            var posType = "square";
            var expectedMessage = $"Secret was not found in key vault for ${partnerId}";
            var exception = new RequestFailedException(1, expectedMessage, SecretNotFoundErrorCode, null);
            var posProfile = new Models.PosProfile
            {
                Id = new ObjectId(),
                PartnerId = partnerId,
                PosConfiguration = new List<PosCredentialsConfiguration>
                {
                    new PosCredentialsConfiguration { KeyVaultReference = "ref", PosType = posType }
                }
            };

            _posProfileRepositoryMock
                .Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile> { posProfile });

            _secretClientMock
                .Setup(x => x.DeleteSecretAsync(It.IsAny<string>()))
                .Throws(exception);

            await Assert.ThrowsAsync<RequestFailedException>(async () => await _posProfileService.DeletePosProfileAndSecretsAsync(partnerId, posType));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((message, type) => message.ToString() == expectedMessage),
                    It.Is<Exception>(ex => ex == exception),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

            _secretClientMock.Verify(x => x.DeleteSecretAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAccesTokenForPartner_CorrectParametersPassed_Success()
        {
            var partnerId = "test-partner-id";

            var posProfile = new Models.PosProfile
            {
                Id = new ObjectId(),
                PartnerId = partnerId,
                PosConfiguration = new List<PosCredentialsConfiguration>
                {
                    new PosCredentialsConfiguration { KeyVaultReference = "ref", PosType = "square" }
                }
            };

            var posProfileSecretConfiguration =
                new PosProfileSecretConfiguration(
                    new AccessToken(Value: "accessToken", DateTime.UtcNow),
                    new RefreshToken(Value: "refreshToken", null));

            var secret = new KeyVaultSecret("secretName", JsonConvert.SerializeObject(posProfileSecretConfiguration));

            _posProfileRepositoryMock
                .Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile> { posProfile });

            _secretClientMock
                .Setup(x => x.GetSecret(It.IsAny<string>()))
                .Returns(secret);

            var accessToken = await _posProfileService.GetAccesTokenForPartner(partnerId);

            Assert.NotEmpty(accessToken);
        }

        [Fact]
        public async Task GetAccesTokenForPartner_EmptyPosProfile_ThrowsValidationException()
        {
            var partnerId = "";

            await Assert.ThrowsAsync<ValidationException>(
                async () => await _posProfileService.GetAccesTokenForPartner(partnerId));
        }

        [Fact]
        public async Task GetAccesTokenForPartner_PosProfileNotExist_ReturnsNull()
        {
            var partnerId = "test-partner-id";

            _posProfileRepositoryMock
                .Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile> { });

            var accessToken = await _posProfileService.GetAccesTokenForPartner(partnerId);

            Assert.Null(accessToken);
        }

        [Fact]
        public async Task GetByMerchantId_PosProfileExist_IsSuccess()
        {
            var merchantId = "merchantId";
            var posProfile = new Models.PosProfile
            {
                PartnerId = "partnerId",
                PosConfiguration = new List<PosCredentialsConfiguration>
                {
                    new PosCredentialsConfiguration
                    {
                        MerchantId = merchantId,
                        PosType = "posType",
                        KeyVaultReference = "reference"
                    }
                }
            };

            _posProfileRepositoryMock
                .Setup(x => x.FindOne(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(posProfile);

            await _posProfileService.GetByMerchantId(merchantId);
        }

        [Fact]
        public async Task GetByMerchantId_PosProfileNotExist_ReturnsNull()
        {
            var merchantId = "merchantId";

            var result = await _posProfileService.GetByMerchantId(merchantId);

            Assert.Null(result);
        }


        [Fact]
        public async Task GetPosProfileAsync_CorrectParametersPassed_NotNullResultReturned()
        {
            var testInput = "testId";

            var posProfile = new Models.PosProfile
            {
                PartnerId = testInput,
                Id = new ObjectId(),
                PosConfiguration = Enumerable.Empty<PosCredentialsConfiguration>().ToArray()
            };

            _posProfileRepositoryMock.Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile> { posProfile });

            var invocation = _posProfileService.Invoking(x => x.GetPosProfileByPartnerId(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Subject
                .Should()
                .NotBeNull();
        }

        [Fact]
        public async Task GetPosProfileAsync_ProfileNotExist_NotFoundExceptionThrowed()
        {
            var testInput = "testId";

            _posProfileRepositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(default(Models.PosProfile));

            var invocation = _posProfileService.Invoking(x => x.GetPosProfileByPartnerId(testInput));
            var result = await invocation.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetPosProfilesAsync_IsHistoricalDataIngestedPassed_FilterByCalled()
        {
            var posProfileSearchCriteria = new PosProfileSearchCriteriaModel(isHistoricalDataIngested: true);

            _posProfileRepositoryMock.Verify();

            try
            {
                await _posProfileService.GetPosProfilesAsync(posProfileSearchCriteria);
            }
            catch (Exception e)
            {
                //ignored
            }

            _posProfileRepositoryMock
                .Verify(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()));
        }

        [Fact]
        public async Task GetPosProfilesAsync_PartnerNotFound_ExceptionThrowed()
        {
            var posProfileSearchCriteria = new PosProfileSearchCriteriaModel(isHistoricalDataIngested: true);

            var invocation = _posProfileService.Invoking(x => x.GetPosProfilesAsync(posProfileSearchCriteria));

            await invocation.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetPosProfilesByPartnerId_ProfilesDoNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var partnerId = "partnerId";

            // Act
            Func<Task> act = () => _posProfileService.GetPosProfileByPartnerId(partnerId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Pos profile for partnerId: {partnerId} not found.");
        }

        [Fact]
        public async Task GetPosProfilesByPartnerId_ProfilesExist_ShouldReturnPosProfilesDto()
        {
            // Arrange
            var partnerId = "partnerId";

            _posProfileRepositoryMock.Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(
                    new List<Models.PosProfile>
                    {
                        new()
                        {
                            PartnerId = partnerId,
                            PosConfiguration = new List<PosCredentialsConfiguration>
                            {
                                new()
                                {
                                    KeyVaultReference = "ref", PosType = "square"
                                }
                            }
                        }
                    });

            // Act
            var result = await _posProfileService.GetPosProfileByPartnerId(partnerId);

            // Assert
            result.Should().BeOfType<PosProfileDto>();
        }


        //[Fact]
        //public async Task CreatePosProfileAsync_RepositoryMethodReturnsError_PayloadLoggedAndExceptionHandledAndLogged()
        //{
        //    var exceptionMessage = "exceptionMessage";
        //    _posProfileRepositoryMock.Setup(
        //            x => x.InsertOne(It.IsAny<Models.PosProfile>()))
        //        .Throws(new MongoException(exceptionMessage));

        //    var creationDto = new PosProfileCreationModel(
        //        "partnerId",
        //        new List<Models.PosCredentialsConfigurationDto>
        //        {
        //            new Models.PosCredentialsConfigurationDto("Square", "AccessToken", "RefreshToken", DateTime.Today, "merchantId")
        //        }
        //    );

        //    try
        //    {
        //        await _posProfileService.CreatePosProfileAndSecretsAsync(creationDto);
        //    }
        //    catch (Exception e)
        //    {
        //        //ignored
        //    }

        //    _loggerMock.VerifyLogWasCalled("partnerId", LogLevel.Error);
        //}

        [Fact]
        public async Task UpdatePosProfileAsync_CorrectParametersPassed_RepositoryInvoked()
        {
            var partnerId = "partnerId";
            var posCredentialsConfigurationsList = new List<PosCredentialsConfigurationDto>
            {
                new PosCredentialsConfigurationDto(PosType: "square", KeyVaultReference: "test", "merchantId")
            };

            var updateDto = new PosProfileUpdateModel
            {
                PosConfigurations = posCredentialsConfigurationsList,
                IsHistoricalDataIngested = true
            };

            await _posProfileService.UpdatePosProfileAsync(partnerId, updateDto);

            _posProfileRepositoryMock.Verify(x => x.UpdateAsync(partnerId, It.Is<Models.PosProfile>(x => x.IsHistoricalDataIngested == true)));
        }

        [Fact]
        public async Task GetPosProfilesByPartnerIdsAsync_CorrectInput_Success()
        {
            // Arrange

            var partnerId = "test-partner-id";
            _posProfileRepositoryMock.Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(
                    new List<Models.PosProfile>
                    {
                        new()
                        {
                            PartnerId = partnerId,
                            PosConfiguration = new List<PosCredentialsConfiguration>
                            {
                                new()
                                {
                                    KeyVaultReference = "ref", PosType = "square"
                                }
                            }
                        }
                    });

            // Act

            var result = await _posProfileService.GetPosProfilesByPartnerIdsAsync(new List<string> { partnerId });

            // Assert

            result.Should().NotBeEmpty();
            result.First().PartnerId.Should().Be(partnerId);
        }

        [Fact]
        public async Task CreatePosProfileAsync_PassedSamePostType_ReturnValidationError()
        {
            var posType = "Square";
            var creationDto = new PosProfileCreationModel<PosCredentialsConfigurationSquareCreationDto>()
            {
                PartnerId = "partnerId",
                PosConfigurations = new PosCredentialsConfigurationSquareCreationDto(
                    posType, "AccessToken", "RefreshToken", DateTime.Today, "merchantId")
            };

            var posProfile = new Models.PosProfile
            {
                PartnerId = "partnerId",
                PosConfiguration = new List<PosCredentialsConfiguration>
                {
                    new PosCredentialsConfiguration
                    {
                        MerchantId = "merchantId",
                        PosType = posType,
                        KeyVaultReference = "reference"
                    }
                }
            };

            var service = new Mock<IPosCredentialsService<PosCredentialsConfigurationSquareCreationDto>>();
            service.Setup(x => x.OnboardingProcess("partnerId", creationDto.PosConfigurations)).ReturnsAsync(
                () => new PosCredentialsConfiguration() { PosType = "Square", MerchantId = "merchantId" });

            _posCredentialsServiceResolver.Setup(x => x.Resolve(creationDto.PosConfigurations)).Returns(service.Object);
            _posProfileRepositoryMock.Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile> { posProfile });

            var invocation = _posProfileService.Invoking(x => x.CreatePosProfileAndSecretsAsync(creationDto));
            await invocation.Should().ThrowAsync<Exception>().WithMessage($"Pos Configuration already exists for pos type : {posType}");
        }
    }
}