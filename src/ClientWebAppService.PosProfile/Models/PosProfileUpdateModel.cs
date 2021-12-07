using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace ClientWebAppService.PosProfile.Models
{
    /// <summary>
    /// Contains fields which are allowed to update for POS Profile entity
    /// </summary>
    /// <param name="PosConfigurations"></param>
    /// <param name="IsHistoricalDataIngested"></param>
    [ExcludeFromCodeCoverage]
    public record PosProfileUpdateModel([JsonProperty("posConfigurations")]IEnumerable<PosCredentialsConfiguration>? PosConfigurations,
        [JsonProperty("isHistoricalDataIngested")]bool IsHistoricalDataIngested)
    {

    }
}