using CXI.Common.ExceptionHandling;
using GL.MSA.Core.HealthCheck.Concrete;
using GL.MSA.Core.HealthCheck.HealthCheckExtensions;
using GL.MSA.Core.HealthCheck.HealthCheckExtentions;
using GL.MSA.Tracing.TraceFactory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using CXI.Common.MongoDb.Extensions;
using System;
using ClientWebAppService.UserProfile.DataAccess;
using System.Diagnostics.CodeAnalysis;
using ClientWebAppService.UserProfile.Business;
using Microsoft.Extensions.Logging;
using FluentValidation.AspNetCore;

namespace ClientWebAppService.UserProfile
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(
                         configureJwtBearerOptions:
                             options =>
                             {
                                 Configuration.Bind("AzureAdB2C", options);
                                 options.TokenValidationParameters.NameClaimType = "name";
                             },
                        configureMicrosoftIdentityOptions:
                             options => { Configuration.Bind("AzureAdB2C", options); });

            services.AddControllers()
                    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

            services.AddTraceExtentionDispatcher(Configuration)
                    .AddHealthChecks()
                    .AddCheck<LivenessHealthCheck>(name: "live",
                                                 failureStatus: HealthStatus.Unhealthy,
                                                 tags: new[] { "live" })
                  .AddMongoDb(mongodbConnectionString: Configuration["Mongo:ConnectionString"],
                              name: "MongoDB",
                              failureStatus: HealthStatus.Unhealthy,
                              tags: new string[] { "mongoDB", "ready" });

            services.AddCxiMongoDb<UserProfileMongoClientProvider>()
                    .AddMongoResiliencyFor<User>(LoggerFactory.Create(builder => builder.AddApplicationInsights()).CreateLogger("mongobb-resilency"))
                    .AddTransient<IUserProfileRepository, UserProfileRepository>()
                    .AddTransient<IUserProfileService, UserProfileService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ClientWebAppService.UserProfile", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClientWebAppService.UserProfile v1"));
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHealthCheck();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
