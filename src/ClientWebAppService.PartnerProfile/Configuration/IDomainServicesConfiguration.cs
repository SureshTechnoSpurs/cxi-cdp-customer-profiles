namespace ClientWebAppService.PartnerProfile.Configuration
{
    public interface IDomainServicesConfiguration
    {
        DownstreamServiceConfiguration? PosProfileServiceConfiguration { get; set; }
    }
}
