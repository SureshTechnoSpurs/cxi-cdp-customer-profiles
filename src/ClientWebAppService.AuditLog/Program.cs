using ClientWebAppService.AuditLog.Core;
using CXI.Common.Extensions;
using CXI.Common.Security.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.AuditLog
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddAzureAppConfiguration(CommonServiceConstraints.AzureConfiguration.KeyPrefix)
                .AddAzureKeyVaultConfiguration(new[] { "ds-global", "ds-customer-profiles", "ds-auditlog" })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
