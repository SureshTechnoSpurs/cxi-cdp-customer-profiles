using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Models;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models.Create;
using CXI.Contracts.PosProfile.Models.PosKeyReference;
using Newtonsoft.Json;

namespace ClientWebAppService.PosProfile.Services.Credentials
{
    /// <summary>
    ///     Omnivore realization for pos credentials service
    /// </summary>
    public class OmnivorePosCredentialsService : IPosCredentialsService<PosCredentialsConfigurationOmnivoreCreationDto>
    {
        private readonly ISecretSetter _secretSetter;

        public OmnivorePosCredentialsService(ISecretSetter secretSetter)
        {
            _secretSetter = secretSetter;
        }

        /// <summary>
        ///     Converting omnivore create credentials to PosCredentialsConfiguration
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="posConfigurations"></param>
        /// <returns></returns>
        public Task<PosCredentialsConfiguration> Process(string partnerId, PosCredentialsConfigurationOmnivoreCreationDto posConfigurations)
        {
            var posConfigurationSecretName = SecretExtensions.GetPosConfigurationSecretName(partnerId, posConfigurations.PosType);
            var keyReferenceModel = new OmnivoreKeyReferenceModel()
            {
                Locations = posConfigurations.Locations
            };
            var posConfigurationJsonSecret = JsonConvert.SerializeObject(keyReferenceModel);

            _secretSetter.Set(posConfigurationSecretName, posConfigurationJsonSecret, null);

            return Task.FromResult(new PosCredentialsConfiguration()
            {
                PosType = posConfigurations.PosType,
                KeyVaultReference = posConfigurationSecretName
            });
        }
    }
}