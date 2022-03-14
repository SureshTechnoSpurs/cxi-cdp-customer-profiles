using System;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PosProfile.Models
{
    /// <summary>
    /// Represents POS Profile credentials DTO
    /// </summary>
    /// <param name="PosType">POS data provider type, e.g. square</param>
    /// <param name="AccessToken">Access token for Data Source</param>
    /// <param name="RefreshToken">Refresh token for Data Source</param>
    /// <param name="ExpirationDate">Access token expiration date</param>
    /// <param name="MerchantId">The ID of the authorizing merchant's business</param>
    [ExcludeFromCodeCoverage]
    public record PosCredentialsConfigurationDto(
        string PosType, 
        string AccessToken, 
        string RefreshToken, 
        DateTime ExpirationDate, 
        string MerchantId)
    {
    }
}