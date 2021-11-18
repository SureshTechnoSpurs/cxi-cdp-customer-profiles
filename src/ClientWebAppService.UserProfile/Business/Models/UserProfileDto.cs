using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.Business.Models
{
    /// <summary>
    /// DTO used in <see cref="IUserProfileService"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record UserProfileDto(
        [JsonProperty("partnerId")] string PartnerId,
        [JsonProperty("email")] string Email,
        [JsonProperty("role")] string Role)
    { }
}