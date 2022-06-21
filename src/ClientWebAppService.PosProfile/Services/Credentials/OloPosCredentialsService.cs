using System.Threading.Tasks;
using Azure;
using ClientWebAppService.PosProfile.Models;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models.Create;
using CXI.Contracts.PosProfile.Models.PosKeyReference;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ClientWebAppService.PosProfile.Services.Credentials
{
    public class OloPosCredentialsService : IPosCredentialsService<PosCredentialsConfigurationOloCreationDto>, IPosCredentialsOffboardingService
    {
        private const string OloPosType = "olo";
        private const string SecretNotFoundErrorCode = "SecretNotFound";

        private readonly ISecretSetter _secretSetter;
        private readonly ILogger<OloPosCredentialsService> _logger;
        private readonly ISecretClient _secretClient;

        public OloPosCredentialsService(ISecretSetter secretSetter,
            ILogger<OloPosCredentialsService> logger,
            ISecretClient secretClient)
        {
            _secretSetter = secretSetter;
            _logger = logger;
            _secretClient = secretClient;
        }

        public Task<PosCredentialsConfiguration> OnboardingProcess(string partnerId, PosCredentialsConfigurationOloCreationDto posConfigurations)
        {
            var posConfigurationSecretName = SecretExtensions.GetPosConfigurationSecretName(partnerId, posConfigurations.PosType);

            // Saving empty model, this data will be filled from admin portal
            var keyReferenceModel = new OloKeyReferenceModel
            {
                ApiKey = string.Empty
            };

            var posConfigurationJsonSecret = JsonConvert.SerializeObject(keyReferenceModel);

            _secretSetter.Set(posConfigurationSecretName, posConfigurationJsonSecret, null);

            return Task.FromResult(new PosCredentialsConfiguration
            {
                    PosType = posConfigurations.PosType,
                    KeyVaultReference = posConfigurationSecretName
                });
        }

        public async Task OffboardingProcess(string partnerId)
        {
            try
            {
                var posConfigurationSecretName = SecretExtensions.GetPosConfigurationSecretName(partnerId, OloPosType);

                await _secretClient.DeleteSecretAsync(posConfigurationSecretName);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == SecretNotFoundErrorCode)
            {
                _logger.LogError(ex, $"Secret was not found in key vault for ${partnerId}");
                throw;
            }
        }
    }
}