using System.Collections.Generic;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Models;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models.Create;
using CXI.Contracts.PosProfile.Models.PosKeyReference;
using Newtonsoft.Json;

namespace ClientWebAppService.PosProfile.Services.Credentials
{
    public class ParBrinkPosCredentialsService : IPosCredentialsService<PosCredentialsConfigurationParBrinkCreationDto>
    {
        private readonly ISecretSetter _secretSetter;

        public ParBrinkPosCredentialsService(ISecretSetter secretSetter)
        {
            _secretSetter = secretSetter;
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