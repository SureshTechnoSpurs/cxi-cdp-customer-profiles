using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace ClientWebAppService.PosProfile
{
    [ExcludeFromCodeCoverage]
    public static class ConfigurationExtensions
    {
        public static string GetDefaultHistoricalIngestPeriod(this IConfiguration configuration)
        {
            return configuration["DefaultHistoricalIngestPeriod"];
        }

    }
}