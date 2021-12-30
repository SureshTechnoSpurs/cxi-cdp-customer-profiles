namespace ClientWebAppService.PartnerProfile.Configuration
{
    public interface IDomainServicesConfiguration
    {
        PosProfileServiceConfiguration? PosProfileService { get; set; }
    }
}
