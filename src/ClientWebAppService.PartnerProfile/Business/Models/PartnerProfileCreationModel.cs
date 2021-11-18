using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Business.Models
{
    /// <summary>
    /// DTO used for partner profile creation <see cref="IPartnerProfileService.CreateProfileAsync(PartnerProfileCreationModel)"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record PartnerProfileCreationModel(
        [JsonProperty("address")] string Address,
        [JsonProperty("name")] string Name)
    {
    }
}
