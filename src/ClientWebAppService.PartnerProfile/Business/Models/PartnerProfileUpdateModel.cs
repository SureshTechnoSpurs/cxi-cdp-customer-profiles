using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Models
{
    /// <summary>
    /// DTO used for partner profile update <see cref="Business.IPartnerProfileService.UpdateProfileAsync(string, PartnerProfileUpdateModel)"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record PartnerProfileUpdateModel(
        [JsonProperty("partnerName")] string? PartnerName,
        [JsonProperty("address")] string? Address,
        [JsonProperty("amountOfLocations")] int AmountOfLocations,
        [JsonProperty("partnerType")] string? PartnerType,
        [JsonProperty("userProfileEmails")] IEnumerable<string> UserProfileEmails)
    { }
}
