using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.Business.Models
{
    /// <summary>
    /// DTO used for profile creation in <see cref="IUserProfileService.CreateProfileAsync(UserCreationModel)"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserCreationModel
    {
        [JsonProperty("partnerId")]
        public string PartnerId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }
}
