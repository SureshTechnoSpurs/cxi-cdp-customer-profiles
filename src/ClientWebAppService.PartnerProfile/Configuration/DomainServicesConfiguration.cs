using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Configuration
{
    [ExcludeFromCodeCoverage]
    public class DomainServicesConfiguration : IDomainServicesConfiguration
    {
        public DownstreamServiceConfiguration? PosProfileServiceConfiguration { get; set; }
    }
}
