using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace ClientWebAppService.PartnerProfile.Business.Models
{
    /// <summary>
    /// Represents contract for fetching active partners for specified POS type.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record PosTypePartnerDto(
        [property: JsonPropertyName("cxiPartnerId")] string PartnerId,
        [property: JsonPropertyName("country")] string Country,
        [property: JsonPropertyName("lakeMnemo")] string LakeMnemo)
    {
    }
}
