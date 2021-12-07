using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

        public static string GetPartnerIdByName(string partnerName) {
            var formatedPartnerName = string.Concat(partnerName.Where(c => !char.IsWhiteSpace(c)));
            return $"{PartnerIdPrefix}{formatedPartnerName.ToLower()}";
        } 
    }
}
