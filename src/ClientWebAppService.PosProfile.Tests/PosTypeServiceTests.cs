using Moq;
using Xunit;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Models;
using ClientWebAppService.PosProfile.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using CXI.Common.ExceptionHandling.Primitives;
using MongoDB.Bson;

namespace ClientWebAppService.PosProfile.Tests
{
    public class PosTypeServiceTests
    {
        private IPosTypeService _posTypeService;
        private readonly Mock<IPosProfileRepository> _posProfileRepositoryMock;
        private readonly Mock<ILogger<PosTypeService>> _loggerMock;

        public PosTypeServiceTests()
        {
            _posProfileRepositoryMock = new Mock<IPosProfileRepository>();            

            _loggerMock = new Mock<ILogger<PosTypeService>>();

            _posTypeService = new PosTypeService(_posProfileRepositoryMock.Object, _loggerMock.Object);
        }


        [Fact]
        public async Task GetActivePartnersByPosTypeAsync_NoProfilesFound_NotFoundExceptionThrown()
        {
            var posType = "square";
            var empyList = new List<string>();

            _posProfileRepositoryMock.Setup(
                  x => x.FilterBy<string>(It.IsAny<Expression<Func<Models.PosProfile, string>>>(),
                  It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(empyList);

            var invocation = _posTypeService.Invoking(x => x.GetPosProfileIdsByPosTypeAsync(posType));

            await invocation.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetActivePartnersByPosTypeAsync_ProfilesFound_CorrectResultReturned()
        {
            var posType = "square";
            var squarePosProfiles = new List<string> { "cxi-usa-test1", "cxi-usa-test2" };

            _posProfileRepositoryMock.Setup(
                  x => x.FilterBy<string>(It.IsAny<Expression<Func<Models.PosProfile, string>>>(),
                  It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(squarePosProfiles);

            var result = await _posTypeService.GetPosProfileIdsByPosTypeAsync(posType);

            result.Count().Should().Be(squarePosProfiles.Count);
        }

        [Fact]
        public async Task GetActivePartnersByPosTypeAsync_CorrectParametersPassed_FilterByCalled()
        {
            var posType = "square";
            var squarePosProfiles = new List<string> { "cxi-usa-test1", "cxi-usa-test2" };

            _posProfileRepositoryMock.Setup(
                  x => x.FilterBy<string>(It.IsAny<Expression<Func<Models.PosProfile, string>>>(),
                  It.IsAny<Expression<Func<Models.PosProfile, bool>>>()))
                .ReturnsAsync(squarePosProfiles);

            await _posTypeService.GetPosProfileIdsByPosTypeAsync(posType);

            _posProfileRepositoryMock.Verify(x => x.FilterBy<string>(It.IsAny<Expression<Func<Models.PosProfile, string>>>(), It.IsAny<Expression<Func<Models.PosProfile, bool>>>()));
        }
    }
}
