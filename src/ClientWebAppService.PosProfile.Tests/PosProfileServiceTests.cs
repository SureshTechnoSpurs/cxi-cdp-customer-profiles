using Moq;
using Xunit;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Models;
using ClientWebAppService.PosProfile.Services;
using CXI.Common.Security.Secrets;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace ClientWebAppService.PosProfile.Tests
{
    public class PosProfileServiceTests
    {
        private IPosProfileService _posProfileService;
        private readonly Mock<IPosProfileRepository> _posProfileRepositoryMock;
        private readonly Mock<ISecretSetter> _secretSetterMock;
        private readonly Mock<ILogger<PosProfileService>> _loggerMock;
        
        public PosProfileServiceTests()
        {
            _posProfileRepositoryMock = new Mock<IPosProfileRepository>();
            _posProfileRepositoryMock.Setup(
                 x => x.FindOne(It.IsAny<Expression<Func<Models.PosProfile, bool>>>()));

            _secretSetterMock = new Mock<ISecretSetter>();

            _loggerMock = new Mock<ILogger<PosProfileService>>();
            
            _posProfileService = new PosProfileService(_posProfileRepositoryMock.Object, _secretSetterMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreatePosProfileAsync_CorrectParametersPassed_SuccessfulResultReturned()
        {
            _posProfileService = new PosProfileService(_posProfileRepositoryMock.Object, _secretSetterMock.Object, _loggerMock.Object);

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
        public async Task CreatePosProfileAsync_CorrectParametersPassed_SecretSetterSetInvokedWithCorrectParameters()
        {
            _posProfileService = new PosProfileService(_posProfileRepositoryMock.Object, _secretSetterMock.Object, _loggerMock.Object);

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
        }
        
        [Fact]
        public async Task CreatePosProfileAsync_RepositoryMethodReturnsError_PayloadLoggedAndExceptionHandledAndLogged()
        {
            _posProfileService = new PosProfileService(_posProfileRepositoryMock.Object, _secretSetterMock.Object, _loggerMock.Object);

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
    }
}