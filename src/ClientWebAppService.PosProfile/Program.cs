using System.Diagnostics.CodeAnalysis;
using CXI.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ClientWebAppService.PosProfile
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
                .AddAzureAppConfiguration("ClientWebAppServicePosProfile:")
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
