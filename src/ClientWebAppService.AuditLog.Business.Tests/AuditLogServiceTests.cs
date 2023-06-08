using CXI.Contracts.AuditLog.Models;
using CXI.Common.AuditLog;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ClientWebAppService.AuditLog.Business.Tests
{
    public class AuditLogServiceTests
    {
        private readonly IAuditLogService _service;
        private ILogger<AuditLogService> _logger;
        public Mock<IAuditLog> _auditLogServiceClientMock = new Mock<IAuditLog>();
        public AuditLogServiceTests()
        {
            _service = new AuditLogService(
                 new Mock<ILogger<AuditLogService>>().Object,
                _auditLogServiceClientMock.Object);
        }

        [Fact]
        public async Task CreateAuditLogsAsync_CorrectParametersPassed_ExceptionNotThrowedAndNotNullResultReturned()
        {
            _auditLogServiceClientMock.Setup(x => x.InsertAuditLogs(It.IsAny<PartnerAuditLogEntity>()))
                .Returns(Task.CompletedTask);
            var testInput = new CreateAuditLogDto("testpartnerid", "testuseremail@customerxi.com", "testname", "testlogin", "https://devtest.cxicodes.com", "login click", DateTime.UtcNow, "US", "Web", "Chrome", "Username: admin login", "", "", "");
            var invocation = _service.Invoking(x => x.CreateAuditLogsAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAuditLogsAsync_CorrectParametersPassed_CorrectResult()
        {
            _auditLogServiceClientMock.Setup(x => x.InsertAuditLogs(It.IsAny<PartnerAuditLogEntity>()))
                .Returns(Task.CompletedTask);

            var testInput = new CreateAuditLogDto("testpartnerid", "testuseremail@customerxi.com", "testname", "testlogin", "https://devtest.cxicodes.com", "login click", DateTime.UtcNow, "US", "Web", "Chrome", "Username: admin login", "", "", "");
            var invocation = _service.Invoking(x => x.CreateAuditLogsAsync(testInput));
            var result = await invocation.Should().NotThrowAsync();

            _auditLogServiceClientMock.Verify(x => x.InsertAuditLogs(It.IsAny<PartnerAuditLogEntity>()));
        }
    }
}
