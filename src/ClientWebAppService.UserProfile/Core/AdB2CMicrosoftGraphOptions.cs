using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.Core
{
    [ExcludeFromCodeCoverage]
    public class AdB2CMicrosoftGraphOptions
    {
        public string MicrosoftGraphAppId { get; set; }

        public string Domain { get; set; }

        public string MicrosoftGraphAppClientSercret { get; set; }
    }
}
