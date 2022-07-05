using Azure;
using ClientWebAppService.PosProfile.Models;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models.Create;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Services.Credentials
{
    /// <summary>
    /// ToastPosCredentialsService
    /// </summary>
    public class ToastPosCredentialsService : 
        IPosCredentialsService<PosCredentialsConfigurationToastCreationDto>, IPosCredentialsOffboardingService
    {
        private readonly string PartnerCodeSecretNameTemplate = "{0}-toast";
        private readonly string PartnerCodeTemporarySecretNameTemplate = "temp-{0}-toast";
        private const string ToastPosType = "toast";
        private const string SecretNotFoundErrorCode = "SecretNotFound";

        private readonly ISecretSetter _secretSetter;
        private readonly ILogger<ToastPosCredentialsService> _logger;
        private readonly ISecretClient _secretClient;

        public ToastPosCredentialsService(ISecretSetter secretSetter,
            ILogger<ToastPosCredentialsService> logger,
            ISecretClient secretClient)
        {
            _secretSetter = secretSetter;
            _logger = logger;
            _secretClient = secretClient;
        }

        /// <inheritdoc cref="OnboardingProcess(string, PosCredentialsConfigurationToastCreationDto)"/>
        public async Task<PosCredentialsConfiguration> OnboardingProcess(
            string partnerId, 
            PosCredentialsConfigurationToastCreationDto posConfiguration)
        {
            var posConfigurationSecretName = string.Format(PartnerCodeSecretNameTemplate, partnerId);

            _secretSetter.Set(posConfigurationSecretName, posConfiguration.PartnerCode, null);

            var tempSecretName = string.Format(PartnerCodeTemporarySecretNameTemplate, partnerId);
            await _secretClient.DeleteSecretAsync(tempSecretName);

            return new PosCredentialsConfiguration
            {
                PosType = posConfiguration.PosType,
                KeyVaultReference = posConfigurationSecretName
            };
        }

        /// <inheritdoc cref="OffboardingProcess(string)"/>
        public async Task OffboardingProcess(string partnerId)
        {
            try
            {
                var posConfigurationSecretName = SecretExtensions.GetPosConfigurationSecretName(partnerId, ToastPosType);

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