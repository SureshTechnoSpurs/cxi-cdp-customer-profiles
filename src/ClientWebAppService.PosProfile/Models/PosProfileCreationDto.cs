using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PosProfile.Models
{
    /// <summary>
    /// Represents DTO for creation POS Profile
    /// </summary>
    /// <param name="PartnerId"></param>
    /// <param name="PosConfigurations"></param>
    [ExcludeFromCodeCoverage]
    public record PosProfileCreationDto(string PartnerId, IEnumerable<PosCredentialsConfigurationDto> PosConfigurations, bool HistoricalDataIngested = false)
    {
    }
}