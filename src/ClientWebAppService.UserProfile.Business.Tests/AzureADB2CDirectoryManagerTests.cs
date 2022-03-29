using ClientWebAppService.UserProfile.Core.Exceptions;
using CXI.EmailService.Tests.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ClientWebAppService.UserProfile.Business.Tests
{
    public class AzureADB2CDirectoryManagerTests
    {
        public readonly IAzureADB2CDirectoryManager _manager;

        private Mock<IAzureADB2CDirectoryRepository> _repositoryMock = new Mock<IAzureADB2CDirectoryRepository>();
        private Mock<ILogger<AzureADB2CDirectoryManager>> _logger = new Mock<ILogger<AzureADB2CDirectoryManager>>();

        public AzureADB2CDirectoryManagerTests()
        {
            _manager = new AzureADB2CDirectoryManager(_repositoryMock.Object, _logger.Object);
        }

        [Fact]
        public async Task DeleteADB2CAccountByEmailAsync_UserAccountFound_ProperMethodsInoked()
        {
            var testInput = "test@mail.com";
            var testUserId = "testId";
            var testGraphServiceResponse = new GraphServiceUsersCollectionPage()
            {
                new Microsoft.Graph.User { Id = testUserId }
            };

            _repositoryMock.Setup(x => x.GetDirectoryUsersByEmail(testInput))
                           .ReturnsAsync(testGraphServiceResponse);

            await _manager.Invoking(x => x.DeleteADB2CAccountByEmailAsync(testInput))
                         .Should()
                         .NotThrowAsync();

            _repositoryMock.Verify(x => x.DeleteDirectoryUserByIdAsync(testUserId), Times.Once);            
        }

        [Fact]
        public async Task DeleteADB2CAccountByEmailAsync_UserAccountNotFound_ProperMessageLogged()
        {
            var exceptionMessagePart = "Azure ADB2C directory doesn't contain accounts with the email";
            var testInput = "test@mail.com";
            var testUserId = "testId";
            var testGraphServiceResponse = new GraphServiceUsersCollectionPage();

            _repositoryMock.Setup(x => x.GetDirectoryUsersByEmail(testInput))
                           .ReturnsAsync(testGraphServiceResponse);

            await _manager.Invoking(x => x.DeleteADB2CAccountByEmailAsync(testInput))
                         .Should()
                         .NotThrowAsync();

            _logger.VerifyLoggingForContainingMessagePart(exceptionMessagePart, LogLevel.Error);
        }

        [Fact]
        public async Task DeleteADB2CAccountByEmailAsync_MultipleUserAccountFound_ExceptionThrown()
        {
            var exceptionMessagePart = "Multiple accounts with the same email address";
            var testInput = "test@mail.com";
            var testUserId = "testId";
            var testGraphServiceResponse = new GraphServiceUsersCollectionPage()
            { 
                new Microsoft.Graph.User { Id = testUserId },
                new Microsoft.Graph.User { Id = "testId2" }
            };

            _repositoryMock.Setup(x => x.GetDirectoryUsersByEmail(testInput))
                           .ReturnsAsync(testGraphServiceResponse);

            await _manager.Invoking(x => x.DeleteADB2CAccountByEmailAsync(testInput))
                         .Should()
                         .ThrowAsync<MultipleADB2CAccountsDetectedException>();

            _logger.VerifyLoggingForContainingMessagePart(exceptionMessagePart, LogLevel.Error, Times.AtLeastOnce());
            _repositoryMock.Verify(x => x.DeleteDirectoryUserByIdAsync(testUserId), Times.Never);
        }
    }
}
