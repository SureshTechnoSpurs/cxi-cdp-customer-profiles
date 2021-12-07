using Moq;
using Xunit;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Configuration;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Models;
using ClientWebAppService.PosProfile.Services;
using CXI.Common.Security.Secrets;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using CXI.Common.ExceptionHandling.Primitives;
using MongoDB.Bson;

namespace ClientWebAppService.PosProfile.Tests
{
    public class PosProfileServiceTests
    {
        private IPosProfileService _posProfileService;
        private readonly Mock<IPosProfileRepository> _posProfileRepositoryMock;
        private readonly Mock<ISecretSetter> _secretSetterMock;
        private readonly Mock<ILogger<PosProfileService>> _loggerMock;
        private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock;

        public PosProfileServiceTests()
        {
            _posProfileRepositoryMock = new Mock<IPosProfileRepository>();
            _configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            _posProfileRepositoryMock.Setup(
                 x => x.FindOne(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()));

            _secretSetterMock = new Mock<ISecretSetter>();

            _loggerMock = new Mock<ILogger<PosProfileService>>();

            _posProfileService = new PosProfileService(_posProfileRepositoryMock.Object, _secretSetterMock.Object, _loggerMock.Object, _configurationMock.Object);
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
            var creationDto = new PosProfileCreationDto(
                "partnerId",
                new List<PosCredentialsConfigurationDto>
                {
                    new PosCredentialsConfigurationDto("Square", "AccessToken", "RefreshToken", DateTime.Today)
                }
            );

            var invocation = _posProfileService.Invoking(x => x.CreatePosProfileAsync(creationDto));

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

            var creationDto = new PosProfileCreationDto(
                "partnerId",
                new List<PosCredentialsConfigurationDto>
                {
                    new PosCredentialsConfigurationDto("square", "AccessToken", "RefreshToken", date)
                }
            );

            var keyVaultItemNameExpected = $"partnerId-square";
            var keyVaultItemValueExpected = @"{""AccessToken"":{""Value"":""AccessToken"",""ExpirationDate"":""2021-09-30T23:00:00Z""},""RefreshToken"":{""Value"":""RefreshToken"",""ExpirationDate"":null}}";

            await _posProfileService.CreatePosProfileAsync(creationDto);

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

            var creationDto = new PosProfileCreationDto(
                "partnerId",
                new List<PosCredentialsConfigurationDto>
                {
                    new PosCredentialsConfigurationDto("Square", "AccessToken", "RefreshToken", DateTime.Today)
                }
            );

            try
            {
                await _posProfileService.CreatePosProfileAsync(creationDto);
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
            var posCredentialsConfigurationsList = new List<PosCredentialsConfiguration> {new PosCredentialsConfiguration
                {PosType = "square", KeyVaultReference = "test"}};
            
            var updateDto = new PosProfileUpdateModel(posCredentialsConfigurationsList, true);
            
            await _posProfileService.UpdatePosProfileAsync(partnerId, updateDto);
            
            _posProfileRepositoryMock.Verify(x => x.UpdateAsync(partnerId, It.Is<Models.PosProfile>(x => x.IsHistoricalDataIngested == true 
            && x.PosConfiguration == posCredentialsConfigurationsList)));
        }

        [Fact]
        public async Task GetPosProfilesAsync_PartnerNotFound_ExceptionThrowed()
        {
            var posProfileSearchCriteria = new PosProfileSearchCriteria
            {
                IsHistoricalDataIngested = true
            };

            var invocation = _posProfileService.Invoking(x => x.GetPosProfilesAsync(posProfileSearchCriteria));
            
            await invocation.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetPosProfilesAsync_IsHistoricalDataIngestedPassed_FilterByCalled()
        {
            var posProfileSearchCriteria = new PosProfileSearchCriteria
            {
                IsHistoricalDataIngested = true
            };
            
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
    }
}