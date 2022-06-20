using CXI.Contracts.PosProfile.Models.PosKeyReference;
using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Services
{
    /// <summary>
    /// Provide business flows around Par Brink.
    /// </summary>
    public interface IParBrinkService
    {
        /// <summary>
        /// Get location of the partners based on pos types
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        Task<ParBrinkKeyReferenceModel> GetParBrinkLocationsAsync(string partnerId);

        /// <summary>
        /// Set location of the partners based on pos types
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="parBrinkKeyReferenceModel"></param>
        /// <returns></returns>
        Task<bool> SetParBrinkLocationsAsync(string partnerId, ParBrinkKeyReferenceModel parBrinkKeyReferenceModel);
    }
}