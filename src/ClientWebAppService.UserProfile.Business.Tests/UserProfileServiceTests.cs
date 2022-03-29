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

        public UserProfileServiceTests()
        {
            _service = new UserProfileService(_repositoryMock.Object, new Mock<ILogger<UserProfileService>>().Object, _emailServiceMock.Object);
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
    }
}