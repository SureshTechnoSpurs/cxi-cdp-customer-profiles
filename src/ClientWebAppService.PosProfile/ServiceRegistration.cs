using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Azure.Core;
using Azure.Identity;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Services;
using CXI.Common.Authorization;
using CXI.Common.MongoDb.Extensions;
using CXI.Common.Security.Secrets;
using FluentValidation.AspNetCore;
using GL.MSA.Core.HealthCheck.Concrete;
using GL.MSA.Core.HealthCheck.HealthCheckExtensions;
using GL.MSA.Tracing.TraceFactory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace ClientWebAppService.PosProfile
{
    [ExcludeFromCodeCoverage]
    public static class ServiceRegistration
    {
        public static IServiceCollection RegisterService(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            services.AddAzureAdB2CUserAuthentication(configuration, logger);
            services.AddAzureAdB2CMachineToMachineAuthentication(configuration, logger);
            services.AddM2MAuthorization("domainservice_readwrite", Constants.M2MPolicy);

            services.AddTraceExtentionDispatcher(configuration);

            services.AddControllers();
            services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddSingleton<TokenCredential>(_ => new ClientSecretCredential(
                tenantId: configuration["appidentity:tenantid"],
                clientId: configuration["appidentity:clientid"],
                clientSecret: configuration["appidentity:clientsecret"]));

            services.AddHealthChecks()
                .AddCheck<LivenessHealthCheck>(name: "live",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "live" })
                .AddMongoDb(mongodbConnectionString: configuration["Mongo:ConnectionString"],
                    name: "MongoDB",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new string[] { "mongoDB", "ready" });
            
            
            services.AddCxiMongoDb<PosProfileMongoClientProvider>()
                .AddMongoResiliencyFor<Models.PosProfile>(LoggerFactory.Create(builder => builder.AddApplicationInsights())
                    .CreateLogger("mongodb-resiliency"))
                .AddTransient<IPosProfileRepository, PosProfileRepository>()
                .AddTransient<IPosProfileService, PosProfileService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "ClientWebAppService.BFF", Version = "v1"});
            });

            var azureCredential = configuration.GetSection(nameof(AzureClientSecretCredential)).Get<AzureClientSecretCredential>();
            services.AddSingleton<ISecretConfiguration>(azureCredential);
            services.AddSingleton<ISecretClient, AzureKeyVaultSecretClient>();
            services.AddSingleton<ISecretSetter, AzureKeyVaultSecretSetter>();
            
            return services;
        }
    }
}