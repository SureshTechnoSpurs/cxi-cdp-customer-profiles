using ClientWebAppService.PartnerProfile.Core;
using CXI.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile
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
                .AddAzureKeyVaultConfiguration(new[] { "ds-global", "ds-customer-profiles" })
                .AddAzureAppConfiguration(CommonServiceConstraints.AzureConfiguration.KeyPrefix)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
