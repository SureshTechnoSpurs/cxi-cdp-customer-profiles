using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Configuration
{
    [ExcludeFromCodeCoverage]
    public class DomainServicesConfiguration : IDomainServicesConfiguration
    {
        public PosProfileServiceConfiguration? PosProfileService { get; set; }
    }
}
