using System.Collections.Generic;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Models;
using CXI.Contracts.PosProfile.Models;

namespace ClientWebAppService.PosProfile.Services
{
    /// <summary>
    /// Contains get and create operations for POS Profiles
    /// </summary>
    public interface IPosProfileService
    {
        /// <summary>
        /// Creates POS Profile for particular partner as combination of record in MongoDB
        /// and a key vault record
        /// </summary>
        /// <param name="posProfileDto"></param>
        /// <returns></returns>
        Task<PosProfileDto> CreatePosProfileAsync(PosProfileCreationModel posProfileDto);

        /// <summary>
        /// Returns POS Profile by <paramref name="partnerId"/>\
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        Task<PosProfileDto> FindPosProfileByPartnerIdAsync(string partnerId);

        /// <summary>
        /// Fetches POS profiles specified by search criteria
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PosProfileSearchDto>> GetPosProfilesAsync(PosProfileSearchCriteriaModel searchCriteria);

        /// <summary>
        /// Performs update operation for POS Profile
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="updateModel"></param>
        /// <returns></returns>
        Task UpdatePosProfileAsync(string partnerId, PosProfileUpdateModel updateModel);
    }
}