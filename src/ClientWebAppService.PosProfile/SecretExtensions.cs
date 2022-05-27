using System.Diagnostics.CodeAnalysis;
using ClientWebAppService.PosProfile.Models;
using CXI.Contracts.PosProfile.Models.Create;
using Newtonsoft.Json;

namespace ClientWebAppService.PosProfile
{
    [ExcludeFromCodeCoverage]
    public static class SecretExtensions
    {
        public static string GetPosConfigurationSecretName(string partnerId, string posType)
        {
            return $"{partnerId}-{posType.ToLower()}";
        }

        public static (string keyVaultSecretName, string keyVaultSecretValue) ComposeSecretPayloadForDataCollectService(
            string posType,
            string partnerId,
            string accessToken,
            string authenticationScheme)
        {
            return ($"di-{posType}-{partnerId}-tokeninfo", $"{authenticationScheme} {accessToken}");
        }

        public static string ComposePosConfigurationSecretPayload(PosCredentialsConfigurationSquareCreationDto posCredentialsConfiguration)
        {
            var posProfileSecretConfiguration =
                new PosProfileSecretConfiguration(
                    new AccessToken(Value: posCredentialsConfiguration.AccessToken, posCredentialsConfiguration.ExpirationDate),
                    new RefreshToken(Value: posCredentialsConfiguration.RefreshToken, null));

            return JsonConvert.SerializeObject(posProfileSecretConfiguration);
        }
    }
}