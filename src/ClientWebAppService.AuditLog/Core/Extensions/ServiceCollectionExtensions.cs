using ClientWebAppService.AuditLog.Business;
using CXI.Common.ApplicationInsights;
using CXI.Common.Authorization;
using CXI.Common.AuditLog;
using CXI.Common.MongoDb.Extensions;
using GL.MSA.Core.HealthCheck.Concrete;
using GL.MSA.Core.HealthCheck.HealthCheckExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using FluentValidation.AspNetCore;

namespace ClientWebAppService.AuditLog.Core.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterService(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            services.AddAzureAdB2CUserAuthentication(configuration, logger);
            services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
            services.AddTraceExtentionDispatcher(configuration)
                .AddHealthChecks()
                .AddCheck<LivenessHealthCheck>(name: "live",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "live" })
                .AddMongoDb(mongodbConnectionString: configuration["Mongo:ConnectionString"],
                    name: "MongoDB",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new string[] { "mongoDB", "ready" });

            services.AddCxiMongoDb()
                .AddMongoDbApplicationInsightTelemetry("MongoDB.AuditLog")
                .AddMongoResiliencyFor<AuditLogService>(LoggerFactory.Create(builder => builder.AddApplicationInsights()).CreateLogger("mongobb-resilency"))
                .AddTransient<IAuditLogService, AuditLogService>();

            services.AddAuditLogClient(configuration);

            return services;
        }
    }
}