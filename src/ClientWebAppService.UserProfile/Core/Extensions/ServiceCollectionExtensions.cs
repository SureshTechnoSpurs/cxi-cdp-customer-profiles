using CXI.Common.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GL.MSA.Core.HealthCheck.Concrete;
using GL.MSA.Core.HealthCheck.HealthCheckExtensions;
using CXI.Common.ApplicationInsights;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using CXI.Common.MongoDb.Extensions;
using ClientWebAppService.UserProfile.DataAccess;
using ClientWebAppService.UserProfile.Business;
using FluentValidation.AspNetCore;
using CXI.Common.MessageBrokers.Extentions;

namespace ClientWebAppService.UserProfile.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterService(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            services.AddAzureAdB2CUserAuthentication(configuration, logger);
            services.AddAzureAdB2CMachineToMachineAuthentication(configuration, logger);

            services.AddM2MAuthorization("domainservice_readwrite", Constants.M2MPolicy);

            services.Configure<AdB2CInvitationOptions>(configuration.GetSection("AzureAdB2C"));
            services.Configure<AdB2CMicrosoftGraphOptions>(configuration.GetSection("AzureAdB2C"));

            services.AddControllers()
                    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

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
                    .AddMongoDbApplicationInsightTelemetry("MongoDB.UserProfile")
                    .AddMongoResiliencyFor<User>(LoggerFactory.Create(builder => builder.AddApplicationInsights()).CreateLogger("mongobb-resilency"))
                    .AddTransient<IUserProfileRepository, UserProfileRepository>()
                    .AddTransient<IUserProfileService, UserProfileService>()
                    .AddTransient<IEmailService, EmailService>()
                    .AddTransient<IAzureADB2CDirectoryManager, AzureADB2CDirectoryManager>()
                    .AddTransient<IAzureADB2CDirectoryRepository, AzureADB2CDirectoryRepository>();

            services.AddProducer(configuration);


            return services;
        }
    }
}
