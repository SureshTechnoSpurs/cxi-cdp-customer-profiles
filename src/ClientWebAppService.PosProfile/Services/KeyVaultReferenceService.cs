using Azure.Security.KeyVault.Secrets;
using CXI.Common.Helpers;
using CXI.Common.Security.Secrets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Services
{
    /// <inheritdoc cref="IKeyVaultReferenceService"/> 
    public class KeyVaultReferenceService : IKeyVaultReferenceService
    {
        private readonly IPosProfileService _posProfileService;
        private readonly ILogger<KeyVaultReferenceService> _logger;
        private readonly ISecretClient _secretClient;

        /// <summary>
        /// Ctor for Key vault reference service
        /// </summary>
        /// <param name="posProfileService"></param>
        /// <param name="logger"></param>
        /// <param name="secretClient"></param>
        public KeyVaultReferenceService(
            IPosProfileService posProfileService,
            ILogger<KeyVaultReferenceService> logger,
            ISecretClient secretClient)
        {
            _posProfileService = posProfileService;
            _logger = logger;
            _secretClient = secretClient;
        }

        /// <inheritdoc cref="IKeyVaultReferenceService.GetKeyVaultValueByReferenceAsync<typeparam name="T"></typeparam>(string, string)"/> 
        public async Task<T> GetKeyVaultValueByReferenceAsync<T>(string partnerId, string posType) where T : new()
        {
            _logger.LogInformation($"Getting key vault value for the Partner with partnerId: {partnerId}");

            VerifyHelper.NotNull(partnerId, nameof(partnerId));

            var model = new T();

            var keyVaultReference = await GetKeyVaultReference(partnerId, posType);

            if (keyVaultReference != null)
            {
                KeyVaultSecret secret = _secretClient.GetSecret(keyVaultReference);
                if (secret != null)
                {
                    model = JsonConvert.DeserializeObject<T>(secret.Value);
                }
            }

            _logger.LogInformation($"Successfully got the key vault value for the Partner with partnerId: {partnerId}");
            return model;
        }

        /// <inheritdoc cref="IKeyVaultReferenceService.SetKeyVaultValueByReferenceAsync<typeparam name="T"></typeparam>(string, T)"/> 
        public async Task<bool> SetKeyVaultValueByReferenceAsync<T>(string partnerId, string posType, T model)
        {
            _logger.LogInformation($"Setting Key vault value for the Partner with partnerId: {partnerId}");

            var isSuccess = false;
            VerifyHelper.NotNull(partnerId, nameof(partnerId));

            var keyVaultReference = await GetKeyVaultReference(partnerId, posType);
            if (keyVaultReference != null && model != null)
            {
                var secretValue = JsonConvert.SerializeObject(model);
                _secretClient.SetSecret(keyVaultReference, secretValue);
                isSuccess = true;
                _logger.LogInformation($"Successfully updated the key vault value for the Partner with partnerId: {partnerId}");
            }
            return isSuccess;
        }

        private async Task<string> GetKeyVaultReference(string partnerId, string postype)
        {
            var posProfiles = await _posProfileService.GetPosProfileByPartnerId(partnerId);

            if (posProfiles != null && posProfiles.PosCredentialsConfigurations.Any())
            {
                var keyValutReference = posProfiles.PosCredentialsConfigurations.Where(x => x.PosType == postype).Select(x => x.KeyVaultReference).FirstOrDefault();
                return keyValutReference;
            }

            return null;
        }
    }
}