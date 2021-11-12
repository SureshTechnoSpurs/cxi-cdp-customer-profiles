using System;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PosProfile.Models
{
    /// <summary>
    /// Represents sensitive part of POS Profile DTO object, 
    /// </summary>
    /// <param name="AccessToken"></param>
    /// <param name="RefreshToken"></param>
    [ExcludeFromCodeCoverage]
    public record PosProfileSecretConfiguration(AccessToken AccessToken, RefreshToken RefreshToken)
    {
    }
    
    public record AccessToken(string Value, DateTime? ExpirationDate)
    {}
    
    public record RefreshToken(string Value, DateTime? ExpirationDate)
    {}
}