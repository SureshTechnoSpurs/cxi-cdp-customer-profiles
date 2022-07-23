using ClientWebAppService.PartnerProfile.DataAccess;
using Moq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using CXI.Common.ExceptionHandling.Primitives;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using ClientWebAppService.PartnerProfile.Business.Utils;
using Microsoft.Extensions.Logging;
using CXI.Contracts.PosProfile;
using CXI.Contracts.PartnerProfile.Models;
using CXI.Common.Models.Pagination;

namespace ClientWebAppService.PartnerProfile.Business.Tests
{
    public class PartnerProfileServiceTests
    {
        private readonly IPartnerProfileService _service;
        private readonly Mock<IPartnerRepository> _repositoryMock = new Mock<IPartnerRepository>();
        private readonly Mock<IPosProfileServiceClient> _posProfileServiceClientMock = new Mock<IPosProfileServiceClient>();

        public PartnerProfileServiceTests()
        {
            _service = new PartnerProfileService(
                _repositoryMock.Object,
                new Mock<ILogger<PartnerProfileService>>().Object,
                _posProfileServiceClientMock.Object);
        }

        [Fact]
        public async Task CreateProfileAsync_CorrectParametersPassed_ExceptionNotThrowedAndNotNullResultReturned()
        {
            _repositoryMock.Setup(x => x.InsertOne(It.IsAny<Partner>()))
                .Returns(Task.CompletedTask);

            var testInput = new PartnerProfileCreationModel { Address = "testcorrect", Name = "testname" };

            var invocation = _service.Invoking(x => x.CreateProfileAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Subject
                  .Should()
                  .NotBeNull();
        }

        [Fact]
        public async Task CreateProfileAsync_CorrectParametersPassed_SuchPartnerAlreadyExisted_ValidationExceptionThrowed()
        {
            _repositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Partner, bool>>>()))
                .ReturnsAsync(new Partner());

            var testInput = new PartnerProfileCreationModel { Address = "testcorrect", Name = "testname" };

            var invocation = _service.Invoking(x => x.CreateProfileAsync(testInput));
            await invocation.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateProfileAsync_CorrectParametersPassed_CorrectResult()
        {
            _repositoryMock.Setup(x => x.InsertOne(It.IsAny<Partner>()))
                .Returns(Task.CompletedTask);

            var testInput = new PartnerProfileCreationModel { Address = "testcorrect", Name = "testname" };

            var invocation = _service.Invoking(x => x.CreateProfileAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Subject
                  .Should()
                  .Match<PartnerProfileDto>(x =>
                  x.PartnerName == testInput.Name &&
                  x.Address == testInput.Address &&
                  x.PartnerType == PartnerProfileUtils.DefaultPartnerType &&
                  x.PartnerId == PartnerProfileUtils.GetPartnerIdByName(testInput.Name) &&
                  x.ServiceAgreementAccepted == false);
        }

        [Fact]
        public async Task GetByIdAsync_CorrectParametersPassed_SuccessfulResult()
        {
            var testInput = "testId";

            _repositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Partner, bool>>>()))
                           .ReturnsAsync(new Partner { PartnerId = testInput });

            var invocation = _service.Invoking(x => x.GetByIdAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Subject
                  .Should()
                  .NotBeNull()
                  .And
                  .Match<PartnerProfileDto>(x => x.PartnerId == testInput);
        }

        [Fact]
        public async Task GetByIdAsync_ProfileNotExist_NotFoundExceptionThrowed()
        {
            var testInput = "testId";

            _repositoryMock.Setup(x => x.FindOne(It.IsAny<Expression<Func<Partner, bool>>>()))
                           .ReturnsAsync(default(Partner));

            var invocation = _service.Invoking(x => x.GetByIdAsync(testInput));
            var result = await invocation.Should().ThrowAsync<NotFoundException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetByIdAsync_IncorrectPartnerIdPassed_ValidationException_Throwed(string partnerId)
        {
            var invocation = _service.Invoking(x => x.GetByIdAsync(partnerId));
            var result = await invocation.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateProfileAsync_CorrectParametersPassed_SuccessfulResult()
        {
            _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<Partner>()))
                .Returns(Task.CompletedTask);

            var testInput = new PartnerProfileUpdateModel("test", "test", 10, "test", true, new[] { "test@mail.com" }, true, new Subscription(), "v2", null);

            var invocation = _service.Invoking(x => x.UpdateProfileAsync("testId", testInput));

            await invocation.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetPartnerProfilesAsync_ProfilesExist_ShouldReturnPartnerProfiles()
        {
            // Arrange
            _repositoryMock.Setup(x => x.FilterBy(It.IsAny<Expression<Func<Partner, bool>>>()))
                .ReturnsAsync(new List<Partner>
                {
                    new()
                    {
                        PartnerId = "partnerId"
                    }
                });

            // Act
            var result = await _service.GetPartnerProfilesAsync();

            // Assert
            result.Should().AllBeOfType<PartnerProfileDto>();
        }

        [Fact]
        public async Task GetPartnerProfilesAsync_ProfilesDoNotExist_ShouldThrowNotFoundException()
        {
            // Act
            Func<Task> act = () => _service.GetPartnerProfilesAsync();

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Partner profiles don't exist.");
        }

        [Fact]
        public async Task GetPartnerIds_VerifyIdExistsWithActiveTrue_ReturnPartnerIds()
        {
            _repositoryMock.Setup(x => x.FilterBy(It.IsAny<Expression<Func<Partner, bool>>>()))
                .ReturnsAsync(new List<Partner>
                {
                    new()
                    {
                        PartnerId = "partnerId"
                    }
                });

            var result = await _service.SearchPartnerIdsByActiveStateAsync(true);

            result.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetPartnerProfilesPaginatedAsync_ProfilesExist_ShouldReturnPartnerProfilePaginatedDto()
        {
            // Arrange

            _repositoryMock
                .Setup(x => x.GetPaginatedList(It.IsAny<PaginationRequest>(), It.IsAny<Expression<Func<Partner, bool>>>()))
                .ReturnsAsync(new PaginatedResponse<Partner>
                {
                    Items = new List<Partner>
                    {
                        new Partner { PartnerId = "" }
                    },
                    PageIndex = 1,
                    PageSize = 1,
                    TotalCount = 1,
                    TotalPages = 1
                });

            // Act
            var result = await _service.GetPartnerProfilesPaginatedAsync(1, 1);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task UpdatePartnerSubscriptionAsync_CorrectParametersPassed_SuccessfulResult()
        {
            _repositoryMock
                .Setup(x => x.UpdateSubscriptionAsync(It.IsAny<string>(), It.IsAny<Subscription>()))
                .Returns(Task.CompletedTask);

            var updateModel = new SubscriptionUpdateModel 
            {
                LastBilledDate = DateTime.UtcNow,
                State = SubscriptionState.Active,
                SubscriptionId = 1
            };

            var invocation = _service.Invoking(x => x.UpdatePartnerSubscriptionAsync("testId", updateModel));

            await invocation.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SetPartnerActivityStatusAsync_CorrectParams_Success()
        {
            _repositoryMock
                .Setup(x => x.FindOne(It.IsAny<Expression<Func<Partner, bool>>>()))
                .ReturnsAsync(new Partner());

            await _service.SetPartnerActivityStatusAsync("test_partner", true);

            _repositoryMock
                .Verify(x => x.SetActivityStatus(It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        }
    }
}
