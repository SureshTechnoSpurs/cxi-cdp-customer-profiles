using System.Diagnostics.CodeAnalysis;
using ClientWebAppService.PartnerProfile.Business;
using ClientWebAppService.PartnerProfile.Configuration;
using ClientWebAppService.PartnerProfile.DataAccess;
using CXI.Common.Authorization;
using CXI.Common.MongoDb.Extensions;
using CXI.Contracts.PosProfile;
using GL.MSA.Core.HealthCheck.Concrete;
using GL.MSA.Core.HealthCheck.HealthCheckExtensions;
using CXI.Common.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace ClientWebAppService.PartnerProfile.Core.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterService(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            services.AddAzureAdB2CUserAuthentication(configuration, logger);
            services.AddAzureAdB2CMachineToMachineAuthentication(configuration, logger);

            services.AddM2MAuthorization("domainservice_readwrite", Constants.M2MPolicy);
            
            var serviceConfigs = configuration.GetSection("DomainServices").Get<DomainServicesConfiguration>();

            services.AddTransient<IDomainServicesConfiguration>(_ => serviceConfigs);

            services.AddPosProfileServiceClient(serviceConfigs.PosProfileServiceConfiguration.BaseUrl)
                .WithHttpContextAuthorizationTokenResolver();

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
                .AddMongoDbApplicationInsightTelemetry("MongoDB.PartnerProfile")
                .AddMongoResiliencyFor<Partner>(LoggerFactory.Create(builder => builder.AddApplicationInsights()).CreateLogger("mongobb-resilency"))
                .AddTransient<IPartnerRepository, PartnerRepository>()
                .AddTransient<IPartnerProfileService, PartnerProfileService>();

            return services;
        }
    }
}