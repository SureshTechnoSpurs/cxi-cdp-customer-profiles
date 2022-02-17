using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.Core
{
    [ExcludeFromCodeCoverage]
    public class AdB2CInvitationOptions
    {
        public string InvitationClientId { get; set; }

        public string Domain { get; set; }

        public string Instance { get; set; }

        public string SignUpSignInPolicyId { get; set; }

        public string RedirectUrl { get; set; }

        public string TokenSecurityKey { get; set; }

        public string TokenIssuer { get; set; }

        public string TokenAudience { get; set; }        
    }   
}       
