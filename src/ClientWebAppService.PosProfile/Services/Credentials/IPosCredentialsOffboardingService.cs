using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Services.Credentials
{
    public interface IPosCredentialsOffboardingService
    {
        /// <summary>
        ///     Process removing pos dependent configuration from dto
        ///     to PosCredentialsConfiguration db model, with remove sensitive information
        ///     from key-vault.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="posConfigurations"></param>
        /// <returns></returns>
        public Task OffboardingProcess(string partnerId);
    }
}