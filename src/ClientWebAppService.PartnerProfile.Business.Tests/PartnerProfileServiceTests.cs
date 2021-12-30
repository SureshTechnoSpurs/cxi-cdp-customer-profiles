using ClientWebAppService.PartnerProfile.Business.Models;
using ClientWebAppService.PartnerProfile.DataAccess;
using Moq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using CXI.Common.ExceptionHandling.Primitives;
using System.Linq.Expressions;
using System;
using ClientWebAppService.PartnerProfile.Business.Utils;
using ClientWebAppService.PartnerProfile.Models;
using Microsoft.Extensions.Logging;
using GL.MSA.ISC.Transport.RestClient;
using ClientWebAppService.PartnerProfile.Configuration;
using ClientWebAppService.PartnerProfile.Core.Utils;

namespace ClientWebAppService.PartnerProfile.Business.Tests
{
    public class PartnerProfileServiceTests
    {
        private readonly string BaseUrl = "http://test.cxi";
        private readonly IPartnerProfileService _service;
        private Mock<IPartnerRepository> _repositoryMock = new Mock<IPartnerRepository>();
        private Mock<IRestClientFactory> _restClientFactoryMock = new Mock<IRestClientFactory>();
        private Mock<IRestClient> _restClientMock = new Mock<IRestClient>();
        private Mock<IDomainServicesConfiguration> _configurationMock = new Mock<IDomainServicesConfiguration>();

        public PartnerProfileServiceTests()
        {
            _configurationMock
                .SetupGet(x => x.PosProfileService)
                .Returns(new PosProfileServiceConfiguration { BaseUrl = BaseUrl });

            _restClientFactoryMock.Setup(x => x.GetRestClient()).Returns(_restClientMock.Object);

            _service = new PartnerProfileService(_repositoryMock.Object,
                new Mock<ILogger<PartnerProfileService>>().Object,
                _configurationMock.Object,
                _restClientFactoryMock.Object,
                 new RequestDispatcher());
        }

        [Fact]
        public async Task CreateProfileAsync_CorrectParametersPassed_ExceptionNotThrowedAndNotNullResultReturned()
        {
            _repositoryMock.Setup(x => x.InsertOne(It.IsAny<Partner>()))
                .Returns(Task.CompletedTask);

            var testInput = new PartnerProfileCreationModel("testcorrect", "testname");

            var invocation = _service.Invoking(x => x.CreateProfileAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Subject
                  .Should()
                  .NotBeNull();
        }

        [Fact]
        public async Task CreateProfileAsync_CorrectParametersPassed_CorrectResult()
        {
            _repositoryMock.Setup(x => x.InsertOne(It.IsAny<Partner>()))
                .Returns(Task.CompletedTask);

            var testInput = new PartnerProfileCreationModel("testcorrect", "testname");

            var invocation = _service.Invoking(x => x.CreateProfileAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Subject
                  .Should()
                  .Match<PartnerProfileDto>(x =>
                  x.PartnerName == testInput.Name &&
                  x.Address == testInput.Address &&
                  x.PartnerType == PartnerProfileUtils.DefaultPartnerType &&
                  x.PartnerId == PartnerProfileUtils.GetPartnerIdByName(testInput.Name));
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

            var testInput = new PartnerProfileUpdateModel("test", "test", 10, "test", new[] { "test@mail.com" });

            var invocation = _service.Invoking(x => x.UpdateProfileAsync("testId", testInput));

            await invocation.Should().NotThrowAsync();
        }
    }
}
