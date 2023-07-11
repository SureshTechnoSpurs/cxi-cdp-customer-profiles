using ClientWebAppService.AuditLog.Business;
using CXI.Common.ExceptionHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CXI.Contracts.AuditLog.Models;

namespace ClientWebAppService.AuditLog.Controllers
{
    /// <summary>
    /// Provide functionality for create auditlog information.
    /// </summary>
    [Route("api/auditlog")]
    [Route("api/v{version:apiVersion}/auditlog")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    [ExcludeFromCodeCoverage]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> CreateAuditLogs(CreateAuditLogDto createAuditLogDto)
        {
            await _auditLogService.CreateAuditLogsAsync(createAuditLogDto);

            return NoContent();
        }

    }
}
