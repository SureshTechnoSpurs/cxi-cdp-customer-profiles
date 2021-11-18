using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Business.Utils
{
    /// <summary>
    /// Utils that provides default values and generation patterns <see cref="DataAccess.Partner"/> related data.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PartnerProfileUtils
    {
        private const string PartnerIdPrefix = "cxi-usa-hospitality-";

        public const string DefaultPartnerType = "Restaurant";

        public static string GetPartnerIdByName(string partnerName) => $"{PartnerIdPrefix}{partnerName}";
    }
}
