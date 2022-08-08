using CXI.Common.Extensions;
using CXI.Common.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

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
        private const int HashLength = 7;

        public static string GetPartnerIdByName(string partnerName) 
        {
            var formatedPartnerName = partnerName.RemoveWhitespace();
            var hash = ComputeHash(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));

            return $"{PartnerIdPrefix}{formatedPartnerName?.ToLower()}#{hash}";
        }

        private static string ComputeHash(string value)
        {
            var hashUtil = new HashUtils(HashAlgorithmType.SHA1, Encoding.UTF8);
            var hash = hashUtil.GetHashData(value);

            Regex regEx = new Regex("[^a-zA-Z0-9 -]");
            var replacement = regEx.Replace(hash, "");

            if (replacement.Length > HashLength)
                return replacement.Substring(0, HashLength).ToLower();
            else
                return replacement.ToLower();
        }
    }
}
