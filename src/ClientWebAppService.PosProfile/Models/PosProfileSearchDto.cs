using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PosProfile.Models
{
    /// <summary>
    /// Represents DTO for pos profile search response
    /// </summary>
    /// <param name="PartnerId"></param>
    /// <param name="PosTypes"></param>
    [ExcludeFromCodeCoverage]
    public record PosProfileSearchDto(string? PartnerId, IEnumerable<string> PosTypes,  bool IsHistoricalDataIngested, int HistoricalIngestDaysPeriod)
    {
    }
}