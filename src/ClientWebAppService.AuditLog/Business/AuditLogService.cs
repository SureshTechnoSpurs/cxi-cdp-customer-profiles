using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using CXI.Common.AuditLog;
using CXI.Contracts.AuditLog.Models;
using MongoDB.Bson;
using System;

namespace ClientWebAppService.AuditLog.Business
{
    ///<inheritdoc/>
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLog _auditLog;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(ILogger<AuditLogService> logger, IAuditLog auditLog)
        {
            _logger = logger;
            _auditLog = auditLog;

        }

        ///<inheritdoc/>
        public async Task CreateAuditLogsAsync(CreateAuditLogDto createAuditLogDto)
        {
            //create audit logs
            var eventDate = createAuditLogDto.EventDate == "" || createAuditLogDto.EventDate == string.Empty ? DateTime.UtcNow : DateTimeOffset.Parse(createAuditLogDto.EventDate).UtcDateTime;

            var createAuditLog = new PartnerAuditLogEntity()
            {
                Id = ObjectId.GenerateNewId(),
                PartnerId = createAuditLogDto.PartnerId,
                UserEmail = createAuditLogDto.UserEmail,
                DisplayName = createAuditLogDto.DisplayName,
                EntityName = createAuditLogDto.EntityName,
                PageVisited = createAuditLogDto.PageVisited,
                EventName = createAuditLogDto.EventName,
                EventDate = eventDate,
                TimeZone = createAuditLogDto.TimeZone,
                Device = createAuditLogDto.Device,
                Browser = createAuditLogDto.Browser,
                Message = createAuditLogDto.Message,
                Custom1 = createAuditLogDto.Custom1,
                Custom2 = createAuditLogDto.Custom2,
                Custom3 = createAuditLogDto.Custom3
            };

            await _auditLog.InsertAuditLogs(createAuditLog);
        }
    }
}
