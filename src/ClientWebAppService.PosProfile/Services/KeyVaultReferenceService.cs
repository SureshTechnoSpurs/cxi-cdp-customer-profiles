using Azure.Security.KeyVault.Secrets;
using CXI.Common.Helpers;
using CXI.Common.Security.Secrets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Models;
using CXI.Contracts.PosProfile.Models;
using CXI.Contracts.PosProfile.Models.PosKeyReference;
using System.Collections.Generic;
using ClientWebAppService.PosProfile.DataAccess;

namespace ClientWebAppService.PosProfile.Services
{
    /// <inheritdoc cref="IKeyVaultReferenceService"/> 
    public class KeyVaultReferenceService : IKeyVaultReferenceService
    {
        private readonly IPosProfileService _posProfileService;
        private readonly ILogger<KeyVaultReferenceService> _logger;
        private readonly ISecretClient _secretClient;
        private const string PosTypeParbrink = "parbrink";
        private const string Comma = ",";

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

            if (keyVaultReference == null)
            {
                return model;
            }

            if (posType.Equals(PosTypeParbrink))
            {
                model = GetParbrinkSecrectValue<T>(keyVaultReference);
            }
            else
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

        private T GetParbrinkSecrectValue<T>(string keyVaultReference) where T : new()
        {
            T? model;
            var locationKeyvault = new ParBrinkKeyReferenceModel();

            foreach (var secrectkey in keyVaultReference.Split(Comma))
            {
                var secret = _secretClient.GetSecret(secrectkey);
                if (secret == null || secret.Value == null)
                {
                    continue;
                }

                model = JsonConvert.DeserializeObject<T>(secret.Value);

                var groupLocations = (ParBrinkKeyReferenceModel)(object)model;

                locationKeyvault.Locations.AddRange(groupLocations.Locations);
            }

            model = (T)(object)locationKeyvault;
            return model;
        }

        /// <inheritdoc cref="IKeyVaultReferenceService.SetKeyVaultValueByReferenceAsync<typeparam name="T"></typeparam>(string, T)"/> 
        public async Task<bool> SetKeyVaultValueByReferenceAsync<T>(string partnerId, string posType, T model)
        {
            _logger.LogInformation($"Setting Key vault value for the Partner with partnerId: {partnerId}");

            var isSuccess = false;
            VerifyHelper.NotNull(partnerId, nameof(partnerId));

            if (posType.Equals(PosTypeParbrink))
            {

                return await SetKeyVaultValueForParbrink(partnerId, posType, model);
            }

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

        public async Task<bool> SetKeyVaultValueForParbrink<T>(string partnerId, string posType, T model)
        {
            _logger.LogInformation($"Setting Key vault value for the Parbrink Partner with partnerId: {partnerId}");

            var isSuccess = false;
            int length = 0;
            int max = 23000; // max 25 kb

            var keyVaultReference = await GetKeyVaultReference(partnerId, posType);

            if (keyVaultReference == null || model == null)
            {
                return isSuccess;
            }

            var keyvaultocations = (ParBrinkKeyReferenceModel)(object)model;
            var groupLocations = new List<ParBrinkKeyReferenceModel>();
            var groupLocation = new ParBrinkKeyReferenceModel();

            foreach (var location in keyvaultocations.Locations)
            {
                var locationText = JsonConvert.SerializeObject(location);

                if (locationText.Length + length > max)
                {
                    groupLocations.Add(groupLocation);
                    length = 0;
                    groupLocation = new ParBrinkKeyReferenceModel();
                }

                length = length + locationText.Length;
                groupLocation.Locations.Add(location);
            }

            groupLocations.Add(groupLocation);

            int keycount = 1;
            if (keyVaultReference.Length > 0)
            {
                keyVaultReference = keyVaultReference.Split(",")[0];
            }
            string secretKey = keyVaultReference;
            string secretKeys = "";
            foreach (var item in groupLocations)
            {
                var secretValue = JsonConvert.SerializeObject(item);
                secretKeys = keycount == 1 ? keyVaultReference : $"{secretKeys},{secretKey}";
                _secretClient.SetSecret(secretKey, secretValue);
                keycount = keycount + 1;
                secretKey = $"{keyVaultReference}{keycount}";
                isSuccess = true;
                _logger.LogInformation($"Successfully updated the key vault value for the Partner with partnerId: {partnerId}");
            }

            var updateModel = new PosProfileUpdateModel();
            var newPosConfigurations = new List<PosCredentialsConfigurationDto>();
            var existingPosProfile = await _posProfileService.FindPosProfileByPartnerIdAsync(partnerId);

            if (existingPosProfile != null)
            {
                foreach (var configVal in existingPosProfile.PosCredentialsConfigurations)
                {
                    if (configVal.PosType == posType)
                    {
                        newPosConfigurations.Add(new PosCredentialsConfigurationDto(posType, secretKeys, configVal.MerchantId));
                    }
                }
            }

            updateModel.PartnerId = partnerId;
            updateModel.PosConfigurations = newPosConfigurations;
            updateModel.IsHistoricalDataIngested = true;

            await _posProfileService.UpdatePosProfileAsync(partnerId, updateModel);

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