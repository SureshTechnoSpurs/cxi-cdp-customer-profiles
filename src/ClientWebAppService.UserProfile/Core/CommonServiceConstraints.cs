using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.Core
{
    /// <summary>
    /// /Common constrains around the service.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CommonServiceConstraints
    {
        public static class AzureConfiguration
        {
            public const string KeyPrefix = "ClientWebAppServiceUserProfile:";
        }
    }
}
