using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PosProfile.Configuration
{
    /// <summary>
    /// Represents configuration related to Partner profile microservice
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PartnerProfileServiceConfiguration
    {
        /// <summary>
        /// Base url for Partner profile microservice
        /// </summary>
        public string BaseUrl { get; set; }
    }
}
