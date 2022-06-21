using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Services
{
    /// <summary>
    /// Provide business flows around getting key value from DB and get or set key vault.
    /// </summary>
    public interface IKeyVaultReferenceService
    {
        /// <summary>
        /// Get key vault value for a partners based on pos types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partnerId"></param>
        /// <param name="posType"></param>
        /// <returns></returns>
        Task<T> GetKeyVaultValueByReferenceAsync<T>(string partnerId, string posType) where T : new();

        /// <summary>
        /// Set key vault value for a partners based on pos types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="partnerId"></param>
        /// <param name="posType"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> SetKeyVaultValueByReferenceAsync<T>(string partnerId, string posType, T model);
    }
}