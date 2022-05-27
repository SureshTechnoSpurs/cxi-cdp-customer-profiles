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
    /// <summary>
    ///     Omnivore realization for pos credentials service
    /// </summary>
    public class OmnivorePosCredentialsService : IPosCredentialsService<PosCredentialsConfigurationOmnivoreCreationDto>,
        IPosCredentialsOffboardingService
    {
        private const string SecretNotFoundErrorCode = "SecretNotFound";
        private const string OmnivorePosType = "omnivore";
        private readonly ISecretSetter _secretSetter;
        private readonly ISecretClient _secretClient;
        private readonly ILogger<OmnivorePosCredentialsService> _logger;

        public OmnivorePosCredentialsService(ISecretSetter secretSetter, ISecretClient secretClient, ILogger<OmnivorePosCredentialsService> logger)
        {
            _secretSetter = secretSetter;
            _secretClient = secretClient;
            _logger = logger;
        }

        public async Task OffboardingProcess(string partnerId)
        {
            try
            {
                var posConfigurationSecretName = SecretExtensions.GetPosConfigurationSecretName(partnerId, OmnivorePosType);
                await _secretClient.DeleteSecretAsync(posConfigurationSecretName);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == SecretNotFoundErrorCode)
            {
                _logger.LogError(ex, $"Secret was not found in key vault for ${partnerId}");
                throw;
            }
        }

        /// <summary>
        ///     Converting omnivore create credentials to PosCredentialsConfiguration
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="posConfigurations"></param>
        /// <returns></returns>
        public Task<PosCredentialsConfiguration> OnboardingProcess(string partnerId, PosCredentialsConfigurationOmnivoreCreationDto posConfigurations)
        {
            var posConfigurationSecretName = SecretExtensions.GetPosConfigurationSecretName(partnerId, posConfigurations.PosType);
            var keyReferenceModel = new OmnivoreKeyReferenceModel()
            {
                Locations = posConfigurations.Locations
            };
            var posConfigurationJsonSecret = JsonConvert.SerializeObject(keyReferenceModel);

            _secretSetter.Set(posConfigurationSecretName, posConfigurationJsonSecret, null);

            return Task.FromResult(
                new PosCredentialsConfiguration()
                {
                    PosType = posConfigurations.PosType,
                    KeyVaultReference = posConfigurationSecretName
                });
        }
    }
}