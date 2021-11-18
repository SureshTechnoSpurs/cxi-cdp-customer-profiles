using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Business.Models
{
    /// <summary>
    /// DTO used as response in <see cref="IPartnerProfileService"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record PartnerProfileDto(
         [JsonProperty("partnerId")] string PartnerId,
         [JsonProperty("partnerName")] string? PartnerName,
         [JsonProperty("address")] string? Address,
         [JsonProperty("partnerType")] string? PartnerType,
         [JsonProperty("amountOfLocations")] int AmountOfLocations,
         [JsonProperty("userProfileEmails")] IEnumerable<string> UserProfileEmails)
    { }
}
