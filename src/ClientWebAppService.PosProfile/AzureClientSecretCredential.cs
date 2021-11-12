using System.Diagnostics.CodeAnalysis;
using CXI.Common.Security.Secrets;

namespace ClientWebAppService.PosProfile
{
    /// <summary>
    /// Represents Azure Application Id Secret credentials for access
    /// protected resources
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AzureClientSecretCredential : ISecretConfiguration
    {
        public string? TenantId { get; set; }
        
        public string? ClientId { get; set; }
        
        public string? ClientSecret { get; set; }
        
        public string? SeviceUrl { get; set; }
    }
}