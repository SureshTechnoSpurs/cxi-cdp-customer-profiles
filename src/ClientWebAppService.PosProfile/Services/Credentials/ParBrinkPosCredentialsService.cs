using System.Collections.Generic;
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
    public class ParBrinkPosCredentialsService : IPosCredentialsService<PosCredentialsConfigurationParBrinkCreationDto>, IPosCredentialsOffboardingService
    {
        private const string SecretNotFoundErrorCode = "SecretNotFound";
        private const string OmnivorePosType = "parbrink";

        private readonly ISecretSetter _secretSetter;
        private readonly ISecretClient _secretClient;
        private readonly ILogger<ParBrinkPosCredentialsService> _logger;

        public ParBrinkPosCredentialsService(ISecretSetter secretSetter, ISecretClient secretClient, ILogger<ParBrinkPosCredentialsService> logger)
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

        public Task<PosCredentialsConfiguration> OnboardingProcess(string partnerId, PosCredentialsConfigurationParBrinkCreationDto posConfigurations)
        {
            var posConfigurationSecretName = SecretExtensions.GetPosConfigurationSecretName(partnerId, posConfigurations.PosType);

            //Saving empty model, this data will be filled from admin portal
            var keyReferenceModel = new ParBrinkKeyReferenceModel()
            {
                Locations = new List<ParBrinkLocationConfiguration>()
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