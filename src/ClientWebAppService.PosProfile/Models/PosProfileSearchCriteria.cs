using Microsoft.AspNetCore.Mvc;

namespace ClientWebAppService.PosProfile.Models
{
    /// <summary>
    /// Represents input parameters for Pos Profile search operation
    /// </summary>
    public class PosProfileSearchCriteria
    {
        [FromQuery(Name = "isHistoricalDataIngested")]
        public bool? IsHistoricalDataIngested { get; set; }
    }
}