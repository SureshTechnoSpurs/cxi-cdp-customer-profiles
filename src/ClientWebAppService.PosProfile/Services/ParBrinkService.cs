using Azure.Security.KeyVault.Secrets;
using CXI.Common.Helpers;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models.PosKeyReference;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Services
{
    /// <inheritdoc cref="IParBrinkService"/> 
    public class ParBrinkService : IParBrinkService
    {
        private readonly IPosProfileService _posProfileService;
        private readonly ILogger<ParBrinkService> _logger;
        private readonly ISecretClient _secretClient;
        private const string PosType = "parbrink";

        /// <summary>
        /// Ctor for par brink service
        /// </summary>
        /// <param name="posProfileService"></param>
        /// <param name="logger"></param>
        /// <param name="secretClient"></param>
        public ParBrinkService(
            IPosProfileService posProfileService,
            ILogger<ParBrinkService> logger,
            ISecretClient secretClient)
        {
            _posProfileService = posProfileService;
            _logger = logger;
            _secretClient = secretClient;
        }

        /// <inheritdoc cref="IParBrinkService.GetParBrinkLocationsAsync(string)"/> 
        public async Task<ParBrinkKeyReferenceModel> GetParBrinkLocationsAsync(string partnerId)
        {
            _logger.LogInformation($"Getting Par Brink Locations for the Partner with partnerId: {partnerId}");

            VerifyHelper.NotNull(partnerId, nameof(partnerId));

            var parBrinkKeyReferenceModel = new ParBrinkKeyReferenceModel();
            var keyVaultReference = await GetParBrinkKeyVaultReference(partnerId);

            if (keyVaultReference != null)
            {
                KeyVaultSecret secret = _secretClient.GetSecret(keyVaultReference);
                if (secret != null)
                {
                    parBrinkKeyReferenceModel = JsonConvert.DeserializeObject<ParBrinkKeyReferenceModel>(secret.Value);
                }
            }

            _logger.LogInformation($"Successfully got the Par Brink Locations for the Partner with partnerId: {partnerId}");
            return parBrinkKeyReferenceModel;
        }

        /// <inheritdoc cref="IParBrinkService.SetParBrinkLocationsAsync(string, ParBrinkKeyReferenceModel)"/> 
        public async Task<bool> SetParBrinkLocationsAsync(string partnerId, ParBrinkKeyReferenceModel parBrinkKeyReferenceModel)
        {
            _logger.LogInformation($"Getting Par Brink Locations for the Partner with partnerId: {partnerId}");

            VerifyHelper.NotNull(partnerId, nameof(partnerId));

            var keyVaultReference = await GetParBrinkKeyVaultReference(partnerId);
            if (keyVaultReference != null && parBrinkKeyReferenceModel != null)
            {
                var secretValue = JsonConvert.SerializeObject(parBrinkKeyReferenceModel);
                _secretClient.SetSecret(keyVaultReference, secretValue);
            }

            _logger.LogInformation($"Successfully got the Par Brink Locations for the Partner with partnerId: {partnerId}");
            return true;
        }

        private async Task<string> GetParBrinkKeyVaultReference(string partnerId)
        {
            var posProfiles = await _posProfileService.GetPosProfileByPartnerId(partnerId);

            if (posProfiles != null && posProfiles.PosCredentialsConfigurations.Any())
            {
                var keyValutReference = posProfiles.PosCredentialsConfigurations.Where(x=> x.PosType == PosType).Select(x => x.KeyVaultReference).FirstOrDefault();
                return keyValutReference;
            }

            return null;
        }
    }
}