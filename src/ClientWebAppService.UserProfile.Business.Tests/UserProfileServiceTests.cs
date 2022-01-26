using ClientWebAppService.UserProfile.DataAccess;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Contracts.UserProfile.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace ClientWebAppService.UserProfile.Business.Tests
{
    public class UserProfileServiceTests
    {
        public readonly IUserProfileService _service;

        public Mock<IUserProfileRepository> _repositoryMock = new Mock<IUserProfileRepository>();

        public UserProfileServiceTests()
        {
            _service = new UserProfileService(_repositoryMock.Object, new Mock<ILogger<UserProfileService>>().Object);
        }

        [Fact]
        public async Task CreateProfile_CorrectParametersPassed_NotNullResultReturned()
        {
            _repositoryMock.Setup(x => x.InsertOne(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var testInput = new UserCreationModel { Email = "testemail@mail.com", PartnerId = "testPartnerId", Role = "owner" };

            var invocation = _service.Invoking(x => x.CreateProfileAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Subject
                         .Should()
                         .NotBeNull();
        }

        [Fact]
        public async Task CreateProfile_CorrectParametersPassed_CorrectResult()
        {
            _repositoryMock.Setup(x => x.InsertOne(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var testInput = new UserCreationModel { Email = "testemail@mail.com", PartnerId = "testPartnerId", Role = "owner" };

            var result = await _service.CreateProfileAsync(testInput);

            result.Should()
                  .NotBeNull()
                  .And
                  .Match<UserProfileDto>(x => x.Email == testInput.Email && x.PartnerId == testInput.PartnerId && x.Role == testInput.Role);
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
