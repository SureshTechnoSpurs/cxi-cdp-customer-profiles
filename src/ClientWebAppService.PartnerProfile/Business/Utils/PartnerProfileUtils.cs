using CXI.Common.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Business.Utils
{
    /// <summary>
    /// Utils that provides default values and generation patterns <see cref="DataAccess.Partner"/> related data.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PartnerProfileUtils
    {
        private const string PartnerIdPrefix = "cxi-usa-";

        public const string DefaultPartnerType = "Restaurant";

        public const string DefaultPartnerCountry = "USA";

        public static string GetPartnerIdByName(string partnerName) {
            var formatedPartnerName = partnerName.RemoveWhitespace();
            return $"{PartnerIdPrefix}{formatedPartnerName?.ToLower()}";
        } 
    }
}
