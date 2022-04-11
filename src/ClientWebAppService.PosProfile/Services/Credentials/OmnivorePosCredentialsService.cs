using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Models;
using CXI.Contracts.PosProfile.Models.Create;

namespace ClientWebAppService.PosProfile.Services.Credentials
{
    /// <summary>
    ///     Omnivore realization for pos credentials service
    /// </summary>
    public class OmnivorePosCredentialsService : IPosCredentialsService<PosCredentialsConfigurationOmnivoreCreationDto>
    {
        /// <summary>
        ///     Converting omnivore create credentials to PosCredentialsConfiguration
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="posConfigurations"></param>
        /// <returns></returns>
        public Task<PosCredentialsConfiguration> Process(string partnerId, PosCredentialsConfigurationOmnivoreCreationDto posConfigurations)
        {

            return Task.FromResult(new PosCredentialsConfiguration()
            {
                PosType = posConfigurations.PosType
            });
        }
    }
}