using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PosProfile.Configuration
{
    /// <summary>
    /// Configuration for domain services
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DomainServicesConfiguration
    {
        /// <summary>
        /// PartnerProfileServiceConfiguration
        /// </summary>
        public PartnerProfileServiceConfiguration PartnerProfileServiceConfiguration { get; set; }
    }
}
