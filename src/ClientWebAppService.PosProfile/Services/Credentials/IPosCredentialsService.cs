using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Models;

namespace ClientWebAppService.PosProfile.Services.Credentials
{
    /// <summary>
    ///     Service for work with provided pos type credentials model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPosCredentialsService<in T>
    {
        /// <summary>
        ///     Process converting pos dependent configuration from create dto
        ///     to PosCredentialsConfiguration db model, with save sensitive information
        ///     to key-vault.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="posConfigurations"></param>
        /// <returns></returns>
        public Task<PosCredentialsConfiguration> Process(string partnerId, T posConfigurations);
    }
}