using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Azure.Core;
using Azure.Identity;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Services;
using ClientWebAppService.PosProfile.Services.Credentials;
using CXI.Common.Authorization;
using CXI.Common.MongoDb.Extensions;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models.Create;
using FluentValidation.AspNetCore;
using GL.MSA.Core.HealthCheck.Concrete;
using GL.MSA.Core.HealthCheck.HealthCheckExtensions;
using CXI.Common.ApplicationInsights;
using Microsoft.AspNetCore.Mvc.Versioning;
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
                .AddMongoDb(configuration["Mongo:ConnectionString"],
                    name: "MongoDB",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new string[] { "mongoDB", "ready" });
            
            
            services.AddCxiMongoDb()
                    .AddMongoDbApplicationInsightTelemetry("MongoDB.PosProfile")
                    .AddMongoResiliencyFor<Models.PosProfile>(LoggerFactory.Create(builder => builder.AddApplicationInsights()).CreateLogger("mongobb-resilency"))
                .AddTransient<IPosProfileRepository, PosProfileRepository>()
                .AddTransient<IPosProfileService, PosProfileService>()
                .AddTransient<IPosTypeService, PosTypeService>()
                .AddTransient<IParBrinkService, ParBrinkService>();

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = Microsoft.AspNetCore.Mvc.ApiVersion.Default;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("api-version", "version"),
                    new QueryStringApiVersionReader("api-version", "version")
                );
                options.ReportApiVersions = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "ClientWebAppService.PosProfile", Version = "v1"});
            });

            var azureCredential = configuration.GetSection(nameof(AzureClientSecretCredential)).Get<AzureClientSecretCredential>();
            services.AddSingleton<ISecretConfiguration>(azureCredential);
            services.AddSingleton<ISecretClient, AzureKeyVaultSecretClient>();
            services.AddSingleton<ISecretSetter, AzureKeyVaultSecretSetter>();

            services.AddTransient<IPosCredentialsServiceResolver, PosCredentialsServiceResolver>();
            services.AddTransient<IPosCredentialsService<PosCredentialsConfigurationSquareCreationDto>, SquarePosCredentialsService>();
            services.AddTransient<IPosCredentialsService<PosCredentialsConfigurationOmnivoreCreationDto>, OmnivorePosCredentialsService>();
            services.AddTransient<IPosCredentialsService<PosCredentialsConfigurationParBrinkCreationDto>, ParBrinkPosCredentialsService>();
            services.AddTransient<IPosCredentialsService<PosCredentialsConfigurationOloCreationDto>, OloPosCredentialsService>();

            return services;
        }
    }
}