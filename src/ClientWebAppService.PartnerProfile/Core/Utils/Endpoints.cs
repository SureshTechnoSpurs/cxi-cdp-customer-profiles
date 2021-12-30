using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class Endpoints
    {
        public static class PosProfileService
        {
            private static readonly string GetActivePartnersByPosTypeRoutePattern = "{0}/api/" + "postype" + "/{1}";
            public static string GetActivePartnersByPosTypeEndpoint(string? baseUrl, string partnerId) =>
                string.Format(GetActivePartnersByPosTypeRoutePattern, baseUrl, partnerId);
        }
    }
}
