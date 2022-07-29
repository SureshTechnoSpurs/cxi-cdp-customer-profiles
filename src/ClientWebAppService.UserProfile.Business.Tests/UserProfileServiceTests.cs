using ClientWebAppService.UserProfile.Core.Exceptions;
using ClientWebAppService.UserProfile.DataAccess;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Contracts.UserProfile.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace ClientWebAppService.UserProfile.Business.Tests
{
    public class UserProfileServiceTests
    {
        public readonly IUserProfileService _service;

        public Mock<IUserProfileRepository> _repositoryMock = new Mock<IUserProfileRepository>();
        public Mock<IEmailService> _emailServiceMock = new Mock<IEmailService>();
        public Mock<IAzureADB2CDirectoryManager> _azureADB2CDirectoryManagerMock = new Mock<IAzureADB2CDirectoryManager>();

        public UserProfileServiceTests()
        {
            _service = new UserProfileService(_repositoryMock.Object, new Mock<ILogger<UserProfileService>>().Object, _emailServiceMock.Object, _azureADB2CDirectoryManagerMock.Object);
        }

        [Fact]
        public async Task CreateProfileAsync_CorrectParametersPassed_NotNullResultReturned()
        {
            _repositoryMock.Setup(x => x.InsertOne(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var testInput = new UserCreationDto { Email = "testemail@mail.com", PartnerId = "testPartnerId", Role = UserRole.Owner };

            var invocation = _service.Invoking(x => x.CreateProfileAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Subject
                         .Should()
                         .NotBeNull();
        }

        [Fact]
        public async Task CreateProfileAsync_CorrectParametersPassed_CorrectResult()
        {
            _repositoryMock.Setup(x => x.InsertOne(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var testInput = new UserCreationDto { Email = "testemail@mail.com", PartnerId = "testPartnerId", Role = UserRole.Associate };

            var result = await _service.CreateProfileAsync(testInput);

            result.Should()
                  .NotBeNull()
                  .And
                  .Match<UserProfileDto>(x => x.Email == testInput.Email && x.PartnerId == testInput.PartnerId && x.Role == testInput.Role);
        }

        [Fact]
        public async Task CreateProfileAsync_AssociateUserPassed_InvitationSent()
        {
            _repositoryMock.Setup(x => x.InsertOne(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var testInput = new UserCreationDto { Email = "testemail@mail.com", PartnerId = "testPartnerId", Role = UserRole.Associate };

            var result = await _service.CreateProfileAsync(testInput);

            _emailServiceMock.Verify(mock => mock.SendInvitationMessageToAssociateAsync(testInput.Email), Times.Once);
        }

        [Fact]
        public async Task CreateProfileAsync_ExistingUserPassed_ValidationExceptionThrown()
        {
            var existingUser = new User { Email = "testemail@mail.com", PartnerId = "testPartnerId", Role = UserRole.Associate };

            _repositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(existingUser);

            var testInput = new UserCreationDto { Email = "testemail@mail.com", PartnerId = "testPartnerId", Role = UserRole.Associate };

            var invocation = _service.Invoking(x => x.CreateProfileAsync(testInput));
            var result = await invocation.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task GetUserProfilesAsync_UserProfilesFound_SuccessfulResultReturned()
        {
            var existingUsers = new List<UserProfileDto>()
            {
                new UserProfileDto("testPartnerId", "testemail@mail.com", UserRole.Associate, false),
                new UserProfileDto("testPartnerId", "testemail2@mail.com", UserRole.Associate, true)
            };

            _repositoryMock.Setup(
                x => x.FilterBy(It.IsAny<Expression<Func<User, UserProfileDto>>>(),
                It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(existingUsers);

            var testInput = new UserProfileSearchDto { PartnerId = "testPartnerId", Role = UserRole.Associate };

            var result = await _service.GetUserProfilesAsync(testInput);

            result.Should().Match<List<UserProfileDto>>(x => x.TrueForAll(element => element.Role == testInput.Role));
        }

        [Fact]
        public async Task GetUserProfilesAsync_UserProfilesNotFound_NotFoundExceptionThrown()
        {
            var existingUsers = new List<UserProfileDto>();

            _repositoryMock.Setup(
                x => x.FilterBy(It.IsAny<Expression<Func<User, UserProfileDto>>>(),
                It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(existingUsers);

            var testInput = new UserProfileSearchDto { PartnerId = "testPartnerId", Role = UserRole.Associate };

            var invocation = _service.Invoking(x => x.GetUserProfilesAsync(testInput));
            var result = await invocation.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateUserProfilesAsync_CorrectParametersPassed_UserProfileUpdated()
        {
            var initialUserProfileState = new User { Email = "testemail@mail.com", PartnerId = "testPartnerId", Role = UserRole.Associate, InvitationAccepted = false };

            _repositoryMock.Setup(
                x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(initialUserProfileState);

            var updateInput = new UserProfileUpdateDto
            {
                PartnerId = initialUserProfileState.PartnerId,
                Email = initialUserProfileState.Email,
                InvitationAccepted = true
            };
            await _service.UpdateUserProfilesAsync(updateInput);

            _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));
        }

        [Fact]
        public async Task GetByEmail_CorrectParametersPassed_SuccessfulResult()
        {
            var testInput = "test@mail.com";

            _repositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<User, bool>>>()))
                           .ReturnsAsync(new User { Email = testInput });

            var invocation = _service.Invoking(x => x.GetByEmailAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Subject
                  .Should()
                  .NotBeNull()
                  .And
                  .Match<UserProfileDto>(x => x.Email == testInput);
        }

        [Fact]
        public async Task GetByEmail_ProfileNotExist_NotFoundExceptionThrowed()
        {
            var testInput = "test@mail.com";

            _repositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<User, bool>>>()))
                           .ReturnsAsync(default(User));

            var invocation = _service.Invoking(x => x.GetByEmailAsync(testInput));
            var result = await invocation.Should().ThrowAsync<NotFoundException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("testmail.com")]
        [InlineData("@mail.com")]
        public async Task GetByEmail_IncorrectEmailPassed_ValidationException_Throwed(string email)
        {
            var invocation = _service.Invoking(x => x.GetByEmailAsync(email));
            var result = await invocation.Should().ThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("testmail.com")]
        [InlineData("@mail.com")]
        public async Task DeleteProfileByEmailAsync_IncorrectEmailPassed_ValidationException_Throwed(string email)
        {
            var invocation = _service.Invoking(x => x.DeleteProfileByEmailAsync(email));
            var result = await invocation.Should().ThrowAsync<ValidationException>();
        }


        [Fact]
        public async Task DeleteProfileByEmailAsync_UserProfileNotFound_NotFoundExceptionThrown()
        {
            var testInput = "test@mail.com";

            _repositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<User, bool>>>()))
                           .ReturnsAsync(default(User));

            var invocation = _service.Invoking(x => x.DeleteProfileByEmailAsync(testInput));
            var result = await invocation.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DeleteProfileByEmailAsync_UserProfileAssociateFound_ProperMethodsInoked()
        {
            var testInput = "test@mail.com";
            var existingUser = new User { Email = testInput, PartnerId = "testPartnerId", Role = UserRole.Associate };

            _repositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<User, bool>>>()))
                           .ReturnsAsync(existingUser);

            await _service.Invoking(x => x.DeleteProfileByEmailAsync(testInput))
                         .Should()
                         .NotThrowAsync();

            _repositoryMock.Verify(x => x.DeleteOne(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
            _azureADB2CDirectoryManagerMock.Verify(x => x.DeleteADB2CAccountByEmailAsync(testInput), Times.Once);
        }

        [Fact]
        public async Task DeleteProfileByEmailAsync_UserProfileOwnerFound_ProperMethodsInvoked()
        {
            var inputEmail = "test@mail.com";
            var testPartnerId = "testPartnerId";
            var existingUser = new User { Email = inputEmail, PartnerId = testPartnerId, Role = UserRole.Owner };        

            _repositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<User, bool>>>()))
                           .ReturnsAsync(existingUser);

            await _service.Invoking(x => x.DeleteProfileByEmailAsync(inputEmail))
                         .Should()
                         .ThrowAsync<OwnerDeletionForbiddenException>();
        }

        [Fact]
        public async Task GetUsersCountByPartners_UsersFound_GetUsersCountByPartners()
        {
            var partnerIds = new List<string> { "partnerID_1", "partnerID_2" , "partnerID_3" };
            var users = new List<User>()
            {
                new() { PartnerId = "partnerID_1"},
                new() { PartnerId = "partnerID_1"},
                new() { PartnerId = "partnerID_1"},
                new() { PartnerId = "partnerID_1"},
                new() { PartnerId = "partnerID_1"},
                new() { PartnerId = "partnerID_2"}
            };

            _repositoryMock.Setup(r => r.FilterBy(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(users);

            var result = await _service.GetUsersCountByPartners(partnerIds);

            Assert.NotNull(result);
            Assert.Equal(partnerIds.Count, result.Count);
            Assert.Contains("partnerID_1", (IDictionary<string,int>)result);
            Assert.Equal(5, result.GetValueOrDefault("partnerID_1"));
            Assert.Equal(1, result.GetValueOrDefault("partnerID_2"));
            Assert.Equal(0, result.GetValueOrDefault("partnerID_3"));
        }
    }
}