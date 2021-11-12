using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PosProfile.Models
{
    /// <summary>
    /// Represents DTO for get partner profile operation
    /// </summary>
    /// <param name="PartnerId"></param>
    /// <param name="PosConfigurations"></param>
    [ExcludeFromCodeCoverage]
    public record PosProfileDto(string? PartnerId, IEnumerable<PosCredentialsConfiguration>? PosConfigurations)
    {
    }
}
