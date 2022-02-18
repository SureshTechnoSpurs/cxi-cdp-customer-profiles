using Azure;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Models;
using ClientWebAppService.PosProfile.Services;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using PosCredentialsConfigurationDto = CXI.Contracts.PosProfile.Models.PosCredentialsConfigurationDto;

namespace ClientWebAppService.PosProfile.Tests
{
    public class PosProfileServiceTests
    {
        private const string SecretNotFoundErrorCode = "SecretNotFound";

        private IPosProfileService _posProfileService;
        private readonly Mock<IPosProfileRepository> _posProfileRepositoryMock;
        private readonly Mock<ISecretSetter> _secretSetterMock;
        private readonly Mock<ILogger<PosProfileService>> _loggerMock;
        private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock;
        private readonly Mock<ISecretClient> _secretClientMock;

        public PosProfileServiceTests()
        {
            _posProfileRepositoryMock = new Mock<IPosProfileRepository>();
            _configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            _posProfileRepositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()));
            _secretSetterMock = new Mock<ISecretSetter>();
            _loggerMock = new Mock<ILogger<PosProfileService>>();
            _secretClientMock = new Mock<ISecretClient>();

            _posProfileService = new PosProfileService(
                _posProfileRepositoryMock.Object,
                _secretSetterMock.Object,
                _loggerMock.Object,
                _configurationMock.Object,
                _secretClientMock.Object);
        }

        [Fact]
        public async Task GetPosProfileAsync_ProfileNotExist_NotFoundExceptionThrowed()
        {
            var testInput = "testId";

            _posProfileRepositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                                     .ReturnsAsync(default(Models.PosProfile));

            var invocation = _posProfileService.Invoking(x => x.FindPosProfileByPartnerIdAsync(testInput));
            var result = await invocation.Should().ThrowAsync<NotFoundException>();
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

            _posProfileRepositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                                     .ReturnsAsync(posProfile);

            var invocation = _posProfileService.Invoking(x => x.FindPosProfileByPartnerIdAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Subject
                .Should()
                .NotBeNull();
        }

        [Fact]
        public async Task CreatePosProfileAsync_CorrectParametersPassed_SuccessfulResultReturned()
        {
            var creationDto = new PosProfileCreationModel(
                "partnerId",
                new List<Models.PosCredentialsConfigurationDto>
                {
                    new Models.PosCredentialsConfigurationDto("Square", "AccessToken", "RefreshToken", DateTime.Today)
                }
            );

            var invocation = _posProfileService.Invoking(x => x.CreatePosProfileAndSecretsAsync(creationDto));

            await invocation.Should().NotThrowAsync();
        }

        [Fact]
        public async Task CreatePosProfileAsync_CorrectParametersPassed_SecretSetterSetInvokedTwiceWithCorrectParameters()
        {
            var date = DateTime.ParseExact("2021-09-30T23:00:00.00Z",
                "yyyy-MM-dd'T'HH:mm:ss.ff'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal);

            var creationDto = new PosProfileCreationModel(
                "partnerId",
                new List<Models.PosCredentialsConfigurationDto>
                {
                    new Models.PosCredentialsConfigurationDto("square", "AccessToken", "RefreshToken", date)
                }
            );

            var keyVaultItemNameExpected = $"partnerId-square";
            var keyVaultItemValueExpected = @"{""AccessToken"":{""Value"":""AccessToken"",""ExpirationDate"":""2021-09-30T23:00:00Z""},""RefreshToken"":{""Value"":""RefreshToken"",""ExpirationDate"":null}}";

            await _posProfileService.CreatePosProfileAndSecretsAsync(creationDto);

            _secretSetterMock.Verify(
                a => a.Set(
                    It.Is<string>(x => x == keyVaultItemNameExpected), It.Is<string>(x => x == keyVaultItemValueExpected), null)
            );

            _secretSetterMock.Verify(ss => ss.Set("di-square-partnerId-tokeninfo", "Bearer AccessToken", null));
        }

        [Fact]
        public async Task CreatePosProfileAsync_RepositoryMethodReturnsError_PayloadLoggedAndExceptionHandledAndLogged()
        {
            var exceptionMessage = "exceptionMessage";
            _posProfileRepositoryMock.Setup(
                    x => x.InsertOne(It.IsAny<Models.PosProfile>()))
                .Throws(new MongoException(exceptionMessage));

            var creationDto = new PosProfileCreationModel(
                "partnerId",
                new List<Models.PosCredentialsConfigurationDto>
                {
                    new Models.PosCredentialsConfigurationDto("Square", "AccessToken", "RefreshToken", DateTime.Today)
                }
            );

            try
            {
                await _posProfileService.CreatePosProfileAndSecretsAsync(creationDto);
            }
            catch (Exception e)
            {
                //ignored
            }

            _loggerMock.VerifyLogWasCalled("partnerId", LogLevel.Error);
        }

        [Fact]
        public async Task UpdatePosProfileAsync_CorrectParametersPassed_RepositoryInvoked()
        {
            var partnerId = "partnerId";
            var posCredentialsConfigurationsList = new List<PosCredentialsConfigurationDto> {
                new PosCredentialsConfigurationDto(PosType : "square", KeyVaultReference : "test")};

            var updateDto = new PosProfileUpdateModel
            {
                PosConfigurations = posCredentialsConfigurationsList,
                IsHistoricalDataIngested = true
            };

            await _posProfileService.UpdatePosProfileAsync(partnerId, updateDto);

            _posProfileRepositoryMock.Verify(x => x.UpdateAsync(partnerId, It.Is<Models.PosProfile>(x => x.IsHistoricalDataIngested == true)));
        }

        [Fact]
        public async Task GetPosProfilesAsync_PartnerNotFound_ExceptionThrowed()
        {
            var posProfileSearchCriteria = new PosProfileSearchCriteriaModel(isHistoricalDataIngested: true);

            var invocation = _posProfileService.Invoking(x => x.GetPosProfilesAsync(posProfileSearchCriteria));

            await invocation.Should().ThrowAsync<NotFoundException>();
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
        public async Task DeletePosProfileAndSecretsAsync_RemovedSecretsAndPosProfile_Success()
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

            _posProfileRepositoryMock
                .Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile> { posProfile });

            await _posProfileService.DeletePosProfileAndSecretsAsync(partnerId);

            _secretClientMock.Verify(x => x.DeleteSecretAsync(It.IsAny<string>()), Times.Exactly(2));
            _posProfileRepositoryMock.Verify(x => x.DeleteMany(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task DeletePosProfileAndSecretsAsync_SecretNotFound_RequestFailedExceptionThrown()
        {
            var partnerId = "test-partner-id";
            var expectedMessage = $"Secret was not found in key vault for ${partnerId}";
            var exception = new RequestFailedException(1, expectedMessage, SecretNotFoundErrorCode, null);
            var posProfile = new Models.PosProfile
            {
                Id = new ObjectId(),
                PartnerId = partnerId,
                PosConfiguration = new List<PosCredentialsConfiguration>
                {
                    new PosCredentialsConfiguration { KeyVaultReference = "ref", PosType = "square" }
                }
            };

            _posProfileRepositoryMock
                .Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile> { posProfile });

            _secretClientMock
                .Setup(x => x.DeleteSecretAsync(It.IsAny<string>()))
                .Throws(exception);

            await Assert.ThrowsAsync<RequestFailedException>(async () => await _posProfileService.DeletePosProfileAndSecretsAsync(partnerId));

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
        public async Task DeletePosProfileAndSecretsAsync_PosProfilesNotFound_Success()
        {
            var partnerId = "test-partner-id";

            _posProfileRepositoryMock
                .Setup(x => x.FilterBy(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(new List<Models.PosProfile>());

            await _posProfileService.DeletePosProfileAndSecretsAsync(partnerId);
            
            _secretClientMock.Verify(x => x.DeleteSecretAsync(It.IsAny<string>()), Times.Never);
            _posProfileRepositoryMock.Verify(x => x.DeleteMany(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()), Times.Never);
        }
    }
}