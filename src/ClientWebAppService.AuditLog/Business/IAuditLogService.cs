using System.Threading.Tasks;
using CXI.Contracts.AuditLog.Models;

namespace ClientWebAppService.AuditLog.Business
{
    public interface IAuditLogService
    {
        /// <summary>
        /// Craete new auditlogs.
        /// </summary>
        Task CreateAuditLogsAsync(CreateAuditLogDto createAuditLogDto);

    }
}